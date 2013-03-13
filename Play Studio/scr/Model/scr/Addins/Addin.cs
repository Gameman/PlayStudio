using System;
using System.IO;
using Play.Studio.Core;

namespace Play.Studio.Module.Addins
{
    /// <summary>
    /// 扩展
    /// </summary>
    public sealed class Addin 
    {
        /// <summary>
        /// 插件文件路径
        /// </summary>
        public string               FileName    { get; private set; }

        /// <summary>
        /// 资源初始化方法
        /// </summary>
        public Action<Setting>      InitMethod  { get; private set; }

        internal Addin(string fileName) 
        {
            FileName = fileName;
        }


        /// <summary>
        /// 加载资源流
        /// </summary>
        public Stream GetResourceStream(string resourceName) 
        {
            return null;
        }

        #region Load

        /// <summary>
        /// 从指定路径加载插件
        /// </summary>
        public static Addin LoadFrom(string fileName) 
        {
            return new Addin(fileName);
        }

        #endregion
    }
}
