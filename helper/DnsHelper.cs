using System.Threading.Tasks;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace RockClimber.Azure.Helpers
{
    public class DnsHelper
    {
        DnsManagementClient dnsManagementClient;
        Zone dnsZone;
        private string ResourceGroupName { get; set; }
        private string ZoneName { get; set; }

        public DnsHelper(ServiceClientCredentials credentials, string resourceGroupName, string zoneName)
        {
            ResourceGroupName = resourceGroupName;
            ZoneName = zoneName;
            init(credentials);
        }
        private void init(ServiceClientCredentials credentials)
        {
            dnsManagementClient = new DnsManagementClient(credentials);
            Zone dnsZone = dnsManagementClient.Zones.Get(ResourceGroupName, ZoneName);
        }

        public async Task<IPage<RecordSet>> RecordSet()
        {
            IPage<RecordSet> recordSet = await dnsManagementClient.RecordSets.ListAllByDnsZoneAsync(ResourceGroupName, ZoneName);
            return recordSet;
        }
    }
}
