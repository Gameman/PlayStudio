namespace Play.Studio.Core.Command.TaskPool.Threading.Internal
{
    public enum WorkItemRunMode
    {
        Async,
        Thread,
        CurrentThread,
        BackgroundThread,
    }
}
