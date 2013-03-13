using System;

namespace Play.Studio.Core.Command.TaskPool
{
    public delegate void TaskStateCallback(Task task);
    
    public delegate void TaskExceptionCallback(Task task, Exception ex);
}
