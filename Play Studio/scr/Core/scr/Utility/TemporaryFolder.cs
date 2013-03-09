using System;
using System.Diagnostics;
using System.IO;
using Play.Studio.Core.Services;

namespace Play.Studio.Core.Utility
{
    /// <summary>
    /// 临时文件夹创建异常
    /// </summary>
    public class TemporaryFolderCreateException : Exception 
    {
        public TemporaryFolderCreateException() : base() { }

        public TemporaryFolderCreateException(string message) : base(message) { }

        public TemporaryFolderCreateException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 临时文件夹
    /// </summary>
    public class TemporaryFolder
    {
        /// <summary>
        /// 获取临时文件夹路径
        /// </summary>
        public string Root 
        {
            get; 
            private set;
        }

        private TemporaryFolder(string path) 
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Root = path;

            Process.GetCurrentProcess().Exited += new EventHandler(ProcessExited);
        }

        /// <summary>
        /// 当进程退出时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void ProcessExited(object sender, EventArgs args)
        {
            Process.GetCurrentProcess().Exited -= ProcessExited;

            // 删除目录
            FileService.RemoveDirectory(Root, false);
        }

        public void Clear() 
        {
            FileService.RemoveFile(Root, false);
        }

        /// <summary>
        /// 保存临时数据并返回完整路径
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Save(byte[] data, string name = null, string extension = FileService.TemporaryExtension)          
        {
            string path = Path.Combine(Root, (string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name) + extension);
            File.WriteAllBytes(path, data);
            return path;
        }

        public static TemporaryFolder Create(string path)   
        {
            /*
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists) 
            {
                var files = info.GetFiles();
                if (files.Length > 0)
                {
                    foreach (var file in files) 
                    {
                        if (!file.Extension.Equals(FileService.TemporaryExtension)) 
                        {
                            throw new TemporaryFolderCreateException(string.Format("该目录中存在非临时文件'{0}'."));
                        }
                    }
                }
            }
            */

            return new TemporaryFolder(path);
        }
    }
}
