using System;
using Play.Studio.Core.Command.TaskPool.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;
using Play.Studio.Core.Command.TaskPool.Threading.Command;

namespace Play.Studio.Core.Command.TaskPool
{
    public abstract class Task : IDisposable
    {
        volatile string message;
        volatile int    progress;
        WorkItem        workItem;
        TaskStartInfo   startInfo;
        Delegate        func;

        protected void SetFunc(Delegate func) 
        {
            this.func = func;
        }

        protected Task() 
        {
        }

        public Task(Delegate func) 
        {
            this.func = func;
        }

        protected virtual object Invoke(params object[] args)                                     
        {
            return Invoke(TaskStartInfo.Default, args);
        }

        protected virtual object Invoke(TaskStartInfo startInfo, params object[] args)            
        {
            if (State == TaskState.Completed || 
                State == TaskState.Canceled || 
                State == TaskState.Idea) {

                    if (this.workItem != null)
                        workItem.Dispose();

                    this.workItem = new WorkItem(func, args);
                    this.workItem.Task = this;
                    this.workItem.Initialize();
                    this.startInfo = startInfo;
                    this.startInfo.task = this;

                    TaskPool.Queue(this);

                    return workItem.Result;
            }
            else {
                throw new WorkItemInvokeException("state");
            }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public bool Cancel()                                            
        {
            return WorkItem.Cancel(true);
        }

        /// <summary>
        /// 挂起任务
        /// </summary>
        public void Suspend()                                           
        {
            WorkItem.CaneclThread.Suspend();
            WorkItem.SetWorkItemState(WorkItemState.Suspend);
        }

        /// <summary>
        /// 恢复挂起任务
        /// </summary>
        public void Resume()                                            
        {
            WorkItem.CaneclThread.Resume();
            WorkItem.SetWorkItemState(WorkItemState.InProgress);
        }

        /// <summary>
        /// 重置任务
        /// </summary>
        public bool Restart()                                           
        {
            return Restart(startInfo);
        }

        public bool Restart(TaskStartInfo startInfo)                    
        {
            bool result;
            if (result = Cancel()) {
                //Invoke(startInfo);
                TaskPool.Queue(this);
            }

            return result;
        }

        public TaskStartInfo StartInfo                                  
        {
            get {
                return startInfo;
            }
        }

        internal WorkItem WorkItem                                      
        {
            get {
                return workItem;
            }
        }

        public TaskState State                                          
        {
            get {
                if (workItem == null)
                    return TaskState.Idea;

                return (TaskState)(int)workItem.WorkItemState;
            }
        }

        public bool IsCanceled                                          
        {
            get {
                return State == TaskState.Canceled;
            }
        }

        public bool IsCompleted                                         
        {
            get {
                return State == TaskState.Completed;
            }
        }

        public int Progress                                             
        {
            get {
                return progress;
            }
        }

        public string Message 
        {
            get {
                return message;
            }
        }

        public TimeSpan ProcessTime                                     
        {
            get {
                return WorkItem.ProcessTime;
            }
        }

        protected void ReportProgress(int progress)
        {
            ReportProgress(progress, string.Empty);
        }

        protected void ReportProgress (int progress, string message)                                  
        {
            this.progress = progress;
            this.message = message;

            if (_taskReportProgressEvent != null)
                _taskReportProgressEvent(this);
        }

        #region Event

        private event TaskStateCallback _taskStartedEvent;
        private event TaskStateCallback _taskCompletedEvent;
        private event TaskStateCallback _taskReportProgressEvent;
        private event TaskExceptionCallback _taskCatchException;

        public event TaskStateCallback Started
        {
            add
            {
                _taskStartedEvent += value;
            }
            remove
            {
                _taskStartedEvent -= value;
            }
        }

        public event TaskStateCallback Completed
        {
            add
            {
                _taskCompletedEvent += value;
            }
            remove
            {
                _taskCompletedEvent -= value;
            }
        }

        public event TaskStateCallback ProgressChanged
        {
            add 
            {
                _taskReportProgressEvent += value;
            }
            remove 
            {
                _taskReportProgressEvent -= value;
            }
        }

        public event TaskExceptionCallback CatchException 
        {
            add {
                _taskCatchException += value;
            }
            remove {
                _taskCatchException -= value;
            }
        }

        internal void FireTaskCompleted()
        {
            OnTaskCompleted();
        }

        protected virtual void OnTaskCompleted() 
        {
            if (null != _taskCompletedEvent)
            {
                _taskCompletedEvent(this);
            }
        }

        internal void FireTaskStarted()
        {
            OnTaskStarted();
        }

        protected virtual void OnTaskStarted() 
        {
            if (null != _taskStartedEvent)
            {
                _taskStartedEvent(this);
            }
        }

        internal void FireTaskException(Exception ex) 
        {
            OnTaskException(ex);
        }

        protected virtual void OnTaskException(Exception ex) 
        {
            if (_taskCatchException != null)
            {
                _taskCatchException(this, ex);
            }
        }

        #endregion

        #region IDisposable Members

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed) {
                if (workItem != null)
                    workItem.Dispose();
            }
            _isDisposed = true;
        }

        public void Dispose(bool disposeEvents) 
        {
            if (disposeEvents)
            {
                _taskStartedEvent = null;
                _taskCatchException = null;
                _taskCompletedEvent = null;
                _taskReportProgressEvent = null;
            }

            Dispose();

        }

        private void ValidateNotDisposed()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().ToString(), "The Task has been shutdown");
            }
        }

        #endregion
    }
}
