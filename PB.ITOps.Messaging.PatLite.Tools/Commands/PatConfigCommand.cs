using System;

namespace PB.ITOps.Messaging.PatLite.Tools.Commands
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
    }
}