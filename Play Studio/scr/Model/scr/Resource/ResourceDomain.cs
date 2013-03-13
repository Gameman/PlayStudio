using System;
using System.Collections.Generic;
using System.IO;
using Play.Studio.Module.Addins;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源域类型
    /// </summary>
    enum ResourceDomainTypes                                                    
    {
        Addin,
        Local,
        Network
    }

    /// <summary>
    /// 资源域
    /// </summary>
    abstract class ResourceDomain                                               
    {
        private static Dictionary<string, ResourceDomain> s_resourceDomains = new Dictionary<string, ResourceDomain>();

        /// <summary>
        /// 域名
        /// </summary>
        public string                           DomainName      { get; private set; }

        /// <summary>
        /// 资源域类型
        /// </summary>
        public abstract ResourceDomainTypes     DomainType      { get; }

        internal ResourceDomain(string domainName)                              
        {
            DomainName = domainName;
        }

        /// <summary>
        /// 读取域内资源
        /// </summary>
        protected internal abstract object Read(Uri uri);

        /// <summary>
        /// 通过uri获得资源域
        /// </summary>
        internal static ResourceDomain From(Uri uri)                             
        {
            var root = GetRoot(uri);
            if (!s_resourceDomains.ContainsKey(root))
            {

                ResourceDomain  domain      = null;
                switch (GetDomainType(uri))
                {
                    case ResourceDomainTypes.Addin:
                        domain = new AddinResourceDomain(root);
                        break;
                    case ResourceDomainTypes.Local:
                        domain = new LocalResourceDomain(root);
                        break;
                    case ResourceDomainTypes.Network:
                        domain = new NetworkResourceDomain(uri.FileName);
                        break;
                }

                s_resourceDomains[root] = domain;
            }

            return s_resourceDomains[root];
        }

        /// <summary>
        /// 判断路径是否指向程序集
        /// </summary>
        private static ResourceDomainTypes GetDomainType(Uri uri)               
        {
            if (uri.FileName.Contains(";"))
            {
                return ResourceDomainTypes.Addin;
            }
            else if (uri.FileName.Contains("http") || uri.FileName.Contains("ftp"))
            {
                return ResourceDomainTypes.Network;
            }
            else
            {
                return ResourceDomainTypes.Local;
            }
        }

        /// <summary>
        /// 获得root
        /// </summary>
        private static string GetRoot(Uri uri) 
        {
            if (uri.FileName.Contains(";"))
            {
                return uri.FileName.Substring(0, uri.FileName.IndexOf(';'));
            }
            else if (uri.FileName.Contains("http") || uri.FileName.Contains("ftp"))
            {
                return uri.FileName;
            }
            else
            {
                return Path.GetDirectoryName(uri.FileName);
            }
        }
    }

    /// <summary>
    /// 程序集资源域
    /// </summary>
    class AddinResourceDomain : ResourceDomain                                    
    {
        private Addin m_addin;

        public override ResourceDomainTypes DomainType                          
        {
            get { return ResourceDomainTypes.Addin; }
        }

        public AddinResourceDomain(string domainName)
            : base(domainName)                                                  
        {
            m_addin = Addin.LoadFrom(domainName);
        }

        protected internal override object Read(Uri uri)                        
        {
            return m_addin.GetResource(uri.FileName.Substring(DomainName.Length + 1, uri.FileName.Length - DomainName.Length - 1));
        }
    }

    /// <summary>
    /// 本地资源域
    /// </summary>
    class LocalResourceDomain : ResourceDomain                                  
    {
        public override ResourceDomainTypes DomainType                          
        {
            get { return ResourceDomainTypes.Local; }
        }

        public LocalResourceDomain(string domainName)
            : base(domainName)                                                  
        {
        }

        protected internal override object Read(Uri uri)                        
        {
            return new FileStream(uri.FileName, FileMode.Open, FileAccess.Read);
        }
    }

    /// <summary>
    /// 网络资源域
    /// </summary>
    class NetworkResourceDomain : ResourceDomain                                
    {
        public override ResourceDomainTypes DomainType                          
        {
            get { return ResourceDomainTypes.Network; }
        }

        public NetworkResourceDomain(string domainName)
            : base(domainName)                                                  
        {
        }

        protected internal override object Read(Uri uri)                        
        {
            throw new NotImplementedException();
        }
    }
}
