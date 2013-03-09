using System;

namespace Play.Studio.Core.Command.TaskPool
{
    class TaskException : Exception
    {
        public TaskException(string message) : base(message) { }
    }
}
