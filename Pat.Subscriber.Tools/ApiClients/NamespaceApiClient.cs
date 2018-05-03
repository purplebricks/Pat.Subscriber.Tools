using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pat.Subscriber.Tools.ApiClients
{
    public class NamespaceApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public NamespaceApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task<string> GetResourceGroupFor(string @namespace, string subscriptionId)
        {
            var responseTemplate = new
            {
                value = new[]
                {
                    new
                    {
                        id = "/subscriptions/5f750a97-50d9-4e36-8081-c9ee4c0210d4/resourceGroups/Default-ServiceBus-SouthCentralUS/providers/Microsoft.ServiceBus/namespaces/NS-91f08e47-2b04-4943-b0cd-a5fb02b88f20",
                        name = "NS-91f08e47-2b04-4943-b0cd-a5fb02b88f20"
                    }
                }
            };
            var path = $"{subscriptionId}/providers/Microsoft.ServiceBus/namespaces?api-version=2017-04-01";
            var response = await _azureHttpClient.Get(new Uri(path, UriKind.Relative), responseTemplate);

            if (response.ResponsePayload == null || response.ResponsePayload.value == null)
            {
                return null;
            }

            var matchedNamespace = response.ResponsePayload.value.FirstOrDefault(v =>
                string.Equals(v.name, @namespace, StringComparison.InvariantCultureIgnoreCase));

            return matchedNamespace?.id.Split('/')[4];
        }
    }
}