using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.CommandLineUtils;

namespace PB.ITOps.Messaging.PatLite.Tools.Commands
{
    public class CommandParser
    {
        private readonly CommandOption _connectionString;
        private readonly CommandOption _namespace;
        private readonly CommandOption _subscriptionSetting;
        private readonly CommandOption _topicSetting;
        private readonly CommandOption _devSetting;
        private readonly CommandOption _clientId;
        private readonly CommandOption _clientSecret;
        private static readonly Regex ServiceBusEntityName = new Regex(@"^[\w-\.\$]{1,50}/?$", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public CommandParser(CommandOption connectionString, CommandOption ns, CommandOption subscriptionSetting, CommandOption topicSetting, CommandOption devSetting, CommandOption clientId, CommandOption clientSecret)
        {
            _connectionString = connectionString;
            _namespace = ns;
            _subscriptionSetting = subscriptionSetting;
            _topicSetting = topicSetting;
            _devSetting = devSetting;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public PatConfigCommand GetConfig()
        {
            var commandValidationFailues = GetValidationErrors();
            if (commandValidationFailues.Any())
            {
                foreach (var failure in commandValidationFailues)
                {
                    Console.Error.WriteError($"Error: {failure}");
                }
                
                return null;
            }

            return new PatConfigCommand
            {
                Namespace = GetNamespace().Namespace,
                Subscription = GetSubscription().Subscription,
                Topic = GetTopic().Topic,
                UseDevelopmentTopic = _devSetting.HasValue(),
                ClientId = GetAuthentication().ClientId,
                ClientSecret = GetAuthentication().ClientSecret
            };
        }

        private IList<string> GetValidationErrors()
        {
            var errors = new List<string>();

            var @namespace = GetNamespace();
            if (!string.IsNullOrEmpty(@namespace.Error))
            {
                errors.Add(@namespace.Error);
            }

            var authentication = GetAuthentication();
            if (!string.IsNullOrEmpty(authentication.Error))
            {
                errors.Add(authentication.Error);
            }

            var subscription = GetSubscription();
            if (!string.IsNullOrEmpty(subscription.Error))
            {
                errors.Add(subscription.Error);
            }

            var topic = GetTopic();
            if (!string.IsNullOrEmpty(topic.Error))
            {
                errors.Add(topic.Error);
            }

            return errors;
        }

        private (string Error, string Namespace) GetNamespace()
        {
            if (_connectionString.HasValue())
            {
                try
                {
                    var conStr = new ServiceBusConnectionStringBuilder(_connectionString.Value());
                    var @namespace = conStr.Endpoint.Split('.').First().Substring(5);
                    return (null, @namespace);
                }
                catch (ArgumentException exc)
                {
                    return ($"{_connectionString.LongName} is invalid: {exc.Message}", null);
                }
            }

            if (_namespace.HasValue())
            {
                return (null, _namespace.Value());
            }

            return ($"One of --{_namespace.LongName} or --{_connectionString.LongName} must be provided.", null);
        }

        private (string Error, string ClientId, string ClientSecret) GetAuthentication()
        {
            if (_clientId.HasValue() && !_clientSecret.HasValue()
                || !_clientId.HasValue() && _clientSecret.HasValue())
            {
                return ($"--{_clientId.LongName} and --{_clientSecret.LongName} must be provided together", null, null);
            }
            return (null, _clientId.Value(), _clientSecret.Value());
        }

        private (string Error, string Subscription) GetSubscription()
        {
            return GetEntity(_subscriptionSetting);
        }

        private (string Error, string Topic) GetTopic()
        {
            return GetEntity(_topicSetting);
        }

        private static (string Error, string Subscription) GetEntity(CommandOption commandOption)
        {
            if (!commandOption.HasValue())
            {
                return ($"--{commandOption.LongName} must be provided", null);
            }

            if (!ServiceBusEntityName.IsMatch(commandOption.Value()))
            {
                return ($@"--{commandOption.LongName} must match the regex '{ServiceBusEntityName}'", null);
            }

            if (commandOption.Value().StartsWith("/") || commandOption.Value().EndsWith("/"))
            {
                return ($"--{commandOption.LongName} cannot start or end with /", null);
            }

            return (null, commandOption.Value());
        }
    }
}