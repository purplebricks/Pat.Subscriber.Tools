using System;
using Microsoft.Extensions.CommandLineUtils;
using Pat.Subscriber.Tools.ApiClients;
using Pat.Subscriber.Tools.Commands;

namespace Pat.Subscriber.Tools
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "pat"
            };

            app.HelpOption("-?|-h|--help");

            var client = new AzureHttpClient();
            var patConfigurationService = BuildConfigurationService(client);

            app.Command("create", new CreateCommand(patConfigurationService).Register);
            app.Command("deleteSubscription", new DeleteSubscriptionCommand(patConfigurationService).Register);
            app.Command("deleteTopic", new DeleteTopicCommand(patConfigurationService).Register);
            app.Command("logout", new LogoutCommand(client).Register);

            app.OnExecute(() => {
                app.ShowHelp();
                return 0;
            });

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException commandParsingException)
            {
                Console.WriteLine(commandParsingException.Message);
                commandParsingException.Command.ShowHelp();
                return -1;
            }
            catch (AuthenticationFailureException)
            {
                Console.Error.WriteError("Error: Failed to authenticate with Azure AD");
                return -1;
            }
            catch (UnexpectedResponseException unexpectedResponseException)
            {
                Console.Error.WriteError($"Error: Unexpected response: {unexpectedResponseException.Message}");
                return -1;
            }
        }

        private static PatBootstrapService BuildConfigurationService(AzureHttpClient client)
        {
            return new PatBootstrapService(
                client,
                new AzureSubscriptionApiClient(client), 
                new NamespaceApiClient(client), 
                new SubscriptionApiClient(client),
                new TopicApiClient(client));
        }
    }
}
