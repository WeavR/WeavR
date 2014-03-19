using System;
using System.Diagnostics;
using System.Linq;

namespace WeavR
{
    public class ConsoleLogger : StringLogger
    {
        protected override void WriteInfo(string message)
        {
            Console.Out.WriteLine(message);
            Trace.TraceInformation(message);
        }

        protected override void WriteWarning(string message)
        {
            Console.Out.WriteLine(message);
            Trace.TraceWarning(message);
        }

        protected override void WriteError(string message)
        {
            Console.Out.WriteLine(message);
            Trace.TraceError(message);
        }
    }
}