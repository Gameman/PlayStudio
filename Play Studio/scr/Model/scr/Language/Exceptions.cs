using System;

namespace Play.Studio.Module.Language
{
    /// <summary>
    /// 语言包异常
    /// </summary>
    [Serializable]
    public class LanguageException : Exception
    {
        public LanguageException() { }
        public LanguageException(string message) : base(message) { }
        public LanguageException(string message, Exception inner) : base(message, inner) { }
        protected LanguageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// 语言包读取异常
    /// </summary>
    [Serializable]
    public class LanguageReadException : LanguageException
    {
        public LanguageReadException() { }
        public LanguageReadException(string message) : base(message) { }
        public LanguageReadException(string message, Exception inner) : base(message, inner) { }
        protected LanguageReadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
