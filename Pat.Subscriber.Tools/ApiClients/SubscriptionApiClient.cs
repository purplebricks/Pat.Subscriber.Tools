using Pat.Subscriber.Tools.Commands;
using System;
using System.Threading.Tasks;

namespace Pat.Subscriber.Tools.ApiClients
{
    public class SubscriptionApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public SubscriptionApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task CreateSubscription(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
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

            var path = BuildSubscriptionPath(configCommand, azureSubscriptionId, resouceGroupName);
            await _azureHttpClient.Put(new Uri(path, UriKind.Relative), payload);
        }

        public async Task RemoveDefaultFilter(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            var path = BuildRulePath(configCommand, azureSubscriptionId, resouceGroupName, "%24Default");
            await _azureHttpClient.Delete(new Uri(path, UriKind.Relative));
        }

        public async Task DeleteSubscription(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            var path = BuildSubscriptionPath(configCommand, azureSubscriptionId, resouceGroupName);
            await _azureHttpClient.Delete(new Uri(path, UriKind.Relative));
        }

        private static string BuildSubscriptionPath(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            return ApiRouteBuilder.Build(azureSubscriptionId, resouceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName, configCommand.Subscription);
        }

        private static string BuildRulePath(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName, string ruleName)
        {
            return ApiRouteBuilder.Build(azureSubscriptionId, resouceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName, configCommand.Subscription, ruleName);
        }
    }
}