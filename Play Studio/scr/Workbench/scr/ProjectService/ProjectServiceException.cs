using System;
using System.Runtime.Serialization;

namespace Play.Studio.Core.Services
{
    class ProjectServiceException : Exception
    {
        public ProjectServiceException() : base()
		{
		}
		
		public ProjectServiceException(Type serviceType) : base("Required service not found: " + serviceType.FullName)
		{
		}
		
		public ProjectServiceException(string message) : base(message)
		{
		}
		
		public ProjectServiceException(string message, Exception innerException) : base(message, innerException)
		{
		}

        protected ProjectServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
		}
    }
}
