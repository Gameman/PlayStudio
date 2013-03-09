using System;

namespace Play.Studio.Model.Templates
{
    [Serializable]
    public class TemplateException : Exception
    {
        public TemplateException() { }
        public TemplateException(string message) : base(message) { }
        public TemplateException(string message, Exception inner) : base(message, inner) { }
        protected TemplateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TemplateLoadException : TemplateException
    {
        public TemplateLoadException() { }
        public TemplateLoadException(string message) : base(message) { }
        public TemplateLoadException(string message, Exception inner) : base(message, inner) { }
        protected TemplateLoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
