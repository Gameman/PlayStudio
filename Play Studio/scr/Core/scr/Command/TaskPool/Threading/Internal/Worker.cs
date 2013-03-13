namespace Play.Studio.Core.Command.TaskPool.Threading.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Threading;

    [DefaultEvent("DoWork"), HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    class Worker : IDisposable
    {
        private AsyncOperation asyncOperation;

        private bool isRunning;
        private readonly SendOrPostCallback operationCompleted;

        private WorkItem attachWorkItem;

        private readonly ParameterizedThreadStart threadStart;
        private readonly Dictionary<WorkItemRunMode, WorkProcessor> processors;

        public WorkItem AttachWorkItem                                      
        {
            get {
                return attachWorkItem;
            }
        }

        public WorkGroup WorkGroup
        {
            get;
            private set;
        }

        public event DoWorkEventHandler DoWork;

        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        public Worker(WorkGroup group)                                                     
        {
            WorkGroup = group;

            this.threadStart = new ParameterizedThreadStart(this.WorkerThreadStart);
            this.operationCompleted = new SendOrPostCallback(this.AsyncOperationCompleted);
            this.processors = new Dictionary<WorkItemRunMode, WorkProcessor>(3);

            this.processors.Add(WorkItemRunMode.Async, new AsyncWorkProcessor(this.threadStart));
            this.processors.Add(WorkItemRunMode.Thread, new ThreadProcessor(this.threadStart));
            this.processors.Add(WorkItemRunMode.CurrentThread, new CurrentProcessor(this.threadStart));
            this.processors.Add(WorkItemRunMode.BackgroundThread, new BackThreadProcessor(this.threadStart));
        }

        private void AsyncOperationCompleted(object arg)                                
        {
            this.OnRunWorkerCompleted((RunWorkerCompletedEventArgs)arg);
        }

        protected virtual void OnDoWork(DoWorkEventArgs e)                              
        {
            if (DoWork != null)
            {
                DoWork(this, e);
            }
        }

        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)      
        {
            if (RunWorkerCompleted != null)
            {
                RunWorkerCompleted(this, e);
            }

            this.isRunning = false;
        }

        public bool Run(WorkItem workItem, WorkItemRunMode mode)                                              
        {
            try 
            {
                if (this.isRunning) 
                {
                    throw new InvalidOperationException("BackgroundWorker_WorkerAlreadyRunning");
                }
                this.isRunning = true;
                this.asyncOperation = AsyncOperationManager.CreateOperation(null);
                this.attachWorkItem = workItem;

                this.processors[mode].Run(workItem);
                return true;
            }
            catch (Exception ex) 
            { 
                return false; 
            }
        }

        private void WorkerThreadStart(object argument)                                 
        {
            object result = null;
            Exception error = null;
            bool cancelled = false;
           
            try {
                DoWorkEventArgs e = new DoWorkEventArgs(argument);
                this.OnDoWork(e);
                if (e.Cancel) {
                    cancelled = true;
                }
                else {
                    result = e.Result;
                }
            }
            catch (Exception exception2) 
            {
                error = exception2;
            }
            RunWorkerCompletedEventArgs arg = new RunWorkerCompletedEventArgs(result, error, cancelled);
            this.asyncOperation.PostOperationCompleted(this.operationCompleted, arg);
        }

        public bool IsBusy                                                              
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get {
                return this.isRunning;
            }
        }

        public void Dispose() 
        {
            AttachWorkItem.Cancel(true);

            if (this.AttachWorkItem != null)
                this.AttachWorkItem.Dispose();
        }

    }
}

