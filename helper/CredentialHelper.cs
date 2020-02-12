using System;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Rest;

namespace RockClimber.Azure.Helpers
{
    public class CredentialHelper
    {
        private string GetKeyVaultSecret(string SecretName)
        {
            string s = Environment.GetEnvironmentVariable(SecretName);
            return s;
        }

        public ServiceClientCredentials GetAzureCredentials()
        {
            string clientId = GetKeyVaultSecret("ClientID");
            string clientSecret = GetKeyVaultSecret("ClientSecret");
            string tenantId = "dsfgvilhf";
            ServiceClientCredentials credentials = new AzureCredentialsFactory().FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            return credentials;
        }
    }
}