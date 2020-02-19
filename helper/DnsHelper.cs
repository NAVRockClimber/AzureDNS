using System.Collections.Generic;
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

        public DnsHelper(ServiceClientCredentials credentials, string SubscriptionID, string resourceGroupName, string zoneName)
        {
            ResourceGroupName = resourceGroupName;
            ZoneName = zoneName;
            init(credentials, SubscriptionID);
        }
        private void init(ServiceClientCredentials credentials, string SubscriptionID)
        {
            dnsManagementClient = new DnsManagementClient(credentials);
            dnsManagementClient.SubscriptionId = SubscriptionID;
            IZonesOperations zones = dnsManagementClient.Zones;
            dnsZone = zones.Get(ResourceGroupName, ZoneName);
        }

        public async Task<IPage<RecordSet>> RecordSet()
        {
            IPage<RecordSet> recordSet = await dnsManagementClient.RecordSets.ListAllByDnsZoneAsync(ResourceGroupName, ZoneName);
            return recordSet;
        }

        public async Task<RecordSet> RecordSet(string recordSetName, RecordType recordType)
        {
            RecordSet recordSet = await dnsManagementClient.RecordSets.GetAsync(ResourceGroupName, ZoneName, recordSetName, recordType);
            return recordSet;
        }

        public async Task<RecordSet> CreateZone(IMode mode)
        {
            RecordSet recordSetParam = new RecordSet();
            RecordType recordType = RecordType.TXT;
            recordSetParam.TTL = 3600;

            if (mode.Type == RecordType.A)
            {
                ARecord aRecord = new ARecord(mode.Address);
                recordSetParam.ARecords = new List<ARecord>();
                recordSetParam.ARecords.Add(aRecord);
                recordType = RecordType.A;
            }
            if (mode.Type == RecordType.AAAA)
            {
                AaaaRecord aaaaRecord = new AaaaRecord(mode.Address);
                recordSetParam.AaaaRecords = new List<AaaaRecord>();
                recordSetParam.AaaaRecords.Add(aaaaRecord);
                recordType = RecordType.AAAA;
            }
            RecordSet recordSet = await dnsManagementClient.RecordSets.CreateOrUpdateAsync(ResourceGroupName, ZoneName, mode.Hostname, recordType, recordSetParam);
            return recordSet;
        }
    }
}
