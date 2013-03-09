using System.Threading;

namespace Play.Studio.Core.Command.TaskPool.Threading.Internal
{
    abstract class WorkProcessor
    {
        protected ParameterizedThreadStart Callback { get; private set; }

        public WorkProcessor(ParameterizedThreadStart callback) 
        {
            Callback = callback;
        }

        public abstract void Run(WorkItem workItem);
    }

    class AsyncWorkProcessor : WorkProcessor 
    {
        public AsyncWorkProcessor(ParameterizedThreadStart callback)
            : base(callback)
        { 
        }

        public override void Run(WorkItem workItem)
        {
            Callback.BeginInvoke(workItem, null, null);
        }
    }

    class BackThreadProcessor : WorkProcessor 
    {
        public BackThreadProcessor(ParameterizedThreadStart callback)
            : base(callback)
        { 
        }

        public override void Run(WorkItem workItem)
        {
            Thread thread = new Thread(Callback);
            thread.Priority = workItem.Priority;
            
            thread.IsBackground = true;
            thread.Start(workItem);
        }
    }

    class ThreadProcessor : WorkProcessor
    {
        public ThreadProcessor(ParameterizedThreadStart callback)
            : base(callback)
        { 
        }

        public override void Run(WorkItem workItem)
        {
            Thread thread = new Thread(Callback);
            thread.Priority = workItem.Priority;

            thread.IsBackground = false;
            thread.Start(workItem);
        }
    }

    class CurrentProcessor : WorkProcessor 
    {
        public CurrentProcessor(ParameterizedThreadStart callback)
            : base(callback)
        {
        }

        public override void Run(WorkItem workItem)
        {
            Callback.Invoke(workItem);
        }
    }

}
