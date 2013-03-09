namespace Play.Studio.Core.Command.TaskPool
{
    public enum TaskState
    {
        Idea = 0,
        InQueue = 1,    // Nexts: InProgress, Canceled
        InProgress = 2,    // Nexts: Completed, Canceled
        Completed = 3,    // Stays Completed
        Canceled = 4,    // Stays Canceled
        Suspend = 5,
    }
}
