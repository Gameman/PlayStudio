using System.IO;
using Play.Studio.Core.Utility;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 路径
    /// </summary>
    public struct Uri
    {
        public string   FileName    { get; private set; }
        public string   Root        { get; private set; }
        public string   Extension   { get; private set; }
        public UriType  Type        { get; private set; }

        public Uri(string path) 
            : this(path, UriType.Absolute)
        {
        }

        public Uri(string path, UriType type) 
            : this()
        {
            FileName        = path;
            Type            = type;
            Root            = Path.GetDirectoryName(path);
            Extension       = Path.GetExtension(path);
        }

        public override string ToString()
        {
            return FileName;
        }

        public static Uri From(string path) 
        {
            return new Uri(path, UriType.Absolute);
        }
    }

    /// <summary>
    /// 路径类型
    /// </summary>
    public enum UriType
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        Relative,

        /// <summary>
        /// 绝对路径
        /// </summary>
        Absolute 
    }
}
