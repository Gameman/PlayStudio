using System.IO;
using Play.Studio.Core.Utility;

namespace Play.Studio.Core.Services
{
    public static partial class FileService
    {
        private static TemporaryFolder _tempFolder;

        /// <summary>
        /// 获取临时文件后缀名
        /// </summary>
        public const string TemporaryExtension = ".temp";

        /// <summary>
        /// 获取临时文件夹
        /// </summary>
        public static TemporaryFolder TempFolder
        { 
            get {
                return _tempFolder ?? (_tempFolder = TemporaryFolder.Create(Path.Combine(FileService.ApplicationRootPath, "temp")));
            } 
        }
    }
}
