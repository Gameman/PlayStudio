using System;

namespace Play.Studio.Module.Resource
{
    [Serializable]
    public class ResourceException : Exception
    {
        public ResourceException() { }
        public ResourceException(string message) : base(message) { }
        public ResourceException(string message, Exception inner) : base(message, inner) { }
        protected ResourceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
