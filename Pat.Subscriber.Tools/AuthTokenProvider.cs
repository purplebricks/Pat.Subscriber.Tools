using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Pat.Subscriber.Tools
{
    public class AuthTokenProvider
    {
        private string _tokenValue = string.Empty;
        private DateTime _tokenExpiresAtUtc = DateTime.MinValue;

        private string _clientId;
        private string _clientSecret;
        private string _tenantId;
        private readonly FileTokenCache _tokenCache;

        private const string PowerShellClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private const string AzureManagementResource = "https://management.azure.com/";

        public AuthTokenProvider()
        {
            _tokenCache = new FileTokenCache();
        }


        public string AuthUser
        {
            get
            {
                var handler = new JwtSecurityTokenHandler();
                var token = (JwtSecurityToken)handler.ReadToken(_tokenValue);
                var name = token.Claims.SingleOrDefault(c => c.Type == "name");
                var upn = token.Claims.SingleOrDefault(c => c.Type == "upn");
                var appId = token.Claims.SingleOrDefault(c => c.Type == "appid");
                return string.IsNullOrEmpty($"{appId.Value}") 
                    ? $"{name.Value} <{upn.Value}>"
                    : $"{appId.Value}";
            }
        }

        public void SetServicePrincipal(string configCommandClientId, string configCommandClientSecret, string tenantId)
        {
            _clientId = configCommandClientId;
            _clientSecret = configCommandClientSecret;
            _tenantId = tenantId;
        }

        public async Task EnsureAuthTokenValid(HttpClient client)
        {
            // Check to see if the token has expired before requesting one.
            // We will go ahead and request a new one if we are within 2 minutes of the token expiring.
            if (_tokenExpiresAtUtc < DateTime.UtcNow.AddMinutes(-2))
            {
                var context = new AuthenticationContext("https://login.microsoftonline.com/common", _tokenCache);
                string tenantFromToken = null;
                if (context.TokenCache.Count > 0)
                {
                    tenantFromToken = context.TokenCache.ReadItems().First().TenantId;
                }
                var tenant = tenantFromToken ?? _tenantId ?? "common";
                context = new AuthenticationContext("https://login.microsoftonline.com/" + tenant, _tokenCache);

                AuthenticationResult result;
                if (!string.IsNullOrEmpty(_clientId) && !string.IsNullOrEmpty(_clientSecret))
                {
                    result = await context.AcquireTokenAsync(
                        AzureManagementResource,
                        new ClientCredential(_clientId, _clientSecret));
                }
                else
                {
                    try
                    {
                        result = await context.AcquireTokenSilentAsync(
                            AzureManagementResource,
                            PowerShellClientId);
                    }
                    catch (AdalSilentTokenAcquisitionException)
                    {
                        var codeResult = await context.AcquireDeviceCodeAsync(
                            AzureManagementResource,
                            PowerShellClientId);
                        Console.WriteLine("You need to sign in.");
                        Console.WriteLine(codeResult.Message);
                        result = await context.AcquireTokenByDeviceCodeAsync(codeResult);
                    }
                }

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new AuthenticationFailureException("Blank Auth Token");
                }

                _tokenExpiresAtUtc = result.ExpiresOn.UtcDateTime;
                _tokenValue = result.AccessToken;
                Console.WriteLine($"Authenticated as {AuthUser}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenValue);
            }
        }

        public void Logout()
        {
            _tokenCache.Clear();
        }
    }
}