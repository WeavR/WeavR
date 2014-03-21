using System;
using System.Collections.Generic;
using System.Linq;

namespace WeavR.Common
{
    public class LoggerContext
    {
        private readonly Logger logger;
        private readonly string sender;
        private readonly string subcategory;

        public LoggerContext(Logger logger, string sender)
        {
            this.logger = logger;
            this.sender = sender;
            this.subcategory = "";
        }

        private LoggerContext(Logger logger, string sender, string subcategory)
        {
            this.logger = logger;
            this.sender = sender;
            this.subcategory = subcategory;
        }

        public bool HasLoggedWarning
        {
            get { return logger.HasLoggedWarning; }
        }

        public bool HasLoggedError
        {
            get { return logger.HasLoggedError; }
        }

        public void Reset()
        {
            logger.Reset();
        }

        public LoggerContext CreateSubContext(string subcategory)
        {
            return new LoggerContext(logger, sender, subcategory);
        }

        public void LogDebug(string message, params object[] messageArgs)
        {
            logger.LogInfo(subcategory, "", "", 0, 0, 0, 0, message, "", sender, MessageImportance.Low, DateTime.UtcNow, messageArgs);
        }

        public void LogInfo(string message, params object[] messageArgs)
        {
            logger.LogInfo(subcategory, "", "", 0, 0, 0, 0, message, "", sender, MessageImportance.Normal, DateTime.UtcNow, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            logger.LogWarning(subcategory, "", "", 0, 0, 0, 0, message, "", sender, DateTime.UtcNow, messageArgs);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            logger.LogError(subcategory, "", "", 0, 0, 0, 0, message, "", sender, DateTime.UtcNow, messageArgs);
        }

        public void LogException(Exception ex)
        {
            logger.LogError(subcategory, "", "", 0, 0, 0, 0, ExceptionToFriendlyString(ex), "", sender);
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
                    lines.AddRange(exception.StackTrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
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