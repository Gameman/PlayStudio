using System;

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
