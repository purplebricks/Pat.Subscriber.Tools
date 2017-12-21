using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PB.ITOps.Messaging.PatLite.Tools
{
    public class AdalLoggerCallback : IAdalLogCallback
    {
        public void Log(LogLevel level, string message)
        {
            //Swallow logs for now, will need to enable for diagnostics later.
        }
    }
}