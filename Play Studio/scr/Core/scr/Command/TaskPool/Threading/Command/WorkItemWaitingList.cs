using System.Diagnostics;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool.Threading.Command
{
    class WorkItemWaitingList
    {
        WorkItemQueue _taskInfos = new WorkItemQueue();

        private readonly object _lock;

        public WorkItemWaitingList() 
        {
            _lock = new object();
        }

        public object SyncRoot
        {
            get { return _lock; }
        }

        public void Add(WorkItem workItem) 
        {
            lock (_lock)
                _taskInfos.Enqueue(workItem);
        }

        public WorkItem Get() 
        {
            lock (_lock) {
                WorkItem target = null;

                if (_taskInfos.Count > 0) {
                    target = _taskInfos.Dequeue();
                    if (target.Task.StartInfo.Trigger != null) {
                        if (!target.Task.StartInfo.Trigger.Invoke()) { 
                            _taskInfos.Enqueue(target);
                            target = null;
                        }
                    }
                }

                return target;
            }
        }

        public int Count 
        {
            get {
                lock (_lock)
                    return _taskInfos.Count;
            }
        }
    }
}
