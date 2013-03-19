using System.Collections.Generic;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public static class ResourceManager
    {
        private static Dictionary<Uri, IResource> s_uriResource = new Dictionary<Uri, IResource>();

        /// <summary>
        /// 注册资源
        /// </summary>
        public static void Register(IResource resource)
        {
            s_uriResource[resource.Uri] = resource;
        }
    }
}
