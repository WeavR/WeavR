using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace WeavR.Tasks.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var projectFileName = @"..\..\..\SampleProject\SampleProject.csproj";
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string>();
#if DEBUG
            globalProperty.Add("Configuration", "Debug");
#else
            globalProperty.Add("Configuration", "Release");
#endif
            globalProperty.Add("Platform", "AnyCPU");

            var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new string[] { "Rebuild" }, null);

            var buildParameters = new BuildParameters(pc) { Loggers = new ILogger[] { new ConsoleLogger(LoggerVerbosity.Normal, Write, SetColor, ResetColor) } };

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
        }

        private static void Write(string message)
        {
            Console.Out.Write(message);

            message = message.TrimEnd('\r', '\n');

            if (Console.ForegroundColor == ConsoleColor.Red)
                Trace.TraceError("    : " + message);
            else if (Console.ForegroundColor == ConsoleColor.Yellow)
                Trace.TraceWarning("  : " + message);
            else
                Trace.TraceInformation(message);
        }

        private static void SetColor(ConsoleColor c)
        {
            try
            {
                var colour = c;
                if (c == Console.BackgroundColor)
                {
                    if (Console.BackgroundColor != ConsoleColor.Black)
                    {
                        colour = ConsoleColor.Black;
                    }
                    else
                    {
                        colour = ConsoleColor.Gray;
                    }
                }
                Console.ForegroundColor = colour;
            }
            catch (IOException)
            {
            }
        }

        private static void ResetColor()
        {
            try
            {
                Console.ResetColor();
            }
            catch (IOException)
            {
            }
        }
    }
}