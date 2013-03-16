using System;
using System.Reflection;

[Serializable]
public class PlayException : Exception
{
    private string      m_message;

    public MethodBase MethodBase
    {
        get;
        private set;
    }

    public override string Message
    {
        get
        {
            return m_message;
        }
    }

    public PlayException() 
    {
        // 读取到堆栈上一帧内容
        var frame = new System.Diagnostics.StackTrace(this).GetFrame(-1);

        MethodBase  = frame.GetMethod();
        m_message   = frame.GetMethod().Name;
    }
    public PlayException(string message) : base(message) { }
    public PlayException(string message, Exception inner) : base(message, inner) { }
    protected PlayException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}

namespace Play.Studio.Core
{
    /// <summary>
    /// 异常中心
    /// </summary>
    public static class ExceptionCenter
    {
        /// <summary>
        /// 获取是否是观察状态
        /// </summary>
        public static bool IsWatched { get; private set; }

        /// <summary>
        /// 观察异常
        /// </summary>
        /// <param name="watch"></param>
        public static void Watch(bool watch) 
        {
            if (IsWatched = watch)
                AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            else
                AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
        }

        /// <summary>
        /// 当程序域捕获某个异常时
        /// </summary>
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e) 
        {
           
        }
    }
}