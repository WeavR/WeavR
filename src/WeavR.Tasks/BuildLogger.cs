using System;
using System.Linq;
using Microsoft.Build.Framework;
using WeavR.Common;

namespace WeavR.Tasks
{
    public class BuildLogger : Logger
    {
        private readonly IBuildEngine buildEngine;

        public BuildLogger(IBuildEngine buildEngine)
        {
            this.buildEngine = buildEngine;
        }

        protected override void DoLogInfo(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, Common.MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs)
        {
            buildEngine.LogMessageEvent(new BuildMessageEventArgs(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, (Microsoft.Build.Framework.MessageImportance)importance, eventTimestamp, messageArgs));
        }

        protected override void DoLogWarning(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            buildEngine.LogWarningEvent(new BuildWarningEventArgs(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, messageArgs));
        }

        protected override void DoLogError(string subcategory, string code, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs)
        {
            buildEngine.LogErrorEvent(new BuildErrorEventArgs(subcategory, code, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, senderName, eventTimestamp, messageArgs));
        }
    }
}