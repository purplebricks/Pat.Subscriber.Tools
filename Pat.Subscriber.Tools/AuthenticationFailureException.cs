using System;
using System.Net;

namespace Pat.Subscriber.Tools
{
    public class AuthenticationFailureException : Exception
    {
        public AuthenticationFailureException(string message): base(message)
        {
            
        }
    }

    public class UnexpectedResponseException: Exception
    {
        public UnexpectedResponseException(HttpStatusCode response, string sourceMethod)
            : base($"Received {response} while calling {sourceMethod}")
        {
        }
    }
}