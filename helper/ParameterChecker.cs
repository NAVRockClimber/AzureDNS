using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;

namespace AzureDNSUpdater.helper
{
    public class ParameterChecker
    {
        IQueryCollection parameters;
        bool isValidConfig;
        public ParameterChecker(IQueryCollection Parameters)
        {
            parameters = Parameters;
            isValidConfig = TestParameters();
        }

        private bool TestParameters()
        {
            if (!(parameters.ContainsKey("Hostname")))
            {
                return false;
            }
            if (!(parameters.ContainsKey("A") || parameters.ContainsKey("AAAA") || parameters.ContainsKey("AutoDetect")))
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

        public bool IsValidConfig { get => isValidConfig; }
    }
}
