using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PB.ITOps.Messaging.PatLite.Tools.Commands
{
    public abstract class BaseCommand
    {
        private readonly string _command;
        private readonly string _description;
        private CommandOption _connectionString;
        private CommandOption _namespace;
        private CommandOption _subscriptionSetting;
        private CommandOption _topicSetting;
        private CommandOption _devSetting;
        private CommandOption _clientId;
        private CommandOption _clientSecret;
        private CommandOption _tenantId;
        private CommandParser _commandParser;

        protected BaseCommand(string command, string description)
        {
            _command = command;
            _description = description;
        }

        public void Register(CommandLineApplication app)
        {
            _connectionString = app.Option("-c|--connectionString", "servicebus connection string", CommandOptionType.SingleValue);
            _namespace = app.Option("-n|--namespace", "servicebus namespace", CommandOptionType.SingleValue);
            _subscriptionSetting = app.Option("-s|--subscription", "servicebus subscription name", CommandOptionType.SingleValue);
            _topicSetting = app.Option("-t|--topic", "servicebus topic name", CommandOptionType.SingleValue);
            _devSetting = app.Option("-d|--dev", "use local dev topic (defaults to false)", CommandOptionType.NoValue);
            _clientId = app.Option("-ci|--clientId", "Client Id, used used for service principal authentication (optional).", CommandOptionType.SingleValue);
            _clientSecret = app.Option("-cs|--clientSecret", "Client secret, used used for service principal authentication (optional).", CommandOptionType.SingleValue);
            _tenantId = app.Option("-ti|--tenantId", "Tenant Id (optional).", CommandOptionType.SingleValue);

            _commandParser = new CommandParser(_connectionString, _namespace, _subscriptionSetting, _topicSetting, _devSetting, _clientId, _clientSecret, _tenantId);

            app.Description = _description;

            app.HelpOption("-?|-h|--help");

            app.OnExecute(async () =>
            {
                var commandConfig = _commandParser.GetConfig();
                if (commandConfig == null)
                {
                    app.ShowHelp(_command);
                    return -1;
                }
                try
                {
                    return await Command(commandConfig);
                }
                catch (AggregateException aggExc)
                {
                    Console.Error.WriteError(aggExc.Flatten().Message);
                    return -1;
                }
                catch(Exception exc)
                {
                    Console.Error.WriteError(exc.Message);
                    return -1;
                }
            });
        }

        protected Func<PatConfigCommand, Task<int>> Command { private get; set; }        
    }
}