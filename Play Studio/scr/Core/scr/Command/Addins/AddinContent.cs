using System;
using Play.Studio.Core.Services;

namespace Play.Studio.Core.Command.Addins
{
    public class AddinContent : IDisposable
    {
        public Type         ContentType     { get; private set; }
        public string       Name            { get; private set; }
        public string       Category        { get; private set; }
        public string       Description     { get; private set; }

        public AddinContent(Type type) 
        {
            ContentType = type; 
        }

        public virtual object GetInstance(params object[] args) 
        {
            return TypeService.CreateInstance(ContentType, args);
        }

        public virtual void Dispose() 
        { 
        }
    }

    public class AddinContent<T> : AddinContent 
    {
        public AddinContent() 
            : base(typeof(T))
        {
        }

        public new T GetInstance() 
        {
            return (T)base.GetInstance();
        }
    }
}
