using System;

namespace Play.Studio.Module.Addins
{
    [Serializable]
    public class AddinException : Exception
    {
        public AddinException() { }
        public AddinException(string message) : base(message) { }
        public AddinException(string message, Exception inner) : base(message, inner) { }
        protected AddinException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
