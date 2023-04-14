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
using PEAS.Entities;
using PEAS.Pocos;

namespace PEAS
{
    public static class AppManagementFunction
    {
        [FunctionName("addapp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table(StaticDefinitions.ApplicationTableName, Connection = "AzureWebJobsStorage")] TableClient tableClientApplications,
            ILogger log)
        {
            var response = new DefaultResponse() { Success = false };

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newAppRequest = JsonConvert.DeserializeObject<NewAppRequest>(requestBody);

            if (newAppRequest == null || string.IsNullOrEmpty(newAppRequest.Name))
            {
                return new BadRequestObjectResult("Invalid request");
            }

            Random rnd = new Random();

            var app = new ApplicationTableEntity()
            {
                AppToken = Guid.NewGuid().ToString(),
                SharedSecret = Helper.ComputeSha256Hash(DateTime.Now.Ticks.ToString() + rnd.Next(1000, 99999).ToString() + Guid.NewGuid().ToString()),
                Name = newAppRequest.Name,
                RedirectUrl = "https://localhost/",
                PartitionKey = StaticDefinitions.ApplicationTableName,
                RowKey = Guid.NewGuid().ToString()
            };


            tableClientApplications.AddEntity<ApplicationTableEntity>(app);

            response.Success = true;
            response.Message = JsonConvert.SerializeObject(app);

            return new OkObjectResult(JsonConvert.SerializeObject(response));
        }
    }
}
