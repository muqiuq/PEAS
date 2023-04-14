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
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace PEAS
{
    public class IndexFunction
    {
        private const string ConfigurationKeyApplicationRoot = "AzureWebJobsScriptRoot";
        private string indexFilePath;

        public IndexFunction(IConfiguration configuration)
        {
            this.indexFilePath = Path.GetFullPath(Path.Combine(
              configuration.GetValue<string>(ConfigurationKeyApplicationRoot),
              "www/index.html"));
        }


        [FunctionName("Index")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Table(StaticDefinitions.ApplicationTableName, Connection = "AzureWebJobsStorage")] TableClient tableClientApplications,
            ILogger log)
        {
            var appToken = req.Query["app"];

            var app = tableClientApplications.Query<ApplicationTableEntity>().Where(i => i.AppToken == appToken).FirstOrDefault();

            if(app == null)
            {
                return new NotFoundResult();
            }

            var indexFile = File.ReadAllText(indexFilePath);
            indexFile = indexFile.Replace("[[NAME]]", app.Name).Replace("[[APPTOKEN]]", app.AppToken);
            return new ContentResult() { Content = indexFile , ContentType = "text/html" };
        }
    }
}
