﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pat.Subscriber.Tools.ApiClients
{
    public class AzureSubscriptionApiClient
    {
        private readonly AzureHttpClient _azureHttpClient;

        public AzureSubscriptionApiClient(AzureHttpClient azureHttpClient)
        {
            _azureHttpClient = azureHttpClient;
        }

        public async Task<IEnumerable<string>> GetSubscriptions()
        {
            var responseTemplate = new
            {
                value = new[]
                {
                    new {subscriptionId = ""}
                }
            };

            var response = await _azureHttpClient.Get(new Uri("?api-version=2017-08-01", UriKind.Relative), responseTemplate);
            var subscriptions = response.ResponsePayload.value.Select(x => x.subscriptionId);
            if (!subscriptions.Any())
            {
                Console.Error.WriteError($"Error: No azure subscriptions were found for the user {_azureHttpClient.AuthUser}");
            }
            return subscriptions;
        }
    }
}