using System;
using System.Collections.Generic;
using System.IO;
using Play.Studio.Core;
using Play.Studio.Module.Resource;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Resources;
using Play.Studio.Core.Utility;

namespace Play.Studio.Module.Addins
{
    /// <summary>
    /// 扩展
    /// </summary>
    public sealed class Addin : Collection<IResource>
    {
        private Dictionary<string, ResourceManager> m_resourceManagers;

        /// <summary>
        /// 插件程序集
        /// </summary>
        public Assembly             Assembly    { get; private set; }

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

            m_resourceManagers = new Dictionary<string, ResourceManager>();

            Assembly = Assembly.Load(fileName);
            foreach (var resource in Assembly.GetManifestResourceNames()) 
            {
                if (resource.Contains("resources"))
                {
                    var name = resource.Substring(Assembly.GetName().Name.Length + 1, resource.Length - Assembly.GetName().Name.Length - "resources".Length - 1) + "resx";
                    m_resourceManagers[name] = new ResourceManager(resource.Substring(0, resource.Length - "resources".Length - 1), Assembly);
                }
            }

             //   [0]: "Play.Studio.Workbench.Properties.Resources.resources"
             //   [1]: "Play.Studio.Workbench.g.resources"
        }


        /// <summary>
        /// 加载资源流
        /// </summary>
        public object GetResource(string resourceName) 
        {
            if (resourceName.Contains("->"))
            {
                var paths = resourceName.Split('-', '>');
                if (paths.Length > 2)
                {
                    var name = paths[0] + ".resx";
                    if (!m_resourceManagers.ContainsKey(name))
                        m_resourceManagers[name] = new ResourceManager(name, Assembly);


                    return m_resourceManagers[name].GetObject(paths[2]);
                }
                else 
                {
                    throw new AddinException();
                }
            }
            else
            {
                // 查找到资源名
                return Assembly.GetManifestResourceStream(resourceName);
            }
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
