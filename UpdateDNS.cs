using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using RockClimber.Azure.Helpers;


namespace AzureDNSUpdater
{
    public static class UpdateDNS
    {
        [FunctionName("UpdateDNS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (!CheckParameters(req.Query))
            {
                return new BadRequestResult();
            }

            CredentialHelper credentialHelper = new CredentialHelper();
            var serviceClientCredentials = credentialHelper.GetAzureCredentials();

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static bool CheckParameters(IQueryCollection parameters)
        {
            if (!(parameters.ContainsKey("hostname")))
            {
                return false;
            }
            if (!(parameters.ContainsKey("A") || parameters.ContainsKey("AAAA")))
            {
                return false;
            }

            return true;
        }

    }
}
