using System;
using System.Threading.Tasks;
using PB.ITOps.Messaging.PatLite.Tools.Commands;

namespace PB.ITOps.Messaging.PatLite.Tools.ApiClients
{
    public class SubscriptionApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public SubscriptionApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task CreateSubscription(PatConfigCommand configCommand, string azureSubscriptionId)
        {
            var payload = new
            {
                name = configCommand.Subscription,
                properties = new
                {
                    maxDeliveryCount = 10,
                    status = "Active"
                }
            };

            var path = ApiRouteBuilder.Build(azureSubscriptionId, configCommand.Namespace, configCommand.EffectiveTopicName, configCommand.Subscription);
            await _azureHttpClient.Put(new Uri(path, UriKind.Relative), payload);
        }
    }
}