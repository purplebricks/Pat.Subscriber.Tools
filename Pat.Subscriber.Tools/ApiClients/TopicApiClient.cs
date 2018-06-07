using Pat.Subscriber.Tools.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pat.Subscriber.Tools.ApiClients
{
    public class TopicApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public TopicApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task CreateTopic(PatConfigCommand configCommand, string azureSubscriptionId, string resourceGroupName)
        {
            if (await TopicExists(configCommand, resourceGroupName, azureSubscriptionId))
            {
                return;
            }

            var payload = new
            {
                properties = new
                {
                    enablePartitioning = configCommand.EnablePartitioning,
                    maxSizeInMegabytes = configCommand.MaxSizeInMegabytes
                }
            };

            var path = ApiRouteBuilder.Build(azureSubscriptionId, resourceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            await _azureHttpClient.Put(new Uri(path, UriKind.Relative), payload);
        }

        public async Task DeleteTopic(PatConfigCommand configCommand, string azureSubscriptionId, string resourceGroupName)
        {
            if (!await TopicExists(configCommand, azureSubscriptionId, resourceGroupName))
            {
                return;
            }

            var path = ApiRouteBuilder.Build(azureSubscriptionId, resourceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            await _azureHttpClient.Delete(new Uri(path, UriKind.Relative));
        }

        private async Task<bool> TopicExists(PatConfigCommand configCommand, string azureSubscriptionId, string resourceGroupName)
        {
            var path = ApiRouteBuilder.Build(azureSubscriptionId, resourceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            var result = await _azureHttpClient.GetStatusCode(new Uri(path, UriKind.Relative));

            if (result == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
    }
}