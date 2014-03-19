using System;
using System.Linq;
using WeavR.Common;

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
            var logger = new WeavRLogger(new ColourConsoleLogger());

            if (args == null || args.Length < 2)
            {
                logger.LogError("WeavR argument not provided.");
                return;
            }

            Engine.Process(logger, args[1], args[0]);
        }
    }
}