using System;

namespace Play.Studio.Core.Logging
{
    /// <summary>
    /// 日志类型
    /// </summary>
    [Flags]
    public enum LoggingType 
    {
        Info        = 0x01,
        Error       = 0x02,
        Warn        = 0x03,
        Debug       = 0x04,
        Fatal       = 0x05
    }
}
