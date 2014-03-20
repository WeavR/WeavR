using System;
using System.Globalization;
using System.Linq;
using WeavR.Common;

namespace WeavR.Logging
{
    public abstract class StringLogger : Logger
    {
        protected string FormatMessage(string message, params object[] messageArgs)
        {
            if (messageArgs == null)
                return message;

            return string.Format(CultureInfo.CurrentCulture, message, messageArgs);
        }

        protected string FormatLocation(string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber)
        {
            if (string.IsNullOrEmpty(file))
                return "WeavR :";

            if (lineNumber <= 0)
                return file;

            if (columnNumber <= 0)
                return string.Format("{0}({1}):", file, lineNumber);

            if (endLineNumber <= 0 && endColumnNumber <= 0)
                return string.Format("{0}({1},{2}):", file, lineNumber, columnNumber);

            if (endLineNumber > 0 && endColumnNumber <= 0)
                return string.Format("{0}({1}-{3},{2}):", file, lineNumber, columnNumber, endLineNumber);

            if (endLineNumber <= 0 && endColumnNumber > 0)
                return string.Format("{0}({1},{2}-{3}):", file, lineNumber, columnNumber, endColumnNumber);

            return string.Format("{0}({1},{2},{3},{4}):", file, lineNumber, columnNumber, endLineNumber, endColumnNumber);
        }

        protected string Format(string messageType, string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            return string.Join(" ", FormatLocation(file, lineNumber, columnNumber, endLineNumber, endColumnNumber), subcategory, messageType, code + ":", FormatMessage(message, messageArgs));
        }

        protected override void DoLogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs)
        {
            WriteInfo(Format("message", subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, messageArgs));
        }

        protected override void DoLogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            WriteWarning(Format("warning", subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, messageArgs));
        }

        protected override void DoLogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            WriteError(Format("error", subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, messageArgs));
        }

        protected abstract void WriteInfo(string message);

        protected abstract void WriteWarning(string message);

        protected abstract void WriteError(string message);
    }
}