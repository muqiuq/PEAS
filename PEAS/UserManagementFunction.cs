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
using PEAS.Entities;

namespace PEAS
{
    public static class UserManagementFunction
    {
        [FunctionName("adduser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table(StaticDefinitions.UserTableName, Connection = "AzureWebJobsStorage")] TableClient tableClientUsers,
            ILogger log)
        {
            var response = new DefaultResponse() { Success = false };

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newUserRequest = JsonConvert.DeserializeObject<NewUserRequest>(requestBody);

            if(newUserRequest == null || string.IsNullOrEmpty(newUserRequest.EMail) || string.IsNullOrEmpty(newUserRequest.Group) || string.IsNullOrEmpty(newUserRequest.Domain))
            {
                return new BadRequestObjectResult("Invalid request");
            }

            tableClientUsers.AddEntity<UserTableEntity>(new UserTableEntity()
            {
                CanCreateApplication = false,
                Domain = newUserRequest.Domain,
                Enabled = true,
                HashedEmail = Helper.HashEmail(newUserRequest.EMail),
                OTP = null,
                Group = newUserRequest.Group,
                PartitionKey = StaticDefinitions.UserTableName,
                RowKey = Guid.NewGuid().ToString()
            });

            response.Success = true;

            return new OkObjectResult(JsonConvert.SerializeObject(response));
        }
    }
}
