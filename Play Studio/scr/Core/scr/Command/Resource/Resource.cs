using System;
using System.Collections.Generic;
using System.IO;
using Play.Studio.Core.Command.TaskPool;
using Play.Studio.Core.Services;
using System.Threading;

namespace Play.Studio.Core
{
    /// <summary>
    /// 资源接口
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// 资源地址
        /// </summary>
        Uri      Uri         { get; }

        /// <summary>
        /// 资源是否已被加载
        /// </summary>
        bool     Loaded      { get; }
    }

    public abstract class Resource : IResource 
    {
        private static Dictionary<int, Resource> m_currentResources = new Dictionary<int, Resource>();
        public static Resource Current                                                                                  
        {
            get 
            {
                if (!m_currentResources.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    m_currentResources[Thread.CurrentThread.ManagedThreadId] = null;

                return m_currentResources[Thread.CurrentThread.ManagedThreadId];
            }
            private set 
            {
                if (!m_currentResources.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    m_currentResources[Thread.CurrentThread.ManagedThreadId] = null;

                m_currentResources[Thread.CurrentThread.ManagedThreadId] = value;
            }
            
        }

        public Uri  Uri         { get; private set; }
        public bool Loaded      { get; private set; }

        /// <summary>
        /// 读取资源
        /// </summary>
        /// <param name="url">是否是url</param>
        /// <param name="async">是否异步加载</param>
        /// <returns></returns>
        public static object    Read(Type resourceType, Uri uri, params object[] args)                                  
        {
            return (Current = (TypeService.CreateInstance(resourceType, args) as Resource)).OnRead(uri);
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void      AsyncRead(Type resourceType, Uri uri, Action<object> onHaveRead, params object[] args)  
        {
            FuncTask<Type, Uri, object[], object> task = new FuncTask<Type, Uri, object[], object>(Read);
            task.Completed += t =>
            {
                if (onHaveRead != null)
                    onHaveRead(t.WorkItem.Result);
            };
            task.Invoke(resourceType, uri, args);
        }

        protected internal abstract object OnRead(Uri uri);
    }

    /// <summary>
    /// 资源基类
    /// </summary>
    public abstract class Resource<T> : Resource
    {
        /// <summary>
        /// 读取资源
        /// </summary>
        /// <param name="url">是否是url</param>
        /// <param name="async">是否异步加载</param>
        /// <returns></returns>
        public static T     Read(Type resourceType, Uri uri, params object[] args)                                      
        {
            return (T)Resource.Read(resourceType, uri, args);
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void  AsyncRead(Type resourceType, Uri uri, Action<T> onHaveRead, params object[] args)           
        {
            Resource.AsyncRead(resourceType, uri, onHaveRead as Action<object>, args);
        }

        protected internal override object OnRead(Uri uri)
        {
            using (Stream stream = new FileStream(uri.Path, FileMode.Open))
            {
                if (stream == null)
                {
                    throw new ResourceException();
                }
                else
                {
                    return (T)OnRead(stream);
                }
            }
        }

        protected internal abstract T OnRead(Stream stream);
    }

    /// <summary>
    /// 资源基类
    /// </summary>
    public abstract class Resource<TResource, TResourceType> : Resource<TResource> where TResourceType : Resource<TResource, TResourceType>
    {
        /// <summary>
        /// 读取资源
        /// </summary>
        /// <param name="url">是否是url</param>
        /// <param name="async">是否异步加载</param>
        /// <returns></returns>
        public static TResource Read(Uri uri, params object[] args) 
        {
            return Read(typeof(TResourceType), uri, args);
        }

        /// <summary>
        /// 异步读取
        /// </summary>
        public static void AsyncRead(Uri uri, Action<TResource> onHaveRead, params object[] args) 
        {
            AsyncRead(typeof(TResourceType), uri, onHaveRead, args);
        }
    }
}
