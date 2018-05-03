using System;
using System.IO;

namespace Pat.Subscriber.Tools
{
    public static class ConsoleWriter
    {
        public static void WriteError(this TextWriter errorOut, string message)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            errorOut.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}