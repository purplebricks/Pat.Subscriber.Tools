using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Pat.Subscriber.Tools
{
    public class AdalLoggerCallback : IAdalLogCallback
    {
        public void Log(LogLevel level, string message)
        {
            //Swallow logs for now, will need to enable for diagnostics later.
        }
    }
}