using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Play.Studio.Core.Logging
{
    public interface ILogger
    {
        void Debug(object message, params object[] args);
        void Info(string message, params object[] args);
        void Warn(object message, params object[] args);
        void Warn(object message, Exception exception, params object[] args);
        void Error(object message, params object[] args);
        void Error(object message, Exception exception, params object[] args);
        void Fatal(object message, params object[] args);
        void Fatal(object message, Exception exception, params object[] args);

        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
    }
}