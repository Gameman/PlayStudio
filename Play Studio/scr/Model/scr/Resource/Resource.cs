using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Play.Studio.Core.Command.TaskPool;
using Play.Studio.Core.Services;

namespace Play.Studio.Module.Resource
{
    /// <summary>
    /// 资源接口
    /// </summary>
    interface IResource                                                                                                                  
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
    }

    /// <summary>
    /// 资源
    /// </summary>
    public abstract class Resource : IResource                                                                                                  
    {
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

        public Uri                          Uri             { get { return Name.Uri; } }
        public bool                         Loaded          { get; private set; }
        public ResourceName                 Name            { get; private set; }


        private object                      m_result;

        public object                       GetResult()     
        {
            if (m_result == null)
                m_result = OnRead(Uri);

            return m_result;
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
            return Create(resourceType, ResourceName.From(uri), args);
        }

        /// <summary>
        /// 创建资源
        /// </summary>
        public static Resource              Create(Type resourceType, ResourceName resourceName, params object[] args)                                                                   
        {
            var resource = TypeService.CreateInstance(resourceType, args) as Resource;
            resource.Name = resourceName;
            return resource;
        }

        /// <summary>
        /// 读取资源
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
            // 根据uri mime 获得资源类型
            if (m_extResourceType.ContainsKey(uri.Extension))
                return Read(m_extResourceType[uri.Extension], uri, args);
            else
                throw new ResourceException();
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
        /// 保存
        /// </summary>
        public static void                  Save(object sender, Uri uri, params object[] args)                                                  
        {
            (sender as Resource).OnSave(uri, args);
        }

        /// <summary>
        /// 异步保存
        /// </summary>
        public static void                  AsyncSave(object sender, Uri uri, Action<object> onHaveRead, params object[] args)                  
        {
            ActionTask<object, Uri, object[]> task = new ActionTask<object, Uri, object[]>(Save);
            task.Completed += t =>
            {
                if (onHaveRead != null)
                    onHaveRead(t.Result);
            };
            task.Invoke(sender, uri, args);
        }

        /// <summary>
        /// 当读取资源
        /// </summary>
        protected internal object           OnRead(Uri uri)                                                               
        {
            // 得到资源名
            var domain = ResourceDomain.Get(uri);

            // 读取资源
            using (Stream stream = domain.Read(uri))
            {
                if (stream == null)
                {
                    throw new ResourceException();
                }
                else
                {
                    return OnRead(stream);
                }
            }
        }

        protected internal abstract object  OnRead(Stream stream);

        /// <summary>
        /// 保存
        /// </summary>
        protected internal void             OnSave(Uri uri, params object[] args)                                                               
        {
            OnSave(new FileStream(uri.FileName, FileMode.Open, FileAccess.ReadWrite), args);
        }

        /// <summary>
        /// 保存
        /// </summary>
        protected internal abstract void    OnSave(Stream stream, params object[] args);
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
        /// 创建资源
        /// </summary>
        public static Resource<T> Create(Type resourceType, ResourceName name, params object[] args)
        {
            return Create(resourceType, name, args);
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

        /// <summary>
        /// 保存
        /// </summary>
        public static void                  Save(T sender, Uri uri, params object[] args)                                                       
        {
            Save(sender, uri, args);
        }
        
        /// <summary>
        /// 异步保存
        /// </summary>
        public static void                  AsyncSave(T sender, Uri uri, Action<T> onHaveRead, params object[] args)                            
        {
            ActionTask<T, Uri, object[]> task = new ActionTask<T, Uri, object[]>(Save);
            task.Completed += t =>
            {
                if (onHaveRead != null)
                    onHaveRead((T)t.Result);
            };
            task.Invoke(sender, uri, args);
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
        /// 创建资源
        /// </summary>
        public static Resource<TResource, TResourceType> Create(ResourceName name, params object[] args)
        {
            return Create(typeof(TResourceType), name, args) as Resource<TResource, TResourceType>;
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
