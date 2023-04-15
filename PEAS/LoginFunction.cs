using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using PEAS.Pocos;
using System.Collections.Generic;
using PEAS.Entities;
using System.Linq;

namespace PEAS
{
    public static class LoginFunction
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Table(StaticDefinitions.ApplicationTableName, Connection = "AzureWebJobsStorage")] TableClient tableClientApplications,
            [Table(StaticDefinitions.UserTableName, Connection = "AzureWebJobsStorage")] TableClient tableClientUsers,
            ILogger log)
        {
            var response = new LoginRequestResponse() { Success = false};

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var loginRequest = JsonConvert.DeserializeObject<LoginRequestPoco>(requestBody);

            if(loginRequest == null || string.IsNullOrEmpty(loginRequest.EMail) || string.IsNullOrEmpty(loginRequest.AppToken))
            {
                return new BadRequestResult();
            }

            var app = tableClientApplications.Query<ApplicationTableEntity>().Where(i => i.AppToken == loginRequest.AppToken).FirstOrDefault();

            if (app == null)
            {
                return new NotFoundResult();
            }

            var hashedEmail = Helper.HashEmail(loginRequest.EMail);
            var user = tableClientUsers.Query<UserTableEntity>().Where(ute => ute.HashedEmail == hashedEmail).FirstOrDefault();

            if(user == null)
            {
                return new NotFoundResult();
            }

            if (!string.IsNullOrEmpty(loginRequest.OTP) && !string.IsNullOrEmpty(loginRequest.ReqId))
            {
                if(string.IsNullOrEmpty(user.OTP))
                {
                    return new BadRequestResult();
                }
                var timeSinceAttempt = DateTime.UtcNow - user.LastLoginAttempt;
                if(timeSinceAttempt.Value.TotalMinutes >= 7)
                {
                    response.Message = "OTP expired. Please try again.";
                    var obj = new ObjectResult(JsonConvert.SerializeObject(response));
                    obj.StatusCode = StatusCodes.Status408RequestTimeout;
                    log.LogInformation($"Expired OTP for {loginRequest.EMail}");
                    return obj;
                }
                if(user.OTP == loginRequest.OTP)
                {
                    user.OTP = "";
                    user.LastSuccessFullLogin = DateTime.UtcNow;
                    tableClientUsers.UpdateEntity<UserTableEntity>(user, user.ETag);

                    var token = Helper.GetJwtToken(app.SharedSecret, user.Domain, user.Group, loginRequest.EMail);

                    log.LogInformation($"Successfull login : {loginRequest.EMail}");

                    response.Message = "Login successfull.";
                    response.Success = true;
                    response.Token = token;
                    response.RedirectUrl = $"{app.RedirectUrl}?auth={token}&reqid={loginRequest.ReqId}";
                    return new OkObjectResult(JsonConvert.SerializeObject(response));
                }
                else
                {
                    log.LogInformation($"Invalid OTP for {loginRequest.EMail}");
                    response.Message = "Invalid OTP";
                }
            }
            else
            {
                Random rnd = new Random();
                var otp = rnd.Next(100000, 999999).ToString();

                user.OTP = otp;
                user.LastLoginAttempt = DateTime.UtcNow;

                tableClientUsers.UpdateEntity<UserTableEntity>(user, user.ETag);

                Helper.SendOTPEMail(loginRequest.EMail, otp);

                log.LogInformation($"New login attempt: {loginRequest.EMail}");

                response.Success = true;
                response.Message = "Sent e-mail with one time password.";

            }

            return new OkObjectResult(JsonConvert.SerializeObject(response));
        }
    }
}
