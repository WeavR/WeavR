using System;
using System.Linq;

namespace WeavR.Common
{
    [Serializable]
    public enum MessageImportance
    {
        High = 0,
        Normal = 1,
        Low = 2,
    }

    public abstract class Logger
    {
        private bool hasLoggedError;
        private bool hasLoggedWarning;

        public bool HasLoggedWarning
        {
            get { return hasLoggedWarning; }
        }

        public bool HasLoggedError
        {
            get { return hasLoggedError; }
        }

        public void Reset()
        {
            hasLoggedWarning = false;
            hasLoggedError = false;
        }

        public void LogInfo(string message, string helpKeyword, string senderName, MessageImportance importance)
        {
            LogInfo(message, helpKeyword, senderName, importance, DateTime.UtcNow);
        }

        public void LogInfo(string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp)
        {
            LogInfo(message, helpKeyword, senderName, importance, eventTimestamp, null);
        }

        public void LogInfo(string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs)
        {
            LogInfo(null, null, null, 0, 0, 0, 0, message, helpKeyword, senderName, importance, eventTimestamp, messageArgs);
        }

        public void LogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, MessageImportance importance)
        {
            LogInfo(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, importance, DateTime.UtcNow);
        }

        public void LogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp)
        {
            LogInfo(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, importance, eventTimestamp, null);
        }

        public void LogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs)
        {
            DoLogInfo(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, importance, eventTimestamp, messageArgs);
        }

        public void LogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName)
        {
            LogWarning(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, DateTime.UtcNow);
        }

        public void LogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp)
        {
            LogWarning(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, DateTime.UtcNow, null);
        }

        public void LogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            hasLoggedWarning = true;
            DoLogWarning(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, messageArgs);
        }

        public void LogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName)
        {
            LogError(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, DateTime.UtcNow);
        }

        public void LogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp)
        {
            LogError(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, null);
        }

        public void LogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            hasLoggedError = true;
            DoLogError(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, messageArgs);
        }

        protected abstract void DoLogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs);

        protected abstract void DoLogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs);

        protected abstract void DoLogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs);
    }
}