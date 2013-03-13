using System;
using Play.Studio.Core;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 依赖资源标签(AOP)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RelyResourceAttribute : AopAttribute   
    {
        Uri         m_uri;
        bool        m_async;
        Type        m_type;
        object[]    m_args;

        public RelyResourceAttribute(Type resourceType, string path, params object[] args)
            : this(resourceType, path, UriType.Relative, args)
        {
        }

        public RelyResourceAttribute(Type resourceType, string path, UriType type, params object[] args)
            : this(resourceType, new Uri(path, type)) 
        {
        }

        public RelyResourceAttribute(Type resourceType, Uri uri, params object[] args) 
            : this(resourceType, uri, false, args)
        {
        }

        public RelyResourceAttribute(Type resourceType, Uri uri, bool async, params object[] args)
            : base(AopTypes.Prefixed)
        {
            m_uri   = uri;
            m_type  = resourceType;
            m_async = async;
            m_args  = args;
        }

        public override void Invoke()
        {
            if (m_async)
                Resource.AsyncRead(m_type, m_uri, null, m_args);
            else
                Resource.Read(m_type, m_uri, m_args);
        }
    }

    /// <summary>
    /// 资源程序集(定义后才能被载入资源)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ResourceAssembly : Attribute           
    {
    }
}
