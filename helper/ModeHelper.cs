using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Dns.Models;

namespace RockClimber.Azure.Helpers
{
    public class ParameterChecker
    {
        IQueryCollection parameters;
        IMode mode;
        bool configIsValid;
        public ParameterChecker(IQueryCollection Parameters)
        {
            parameters = Parameters;
            configIsValid = TestParameters();
            if (configIsValid)
            {
                mode = getMode();
            }
        }

        private IMode getMode()
        {
            IMode mode = null;
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

        private bool TestParameters()
        {
            if (!(parameters.ContainsKey("Hostname")))
            {
                return false;
            }
            if (!(parameters.ContainsKey("A") ^ parameters.ContainsKey("AAAA")))
            {
                return false;
            }

            if (!parameters.ContainsKey("ResourceGroupName")) return false;

            if (!parameters.ContainsKey("ZoneName")) return false;

            if (parameters.ContainsKey("AutoCreate"))
            {
                bool evaluateBuffer;
                if (!bool.TryParse(parameters["AutoCreate"], out evaluateBuffer))
                {
                    return false;
                }
            }
            return true;
        }

        public IMode Mode { get => mode; }

        public bool ConfigIsValid { get => configIsValid; }
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