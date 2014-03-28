using System;
using System.Linq;

namespace WeavR.Common
{
    public interface ILoggerContext
    {
        bool HasLoggedWarning { get; }
        bool HasLoggedError { get; }

        void Reset();

        ILoggerContext CreateSubContext(string subcategory);

        void LogDebug(string message, params object[] messageArgs);

        void LogInfo(string message, params object[] messageArgs);

        void LogWarning(string message, params object[] messageArgs);

        void LogError(string message, params object[] messageArgs);

        void LogException(Exception ex);
    }
}