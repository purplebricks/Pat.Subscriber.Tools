
namespace Pat.Subscriber.Tools.ApiClients
{
    public static class ApiRouteBuilder
    {
        private static string Build(string subscriptionId, string resourceGroup, string @namespace) 
            => $"{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ServiceBus/namespaces/{@namespace}";

        public static string Build(string subscriptionId, string resourceGroup, string @namespace, string topic) 
            => $"{Build(subscriptionId, resourceGroup, @namespace)}/topics/{topic}";

        public static string Build(string subscriptionId, string resourceGroup, string @namespace, string topic, string subscription) 
            => $"{Build(subscriptionId, resourceGroup, @namespace, topic)}/subscriptions/{subscription}";

        public static string Build(string subscriptionId, string resourceGroup, string @namespace, string topic, string subscription, string ruleName)
            => $"{Build(subscriptionId, resourceGroup, @namespace, topic, subscription)}/rules/{ruleName}";
    }
}