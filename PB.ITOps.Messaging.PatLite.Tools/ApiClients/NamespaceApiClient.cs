using System;
using System.Net;
using System.Threading.Tasks;

namespace PB.ITOps.Messaging.PatLite.Tools.ApiClients
{
    public class NamespaceApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public NamespaceApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task<bool> NamespaceExistsIn(string @namespace, string subscriptionId)
        {
            var path = ApiRouteBuilder.Build(subscriptionId, @namespace);

            var response = await _azureHttpClient.GetStatusCode(new Uri(path, UriKind.Relative));

            if (response == HttpStatusCode.OK)
            {
                return true;
            }

            if (response == HttpStatusCode.NotFound)
            {
                return false;
            }

            throw new UnexpectedResponseException(response, nameof(NamespaceExistsIn));
        }
    }
}