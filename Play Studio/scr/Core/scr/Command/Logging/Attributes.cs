using System;

namespace Play.Studio.Core.Logging
{
    /// <summary>
    /// 日志标签(AOP)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LoggingAttribute : AopAttribute 
    {
        private LoggingType m_loggingType;
        private string      m_message;
        private object[]    m_args;

        public LoggingAttribute(AopTypes aopType, LoggingType loggingType, string message, params object[] args) 
            : base(aopType)
        {
            m_loggingType = loggingType;
            m_message = message;
            m_args = args;
        }

        public override void Invoke()
        {
            switch (m_loggingType)
            {
                case LoggingType.Error:     Logger.Error(m_message, m_args);    break;
                case LoggingType.Info:      Logger.Info(m_message, m_args);     break;
                case LoggingType.Warn:      Logger.Warn(m_message, m_args);     break;
                case LoggingType.Debug:     Logger.Debug(m_message, m_args);    break;
                case LoggingType.Fatal:     Logger.Fatal(m_message, m_args);    break;
            }
        }
    }
}
