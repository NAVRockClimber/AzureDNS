using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Dns.Models;
using System.Collections.Generic;
using System.Linq;

namespace RockClimber.Azure.Helpers
{
    public class ModeHelper
    {
        IQueryCollection parameters;
        IHeaderDictionary headers;
        IMode mode;
        
        public ModeHelper(IQueryCollection Parameters, IHeaderDictionary Headers)
        {
            parameters = Parameters;
            headers = Headers;
            mode = getMode();
        }
        private IMode getMode()
        {
            IMode mode = null;
            if (parameters.ContainsKey("AutoDetect"))
            {
                mode = new IpV4Mode();
                mode.Address = headers["X-Forwarded-For"].FirstOrDefault().Split(new char[] { ':' }).FirstOrDefault();
            }
            if (parameters.ContainsKey("A"))
            {
                mode = new IpV4Mode();
                mode.Address = parameters["A"];
            }
            if (parameters.ContainsKey("AAAA"))
            {
                mode = new IpV6Mode();
                mode.Address = parameters["AAAA"];
            }

            if (mode != null)
            {
                if (parameters.ContainsKey("AutoCreate"))
                {
                    bool evaluateBuffer;
                    bool.TryParse(parameters["AutoCreate"], out evaluateBuffer);
                    mode.AutoCreateZone = evaluateBuffer;
                }
                mode.Zone = parameters["ZoneName"];
                mode.Hostname = parameters["Hostname"];
            }
            return mode;
        }   

        public IMode Mode { get => mode; }
    }
    public interface IMode
    {
        public RecordType Type { get; }
        public string Zone { get; internal set; }

        public string Hostname { get; internal set; }
        public string Address { get; internal set; }
        public bool AutoCreateZone { get; internal set; }
    }

    public class IpV4Mode : IMode
    {
        bool autoCreateZone = false;
        RecordType IMode.Type { get => RecordType.A; }
        string IMode.Zone { get; set; }

        string IMode.Hostname { get; set; }
        string IMode.Address { get; set; }
        bool IMode.AutoCreateZone
        {
            get => autoCreateZone; set => autoCreateZone = value;
        }
    }

    public class IpV6Mode : IMode
    {
        bool autoCreateZone = false;
        RecordType IMode.Type { get => RecordType.AAAA; }
        string IMode.Zone { get; set; }
        string IMode.Hostname { get; set; }
        string IMode.Address { get; set; }
        bool IMode.AutoCreateZone
        {
            get => autoCreateZone; set => autoCreateZone = value;
        }
    }
}