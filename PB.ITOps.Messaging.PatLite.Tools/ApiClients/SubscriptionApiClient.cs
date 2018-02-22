using PB.ITOps.Messaging.PatLite.Tools.Commands;
using System;
using System.Threading.Tasks;

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

            var path = BuildPath(configCommand, azureSubscriptionId);
            await _azureHttpClient.Put(new Uri(path, UriKind.Relative), payload);
        }

        public async Task DeleteSubscription(PatConfigCommand configCommand, string azureSubscriptionId)
        {
            var path = BuildPath(configCommand, azureSubscriptionId);
            await _azureHttpClient.Delete(new Uri(path, UriKind.Relative));
        }

        private static string BuildPath(PatConfigCommand configCommand, string azureSubscriptionId)
        {
            return ApiRouteBuilder.Build(azureSubscriptionId, configCommand.Namespace, configCommand.EffectiveTopicName, configCommand.Subscription);
        }
    }
}