using System;
using System.Collections.Generic;
using System.Linq;

namespace WeavR.Common
{
    public class WeavRLogger
    {
        private const string SENDER = "WeavR";
        private readonly Logger logger;

        public WeavRLogger(Logger logger)
        {
            this.logger = logger;
        }

        public Logger RawLogger { get { return logger; } }

        public void LogInfo(string message, params object[] messageArgs)
        {
            logger.LogInfo(message, "", SENDER, MessageImportance.Normal, DateTime.UtcNow, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            logger.LogWarning("", "", "", 0, 0, 0, 0, message, "", SENDER, DateTime.UtcNow, messageArgs);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            logger.LogError("", "", "", 0, 0, 0, 0, message, "", SENDER, DateTime.UtcNow, messageArgs);
        }

        public void LogException(Exception ex)
        {
            logger.LogError("", "", "", 0, 0, 0, 0, ExceptionToFriendlyString(ex), "", SENDER);
        }

        private string ExceptionToFriendlyString(Exception exception)
        {
            var lines = new List<string>();

            lines.Add("An unhandled exception occurred:");
            while (exception != null)
            {
                lines.Add("Exception: " + exception.Message);

                foreach (var i in exception.Data)
                {
                    lines.Add("Data: " + i);
                }

                if (exception.StackTrace != null)
                {
                    lines.Add("StackTrace:");
                    lines.Add(exception.StackTrace);
                }

                if (exception.Source != null)
                {
                    lines.Add("Source: " + exception.Source);
                }

                if (exception.TargetSite != null)
                {
                    lines.Add("TargetSite: " + exception.TargetSite);
                }

                exception = exception.InnerException;
            }

            return string.Join("\n", lines);
        }
    }
}