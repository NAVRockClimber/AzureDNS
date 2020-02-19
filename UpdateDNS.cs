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
    public static class UpdateDNS
    {
        [FunctionName("UpdateDNS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            ParameterChecker parameterChecker = new ParameterChecker(req.Query);
            if (!parameterChecker.ConfigIsValid)
            {
                return new BadRequestResult();
            }
            IMode mode = parameterChecker.Mode;

            CredentialHelper credentialHelper = new CredentialHelper();
            var serviceClientCredentials = await credentialHelper.GetAzureCredentials();
            string SubscriptionID = Environment.GetEnvironmentVariable("SubscriptionID", EnvironmentVariableTarget.Process);

            string resourceGroupName = req.Query["ResourceGroupName"];
            DnsHelper dnsHelper = null;
            try
            {
                dnsHelper = new DnsHelper(serviceClientCredentials, SubscriptionID, resourceGroupName, mode.Zone);
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e.StackTrace);
            }

            IPage<RecordSet> recordSets = await dnsHelper.RecordSet();
            bool recordExists = false;
            foreach (RecordSet recordSet in recordSets)
            {
                if (recordSet.Name.Equals(mode.Hostname))
                {
                    recordExists = true;
                }
            }

            RecordSet newRecordSet = null;
            if ((!recordExists) && mode.AutoCreateZone)
            {
                newRecordSet = await dnsHelper.CreateZone(mode);
            }

            return newRecordSet != null
                ? (ActionResult)new OkObjectResult($"{newRecordSet.ToString()}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
