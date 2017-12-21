namespace PB.ITOps.Messaging.PatLite.Tools.ApiClients
{
    public static class ApiRouteBuilder
    {
        public static string Build(string subscriptionId, string @namespace) 
            => $"{subscriptionId}/resourceGroups/Shared/providers/Microsoft.ServiceBus/namespaces/{@namespace}";

        public static string Build(string subscriptionId, string @namespace, string topic) 
            => $"{Build(subscriptionId, @namespace)}/topics/{topic}";

        public static string Build(string subscriptionId, string @namespace, string topic, string subscription) 
            => $"{Build(subscriptionId, @namespace, topic)}/subscriptions/{subscription}";
    }
}