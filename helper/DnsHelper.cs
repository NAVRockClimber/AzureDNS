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

        public async Task<RecordSet> RecordSet(string Hostname, RecordType recordType)
        {
            RecordSet recordSet = await dnsManagementClient.RecordSets.GetAsync(ResourceGroupName, ZoneName, Hostname, recordType);
            return recordSet;
        }

        public async Task<bool> RecordExists(IMode mode)
        {
            bool recordExists = false;
            RecordSet record = null;
            IPage<RecordSet> recordSets = await RecordSet();
            foreach (RecordSet recordSet in recordSets)
            {
                if (recordSet.Name.Equals(mode.Hostname))
                {
                    recordExists = true;
                    record = recordSet;
                }
            }
            if (recordExists)
            {
                if (mode.Type == RecordType.A)
                {
                    return record.ARecords != null;
                }
                if (mode.Type == RecordType.AAAA)
                {
                    return record.AaaaRecords != null;
                }
            }
            return false;
        }

        private RecordSet GetUpdateRecordSet(IMode mode)
        {
            RecordSet updateRecordSet = new RecordSet();
            if (mode.Type == RecordType.A)
            {
                ARecord aRecord = new ARecord(mode.Address);
                updateRecordSet.ARecords = new List<ARecord>();
                updateRecordSet.ARecords.Add(aRecord);
            }
            if (mode.Type == RecordType.AAAA)
            {
                AaaaRecord aaaaRecord = new AaaaRecord(mode.Address);
                updateRecordSet.AaaaRecords = new List<AaaaRecord>();
                updateRecordSet.AaaaRecords.Add(aaaaRecord);
            }
            updateRecordSet.TTL = 3600;
            return updateRecordSet;
        }
        public async Task<RecordSet> CreateZone(IMode mode)
        {
            RecordSet recordSetParam = GetUpdateRecordSet(mode);
            RecordType recordType = mode.Type;

            RecordSet recordSet = await dnsManagementClient.RecordSets.CreateOrUpdateAsync(ResourceGroupName, ZoneName, mode.Hostname, recordType, recordSetParam);
            return recordSet;
        }

        public async Task<bool> ZoneIsUpToDate(string Hostname, RecordType recordType, string zoneAddress)
        {
            RecordSet recordSet = await RecordSet(Hostname, recordType);
            if (recordType == RecordType.A)
            {
                ARecord aRecord = new ARecord(zoneAddress);
                foreach (ARecord rec in recordSet.ARecords)
                {
                    string CurrentAddress = rec.Ipv4Address;
                    if (CurrentAddress.Equals(zoneAddress))
                    {
                        return true;
                    }
                }
                return false;
            }
            if (recordType == RecordType.AAAA)
            {
                AaaaRecord aaaaRecord = new AaaaRecord(zoneAddress);
                foreach (AaaaRecord rec in recordSet.AaaaRecords)
                {
                    string CurrentAddress = rec.Ipv6Address;
                    if (CurrentAddress.Equals(zoneAddress))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public async Task<RecordSet> UpdateZone(IMode mode)
        {
            RecordSet recordSetParam = GetUpdateRecordSet(mode);
            RecordType recordType = mode.Type;
            RecordSet recordSet = await dnsManagementClient.RecordSets.UpdateAsync(ResourceGroupName, ZoneName, mode.Hostname, recordType, recordSetParam);
            return recordSet;
        }
    }
}
