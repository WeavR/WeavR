using System;
using System.IO;
using System.Linq;
using WeavR.Common;

namespace WeavR
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new StandardMessageLogger(new ColourConsoleLogger());

            if (args == null || args.Length == 0)
            {
                logger.LogAssemblyNotProvided();
                return;
            }

            var file = new FileInfo(args[0]);

            if (!file.Exists)
            {
                logger.LogAssemblyNotFound(file.FullName);
                return;
            }

            Engine.Process(logger, file.FullName);
        }
    }
}