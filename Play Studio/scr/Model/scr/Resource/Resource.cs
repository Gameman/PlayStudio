using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Play.Studio.Core.Command.TaskPool;
using Play.Studio.Core.Services;
using Play.Studio.Module.Addins;
using Play.Studio.Module.Language;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源接口
    /// </summary>
    public interface IResource                                                                                                                  
    {
        /// <summary>
        /// 资源地址
        /// </summary>
        Uri             Uri         { get; }

        /// <summary>
        /// 资源是否已被加载
        /// </summary>
        bool            Loaded      { get; }

        /// <summary>
        /// 资源名
        /// </summary>
        ResourceName    Name        { get; }

        /// <summary>
        /// 获得结果
        /// </summary>
        object GetResult();
    }

    /// <summary>
    /// 资源
    /// </summary>
    public abstract class Resource : IResource                                                                                                  
    {
        static Resource() 
        {
            // 注册一些通用资源
            Register<LanguageResource>(".lang");
        }


        private static Dictionary<string, Type>     m_extResourceType           = new Dictionary<string, Type>();
        private static Dictionary<int, Resource>    m_currentThreadResources    = new Dictionary<int, Resource>();
        public static Resource Current                                                                                                          
        {
            get 
            {
                if (!m_currentThreadResources.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    m_currentThreadResources[Thread.CurrentThread.ManagedThreadId] = null;

                return m_currentThreadResources[Thread.CurrentThread.ManagedThreadId];
            }
            private set 
            {
                if (!m_currentThreadResources.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    m_currentThreadResources[Thread.CurrentThread.ManagedThreadId] = null;

                m_currentThreadResources[Thread.CurrentThread.ManagedThreadId] = value;
            }
            
        }

        /// <summary>
        /// 注册一个通用资源类型
        /// </summary>
        public static void Register(string extension, Type resourceType)                                                                        
        {
            if (!typeof(Resource).IsAssignableFrom(resourceType))
                throw new ResourceException();

            m_extResourceType[extension] = resourceType;
        }

        /// <summary>
        /// 注册一个通用资源类型
        /// </summary>
        public static void Register<T>(string extension) where T : Resource                                                                     
        {
            Register(extension, typeof(T));
        }

        public Uri                          Uri             { get; private set; }
        public bool                         Loaded          { get; private set; }
        public ResourceName                 Name            { get; private set; }


        private object                      m_result;

        public object                       GetResult()     
        {
            return OnRead();
        }

        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource              Create(Type resourceType, string path, params object[] args)                                                                                 
        {
            return Create(resourceType, Uri.From(path), args);
        }

        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource              Create(Type resourceType, Uri uri, params object[] args)
        {
            var resource = TypeService.CreateInstance(resourceType, args) as Resource;
            resource.Uri = uri;
            resource.Name = ResourceName.From(uri);

            // 在资源管理器内注册资源
            ResourceManager.Register(resource);

            return resource;
        }

        /// <summary>
        /// 读取资源
        /// Play.Studio.Workbench;Properties.Resources->test 读取程序集 Play.Studio.Workbench 中 Properties/Resources.rexs的test资源
        /// </summary>
        public static object                Read(string path, params object[] args)                                                             
        {
            return Read(Uri.From(path), args);
        }

        /// <summary>
        /// 读取资源
        /// </summary>
        public static object                Read(Uri uri, params object[] args)                                                                 
        {
            switch (uri.Extension) 
            {
                case Uri.ADDIN:
                    return Read(typeof(AddinResource), uri);
                default:
                    // 根据uri mime 获得资源类型
                    if (m_extResourceType.ContainsKey(uri.Extension))
                        return Read(m_extResourceType[uri.Extension], uri, args);
                    else
                        throw new ResourceException();
            }
        }

        /// <summary>
        /// 读取资源
        /// </summary>
        public static object                Read(Type resourceType, Uri uri, params object[] args)                                              
        {
            return (Current = Create(resourceType, uri, args)).GetResult();
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void                  AsyncRead(Type resourceType, Uri uri, Action<object> onHaveRead, params object[] args)              
        {
            FuncTask<Type, Uri, object[], object> task = new FuncTask<Type, Uri, object[], object>(Read);
            task.Completed += t =>
            {
                if (onHaveRead != null)
                    onHaveRead(t.Result);
            };
            task.Invoke(resourceType, uri, args);
        }

        /// <summary>
        /// 当读取资源
        /// </summary>
        protected internal object           OnRead()                                                               
        {
            if (m_result == null)
            {
                var resource = Name.Domain.Read(Uri);
                if (resource is Stream)
                {
                    // 读取资源
                    using (Stream stream = resource as Stream)
                    {
                        if (stream == null)
                        {
                            throw new ResourceException();
                        }
                        else
                        {
                            return m_result = OnRead(stream);
                        }
                    }
                }
                else 
                {
                    return m_result = resource;
                }
            }
            else 
            {
                return m_result;
            }
        }

        protected internal abstract object  OnRead(Stream stream);
    }

    /// <summary>
    /// 资源基类
    /// </summary>
    public abstract class Resource<T> : Resource                                                                                                
    {
        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource<T> Create(Type resourceType, string path, params object[] args)
        {
            return Create(resourceType, path, args);
        }

        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource<T>           Create(Type resourceType, Uri uri, params object[] args)
        {
            return Create(resourceType, uri, args);
        }

        /// <summary>
        /// 读取资源
        /// </summary>
        public static new T                 Read(Type resourceType, Uri uri, params object[] args)                                              
        {
            return (T)Resource.Read(resourceType, uri, args);
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void                  AsyncRead(Type resourceType, Uri uri, Action<T> onHaveRead, params object[] args)                   
        {
            Resource.AsyncRead(resourceType, uri, onHaveRead as Action<object>, args);
        }
    }

    /// <summary>
    /// 资源基类
    /// </summary>
    public abstract class Resource<TResource, TResourceType> : Resource<TResource> where TResourceType : Resource<TResource, TResourceType>     
    {
        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource<TResource, TResourceType> Create(string path, params object[] args)
        {
            return Create(typeof(TResourceType), path, args) as Resource<TResource, TResourceType>;
        }

        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource<TResource, TResourceType> Create(Uri uri, params object[] args)
        {
            return Create(typeof(TResourceType), uri, args) as Resource<TResource, TResourceType>;
        }

        /// <summary>
        /// 读取资源
        /// </summary>
        /// <param name="url">是否是url</param>
        /// <param name="async">是否异步加载</param>
        /// <returns></returns>
        public static TResource             Read(Uri uri, params object[] args)                                                                 
        {
            return Read(typeof(TResourceType), uri, args);
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void                  AsyncRead(Uri uri, Action<TResource> onHaveRead, params object[] args)                              
        {
            AsyncRead(typeof(TResourceType), uri, onHaveRead, args);
        }
    }
}
