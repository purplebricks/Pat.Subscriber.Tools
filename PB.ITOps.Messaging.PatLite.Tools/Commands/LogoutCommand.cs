using Microsoft.Extensions.CommandLineUtils;

namespace PB.ITOps.Messaging.PatLite.Tools.Commands
{
    public class LogoutCommand
    {
        private readonly AzureHttpClient _client;

        public LogoutCommand(AzureHttpClient client)
        {
            _client = client;
        }

        public void Register(CommandLineApplication app)
        {
            app.Description = "Logout the authorised user";
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                _client.Logout();
                return 0;
            });
        }
    }
}