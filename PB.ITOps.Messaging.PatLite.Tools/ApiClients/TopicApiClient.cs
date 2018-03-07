using PB.ITOps.Messaging.PatLite.Tools.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PB.ITOps.Messaging.PatLite.Tools.ApiClients
{
    public class TopicApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public TopicApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task CreateTopic(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            if (await TopicExists(configCommand, resouceGroupName, azureSubscriptionId))
            {
                return;
            }

            var payload = new
            {
                properties = new
                {
                    enablePartitioning = true,
                    maxSizeInMegabytes = 1024
                }
            };

            var path = ApiRouteBuilder.Build(azureSubscriptionId, resouceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            await _azureHttpClient.Put(new Uri(path, UriKind.Relative), payload);
        }

        public async Task DeleteTopic(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            if (!await TopicExists(configCommand, azureSubscriptionId, resouceGroupName))
            {
                return;
            }

            var path = ApiRouteBuilder.Build(azureSubscriptionId, resouceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            await _azureHttpClient.Delete(new Uri(path, UriKind.Relative));
        }

        private async Task<bool> TopicExists(PatConfigCommand configCommand, string azureSubscriptionId, string resouceGroupName)
        {
            var path = ApiRouteBuilder.Build(azureSubscriptionId, resouceGroupName, configCommand.Namespace, configCommand.EffectiveTopicName);
            var result = await _azureHttpClient.GetStatusCode(new Uri(path, UriKind.Relative));

            if (result == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
    }
}