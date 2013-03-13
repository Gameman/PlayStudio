using System;

namespace Play.Studio.View.Converters
{
    [Serializable]
    public class TemplateConverterException : Exception
    {
        public TemplateConverterException() { }
        public TemplateConverterException(string message) : base(message) { }
        public TemplateConverterException(string message, Exception inner) : base(message, inner) { }
        protected TemplateConverterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TemplateConverterNotSupportException : Exception
    {
        public TemplateConverterNotSupportException() { }
        public TemplateConverterNotSupportException(string message) : base(message) { }
        public TemplateConverterNotSupportException(string message, Exception inner) : base(message, inner) { }
        protected TemplateConverterNotSupportException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
