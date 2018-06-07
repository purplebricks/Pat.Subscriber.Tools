using System;

namespace Pat.Subscriber.Tools.Commands
{
    public class PatConfigCommand
    {
        public string Subscription { get; set; }
        public string Topic { get; set; }
        public bool UseDevelopmentTopic { get; set; }
        public string Namespace { get; set; }
        public string EffectiveTopicName => (UseDevelopmentTopic ? Topic + Environment.GetEnvironmentVariable("COMPUTERNAME") : Topic).ToLower();
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public bool EnablePartitioning { get; set; }
        public int MaxSizeInMegabytes { get; set; }
    }
}