namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源名
    /// </summary>
    public struct ResourceName
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public Uri              Uri     { get; set; }

        /// <summary>
        /// 资源域名
        /// </summary>
        public ResourceDomain   Domain  { get; set; }


        private ResourceName() 
        {
        }

        private ResourceName(Uri uri, ResourceDomain domain) 
            : this()
        {
            Uri     = uri;
            Domain  = domain;
        }

        public static ResourceName From(Uri uri) 
        {
            return new ResourceName(uri, ResourceDomain.Get(uri));
        }


    }
}
