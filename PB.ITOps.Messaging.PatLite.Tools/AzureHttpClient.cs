using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace PB.ITOps.Messaging.PatLite.Tools
{
    public class AzureHttpClient
    {
        private string _tokenValue = string.Empty;
        private DateTime _tokenExpiresAtUtc = DateTime.MinValue;
        private const string PowerShellClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private const string AzureManagementResource = "https://management.azure.com/";
        private readonly HttpClient _client;
        private string _clientId;
        private string _clientSecret;
        private string _tenantId;
        private readonly FileTokenCache _tokenCache;

        static AzureHttpClient()
        {
            LoggerCallbackHandler.UseDefaultLogging = false;
            LoggerCallbackHandler.Callback = new AdalLoggerCallback();
        }

        public AzureHttpClient()
        {
            _client = new HttpClient();
            _tokenCache = new FileTokenCache();
        }

        public void Logout()
        {
            _tokenCache.Clear();
        }

        public async Task<HttpStatusCode> GetStatusCode(Uri uri)
        {
            var result = await Get(uri);
            return result.StatusCode;
        }

        public async Task Put(Uri uri, object payload)
        {
            await EnsureAuthTokenValid();

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            
            var result = await _client.PutAsync(BuildManagementUri(uri), content);
            result.EnsureSuccessStatusCode();
        }

        public async Task Delete(Uri uri)
        {
            await EnsureAuthTokenValid();

            var result = await _client.DeleteAsync(BuildManagementUri(uri));
            result.EnsureSuccessStatusCode();
        }

        public async Task<T> Get<T>(Uri path, T responseTemplate)
        {
            var result = await Get(path);

            result.EnsureSuccessStatusCode();

            var responsePayload = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeAnonymousType(responsePayload, responseTemplate);
        }

        private async Task<HttpResponseMessage> Get(Uri uri)
        {
            await EnsureAuthTokenValid();

            var result = await _client.GetAsync(BuildManagementUri(uri));
            return result;
        }

        private static Uri BuildManagementUri(Uri uriPath)
        {
            var managementUri = new Uri(new Uri("https://management.azure.com/subscriptions/"), uriPath);

            var queryParams = HttpUtility.ParseQueryString(managementUri.Query);
            if (queryParams.AllKeys.Contains("api-version"))
            {
                return managementUri;
            }

            queryParams["api-version"] = "2017-04-01";

            var builder = new UriBuilder(managementUri)
            {
                Query = string.Join("&", queryParams.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParams[key])}"))
            };

            return builder.Uri;
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

        private async Task EnsureAuthTokenValid()
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
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenValue);
            }
        }

        public void SetServicePrincipal(string configCommandClientId, string configCommandClientSecret, string tenantId)
        {
            _clientId = configCommandClientId;
            _clientSecret = configCommandClientSecret;
            _tenantId = tenantId;
        }
    }
}