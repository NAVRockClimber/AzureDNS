using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace RockClimber.Azure.Helpers
{
    public class CredentialHelper
    {
        private string GetKeyVaultSecret(string SecretName)
        {
            string s = Environment.GetEnvironmentVariable(SecretName, EnvironmentVariableTarget.Process);
            return s;
        }

        public async Task<ServiceClientCredentials> GetAzureCredentials()
        {
            string clientId = GetKeyVaultSecret("ClientID");
            string clientSecret = GetKeyVaultSecret("ClientSecret");
            string tenantId = GetKeyVaultSecret("TenantId");
            var credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientSecret);
            // ServiceClientCredentials credentials = new AzureCredentialsFactory().FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            return credentials;
        }
    }
}