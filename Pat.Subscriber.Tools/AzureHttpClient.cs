using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Polly;

namespace Pat.Subscriber.Tools
{
    public class AzureHttpClient
    {
        private readonly HttpClient _client;
        private readonly AuthTokenProvider _authTokenProvider = new AuthTokenProvider();

        public string AuthUser => _authTokenProvider.AuthUser;

        static AzureHttpClient()
        {
            LoggerCallbackHandler.UseDefaultLogging = false;
            LoggerCallbackHandler.Callback = new AdalLoggerCallback();
        }

        public AzureHttpClient()
        {
            _client = new HttpClient();
        }

        public void Logout()
        {
            _authTokenProvider.Logout();
        }

        public async Task<HttpStatusCode> GetStatusCode(Uri uri)
        {
            var result = await Get(uri);
            return result.StatusCode;
        }

        public async Task Put(Uri uri, object payload)
        {
            await _authTokenProvider.EnsureAuthTokenValid(_client);

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            
            var result = await RetryHonouringRetryAfter.ExecuteAsync(() => _client.PutAsync(BuildManagementUri(uri), content));

            if (!result.IsSuccessStatusCode)
            {
                await ThrowFailureExceptionFor(result);
            }
        }

        public async Task Delete(Uri uri)
        {
            await _authTokenProvider.EnsureAuthTokenValid(_client);

            var result = await RetryHonouringRetryAfter.ExecuteAsync(() => _client.DeleteAsync(BuildManagementUri(uri)));

            if (!result.IsSuccessStatusCode)
            {
                await ThrowFailureExceptionFor(result);
            }
        }

        public async Task<(T ResponsePayload, HttpStatusCode Status)> Get<T>(Uri path, T responseTemplate)
        {
            var result = await Get(path);

            if (!result.IsSuccessStatusCode)
            {
                await ThrowFailureExceptionFor(result);
            }

            var responsePayload = await result.Content.ReadAsStringAsync();

            return (JsonConvert.DeserializeAnonymousType(responsePayload, responseTemplate), result.StatusCode);
        }

        private async Task<HttpResponseMessage> Get(Uri uri)
        {
            await _authTokenProvider.EnsureAuthTokenValid(_client);

            var result = await RetryHonouringRetryAfter.ExecuteAsync(() => _client.GetAsync(BuildManagementUri(uri)));

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

        private static async Task ThrowFailureExceptionFor(HttpResponseMessage result)
        {
            string resultBody;

            try
            {
                resultBody = await result.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                resultBody = $"<Exception reading body: {exception.Message}>";
            }

            var message = $"Received response code {result.StatusCode} ({result.ReasonPhrase}). Body was: {resultBody}";
            throw new InvalidOperationException(message);
        }

        public void SetServicePrincipal(string configCommandClientId, string configCommandClientSecret, string tenantId)
        {
            _authTokenProvider.SetServicePrincipal(configCommandClientId, configCommandClientSecret, tenantId);
        }

        private IAsyncPolicy<HttpResponseMessage> RetryHonouringRetryAfter
        {
            get
            {
                return Policy.Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode) 429)
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: (retryCount, response, context) =>
                        {
                            var retryDurationHeaderValue =
                                response.Result?.Headers.GetValues("Retry-After")?.FirstOrDefault();
                            return int.TryParse(retryDurationHeaderValue, out int retryDuration)
                                ? TimeSpan.FromSeconds(retryDuration)
                                : TimeSpan.FromSeconds(60);
                        },

                        onRetryAsync: (response, timespan, retryCount, context) =>
                        {
                            Console.WriteLine(
                                $"Retrying request to {response.Result.RequestMessage.RequestUri}, attempt {retryCount}");
                            return Task.CompletedTask;
                        }
                    );
            }
        }
    }
}