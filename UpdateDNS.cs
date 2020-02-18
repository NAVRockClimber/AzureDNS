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
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Rest.Azure;

namespace AzureDNSUpdater
{
    enum UpdateType { A, AAAA}
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
            var serviceClientCredentials = await credentialHelper.GetAzureCredentials();
            string SubscriptionID = Environment.GetEnvironmentVariable("SubscriptionID", EnvironmentVariableTarget.Process);

            string resourceGroupName = req.Query["ResourceGroupName"];
            string zoneName = req.Query["ZoneName"];
            DnsHelper dnsHelper = null;
            try { 
                dnsHelper = new DnsHelper(serviceClientCredentials, SubscriptionID, resourceGroupName, zoneName);
            } 
            catch (Exception e)
            { 
                log.LogError(e.Message, e.StackTrace);
            }

            RecordSet recordSet = await dnsHelper.RecordSet(Hostname, RecordType.A);
            string name = string.Empty;
            if (recordSet != null) {  
                name += recordSet.Name;
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static bool CheckParameters(IQueryCollection parameters)
        {
            if (!(parameters.ContainsKey("Hostname")))
            {
                return false;
            }
            if (!(parameters.ContainsKey("A") || parameters.ContainsKey("AAAA")))
            {
                return false;
            }

            if (!parameters.ContainsKey("ResourceGroupName")) return false;

            if (!parameters.ContainsKey("ZoneName")) return false;
            return true;
        }

    }
}
