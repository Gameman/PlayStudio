using System;

namespace Play.Studio.Core.Logging
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class Logger
    {
        public static void Debug(object message, params object[] args) 
        {
        }

        public static void Info(string message, params object[] args) 
        {
            Console.WriteLine(message, args);
        }

        public static void Warn(object message, params object[] args) 
        {
            Warn(message, null, args);
        }

        public static void Warn(object message, Exception exception, params object[] args) 
        {
        }

        public static void Error(object message, params object[] args) 
        {
            Error(message, null, args);
        }

        public static void Error(object message, Exception exception, params object[] args) 
        {
        }

        public static void Fatal(object message, params object[] args) 
        {
            Fatal(message, null, args);
        }

        public static void Fatal(object message, Exception exception, params object[] args) 
        {
        }

        public static bool IsDebugEnabled { get; set; }
        public static bool IsInfoEnabled { get; set; }
        public static bool IsWarnEnabled { get; set; }
        public static bool IsErrorEnabled { get; set; }
        public static bool IsFatalEnabled { get; set; }
    }
}