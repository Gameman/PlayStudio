using System;
using System.Threading;
namespace Play.Studio.Core
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
}
