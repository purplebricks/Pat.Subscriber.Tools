using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace Pat.Subscriber.Tools.Commands
{
    public abstract class BaseCommand
    {
        private readonly string _command;
        private readonly string _description;
        private CommandParser _commandParser;

        protected BaseCommand(string command, string description)
        {
            _command = command;
            _description = description;
        }

        public void Register(CommandLineApplication app)
        {
            var connectionString = app.Option("-c|--connectionString", "servicebus connection string", CommandOptionType.SingleValue);
            var @namespace = app.Option("-n|--namespace", "servicebus namespace", CommandOptionType.SingleValue);
            var subscriptionSetting = app.Option("-s|--subscription", "servicebus subscription name", CommandOptionType.SingleValue);
            var topicSetting = app.Option("-t|--topic", "servicebus topic name", CommandOptionType.SingleValue);
            var partitionSetting = app.Option("-tp|--enablepartitioning", "use topic partitioning dev topic (defaults to false)", CommandOptionType.NoValue);
            var maxTopicSizeSetting = app.Option("-ts|--maxtopicsize", "maximum topic size in MB (defaults to 1024)", CommandOptionType.SingleValue);
            var devSetting = app.Option("-d|--dev", "use local dev topic (defaults to false)", CommandOptionType.NoValue);
            var clientId = app.Option("-ci|--clientId", "Client Id, used used for service principal authentication (optional).", CommandOptionType.SingleValue);
            var clientSecret = app.Option("-cs|--clientSecret", "Client secret, used used for service principal authentication (optional).", CommandOptionType.SingleValue);
            var tenantId = app.Option("-ti|--tenantId", "Tenant Id (optional).", CommandOptionType.SingleValue);

            _commandParser = new CommandParser(connectionString, 
                                                @namespace, 
                                                subscriptionSetting,
                                                topicSetting, 
                                                devSetting, 
                                                clientId, 
                                                clientSecret, 
                                                tenantId, 
                                                partitionSetting,
                                                maxTopicSizeSetting);

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
                catch (Exception exc)
                {
                    Console.Error.WriteError(exc.Message);
                    return -1;
                }
            });
        }

        protected Func<PatConfigCommand, Task<int>> Command { private get; set; }        
    }
}