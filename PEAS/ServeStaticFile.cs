using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeTypes;

namespace PEAS
{
    public class ServeStaticFile
    {
        private readonly string contentRoot;
        // This key is used by Azure Functions to tell you what is the root of this website.
        private const string ConfigurationKeyApplicationRoot = "AzureWebJobsScriptRoot";
        private const string staticFilesFolder = "www";
        private readonly string defaultPage;

        private readonly string[] Blacklist = { "index.html" };

        // The configuration is available for injection.
        // The used settings can be in any config (environment, host.json local.settings.json)
        public ServeStaticFile(IConfiguration configuration)
        {
            this.contentRoot = Path.GetFullPath(Path.Combine(
              configuration.GetValue<string>(ConfigurationKeyApplicationRoot),
              staticFilesFolder));
            this.defaultPage = configuration.GetValue<string>("DEFAULT_PAGE", "index.html");
        }

        [FunctionName("static")]
        public async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var filePath = GetFilePath(req.Query["f"]);
                if (File.Exists(filePath) && !Blacklist.Contains(Path.GetFileName(filePath).ToLower()))
                {
                    var stream = File.OpenRead(filePath);
                    return new FileStreamResult(stream, Helper.GetMimeType(filePath))
                    {
                        LastModified = File.GetLastWriteTime(filePath)
                    };
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private string GetFilePath(string pathValue)
        {
            var path = pathValue ?? "";
            string fullPath = Path.GetFullPath(Path.Combine(contentRoot, pathValue));
            if (!IsInDirectory(this.contentRoot, fullPath))
            {
                throw new ArgumentException("Invalid path");
            }

            if (Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(fullPath, defaultPage);
            }
            return fullPath;
        }

        private static bool IsInDirectory(string parentPath, string childPath) => childPath.StartsWith(parentPath);


    }
}
