namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源名
    /// </summary>
    public struct ResourceName
    {
        private static int s_token;

        /// <summary>
        /// 资源标记
        /// </summary>
        public int              Token   { get; private set; }

        /// <summary>
        /// 资源名
        /// </summary>
        public string           Name    { get; private set; }

        /// <summary>
        /// 资源域名
        /// </summary>
        internal ResourceDomain Domain  { get; private set; }

        /// <summary>
        /// 是否是有效资源
        /// </summary>
        public bool             IsValid { get; private set; }

        private ResourceName(int token, ResourceDomain domain) 
            : this()
        {
            Domain  = domain;
        }

        public static ResourceName From(Uri uri) 
        {
            return new ResourceName(s_token++, ResourceDomain.From(uri));
        }


    }
}
