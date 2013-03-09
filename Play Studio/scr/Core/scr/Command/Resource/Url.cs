namespace Play.Studio.Core
{
    /// <summary>
    /// 路径
    /// </summary>
    public struct Uri
    {
        public string   Path { get; private set; }
        public UriType  Type { get; private set; }

        public Uri(string path) 
            : this(path, UriType.Absolute)
        {
        }

        public Uri(string path, UriType type) 
            : this()
        {
            Path = path;
            Type = type;
        }

        public static Uri Create(string path) 
        {
            return new Uri(path, UriType.Absolute);
        }
    }

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
