using System;
using System.Linq;

namespace WeavR.Common
{
    public class StandardMessageLogger
    {
        private const string SENDER = "WeavR";
        private readonly Logger logger;

        public StandardMessageLogger(Logger logger)
        {
            this.logger = logger;
        }

        public Logger RawLogger { get { return logger; } }

        public void LogAlreadyProcessedMessage()
        {
            logger.LogInfo("Assembly already processed by WeavR.", "", SENDER, MessageImportance.Normal);
        }

        public void LogAssemblyNotFound(string path)
        {
            logger.LogError("", "", "", 0, 0, 0, 0, "Assembly '{0}' not found.", "", SENDER, DateTime.UtcNow, path);
        }

        public void LogAssemblyNotProvided()
        {
            logger.LogError("", "", "", 0, 0, 0, 0, "Assembly argument not provided.", "", SENDER);
        }
    }
}