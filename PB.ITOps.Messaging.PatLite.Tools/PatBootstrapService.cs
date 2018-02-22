using PB.ITOps.Messaging.PatLite.Tools.ApiClients;
using PB.ITOps.Messaging.PatLite.Tools.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PB.ITOps.Messaging.PatLite.Tools
{
    public class PatBootstrapService
    {
        private readonly AzureHttpClient _azureHttpClient;
        private readonly AzureSubscriptionApiClient _azureSubscriptionApiClient;
        private readonly NamespaceApiClient _namespaceApiClient;
        private readonly SubscriptionApiClient _subscriptionApiClient;
        private readonly TopicApiClient _topicApiClient;

        public PatBootstrapService(
            AzureHttpClient azureHttpClient,
            AzureSubscriptionApiClient azureSubscriptionApiClient,
            NamespaceApiClient namespaceApiClient,
            SubscriptionApiClient subscriptionApiClient,
            TopicApiClient topicApiClient)
        {
            _azureHttpClient = azureHttpClient;
            _azureSubscriptionApiClient = azureSubscriptionApiClient;
            _namespaceApiClient = namespaceApiClient;
            _subscriptionApiClient = subscriptionApiClient;
            _topicApiClient = topicApiClient;
        }

        public async Task<int> Create(PatConfigCommand configCommand)
        {
            ConfigureAuth(configCommand);

            var azureSubscriptionId = await GetAzureSubscriptionFor(configCommand);
            if (string.IsNullOrEmpty(azureSubscriptionId))
            {
                return -1;
            }

            await _topicApiClient.CreateTopic(configCommand, azureSubscriptionId);

            await _subscriptionApiClient.CreateSubscription(configCommand, azureSubscriptionId);

            Console.WriteLine($"create complete {configCommand.Namespace} {configCommand.EffectiveTopicName}\\{configCommand.Subscription}");

            return 0;
        }

        private void ConfigureAuth(PatConfigCommand configCommand)
        {
            _azureHttpClient.SetServicePrincipal(configCommand.ClientId, configCommand.ClientSecret, configCommand.TenantId);
        }

        public async Task<int> DeleteSubscription(PatConfigCommand configCommand)
        {
            ConfigureAuth(configCommand);

            var azureSubscriptionId = await GetAzureSubscriptionFor(configCommand);
            if (string.IsNullOrEmpty(azureSubscriptionId))
            {
                return -1;
            }

            await _subscriptionApiClient.DeleteSubscription(configCommand, azureSubscriptionId);

            Console.WriteLine($"delete complete {configCommand.Namespace} {configCommand.EffectiveTopicName}\\{configCommand.Subscription}");
            return 0;
        }

        public async Task<int> DeleteTopic(PatConfigCommand configCommand)
        {
            ConfigureAuth(configCommand);

            var azureSubscriptionId = await GetAzureSubscriptionFor(configCommand);
            if (string.IsNullOrEmpty(azureSubscriptionId))
            {
                return -1;
            }

            await _topicApiClient.DeleteTopic(configCommand, azureSubscriptionId);

            Console.WriteLine($"delete complete {configCommand.Namespace} {configCommand.EffectiveTopicName}\\{configCommand.Subscription}");
            return 0;
        }

        private async Task<string> GetAzureSubscriptionFor(PatConfigCommand configCommand)
        {
            var subscriptions = await _azureSubscriptionApiClient.GetSubscriptions();
            if (subscriptions.Any())
            {
                foreach (var subscription in subscriptions)
                {
                    if (await _namespaceApiClient.NamespaceExistsIn(configCommand.Namespace, subscription))
                    {
                        return subscription;
                    }
                }

                Console.Error.WriteError($"Error: Unable to find namespace {configCommand.Namespace} in any azure subscriptions");
            }

            return string.Empty;
        }
    }
}