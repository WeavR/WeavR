using System;
using System.Linq;
using WeavR.Logging;

namespace WeavR
{
    internal class Program
    {
        internal class Args
        {
            public string TargetAssembly { get; set; }
        }

        private static void Main(string[] args)
        {
            var logger = new LoggerContext(new ColourConsoleLogger(), "WeavR");

            if (args == null || args.Length < 3)
            {
                logger.LogError("WeavR argument not provided.");
                return;
            }

            Engine.Process(logger, new ProjectDetails()
            {
                SolutionDir = args[0],
                ProjectDirectory = args[1],
                AssemblyPath = args[2]
            });
        }
    }
}