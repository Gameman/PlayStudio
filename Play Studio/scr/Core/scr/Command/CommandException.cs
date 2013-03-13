using System;
using System.Runtime.Serialization;

namespace Play.Studio.Core.Command
{
    public class CommandException : Exception
    {
        public CommandException() : base() { }

        public CommandException(string message) : base(message) { }

        protected CommandException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public CommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
