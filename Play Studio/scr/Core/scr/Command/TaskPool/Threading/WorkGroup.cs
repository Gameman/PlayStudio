using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Command;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool.Threading
{
    class WorkGroup : IDisposable
    {
        #region Config

        public const int MinWokerCount = 0;
        public const int MaxWokerCount = 10;

        #endregion

        private HashSet<Task> m_removeList;
        private readonly WorkerList m_workerList;

        public WorkGroup()                  
        {
            m_workerList = new WorkerList();
            m_removeList = new HashSet<Task>();
        }

        #region Public Properties

        public bool IsBusy                  
        {
            get {
                return m_workerList.IsBusy;
            }
        }

        public int BusyDegree 
        {
            get {
                return m_workerList.BusyDegree;
            }
        }

        #endregion

        #region Pubilc Methods

        /// <summary>
        /// 执行一次工作
        /// </summary>
        public bool Execute(WorkItem workItem)               
        {
            Worker worker = m_workerList.GetIdeaWoker();
            if (worker == null) {
                if (m_workerList.Count < MaxWokerCount) {
                    worker = CreateWorker();
                    AddWorker(worker);
                }
                else {
                    return false;
                }
            }

            return worker.Run(workItem, workItem.Task.StartInfo.RunMode);
        }

        #endregion

        #region Private Methods

        private void AddWorker(Worker worker)   
        {
            m_workerList.AddWorker(worker);
        }

        private Worker GetIdeaWorker()          
        {
            return m_workerList.GetIdeaWoker();
        }

        private Worker CreateWorker()           
        {
            Worker worker = new Worker(this);
            worker.DoWork += new DoWorkEventHandler(m_worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
            return worker;
        }

        private void m_worker_DoWork(object sender, DoWorkEventArgs e)                          
        {
            WorkItem workItem = e.Argument as WorkItem;

            workItem.SetWorkItemState(WorkItemState.InProgress);

#if DEBUG
            workItem.Task.FireTaskStarted();
#else
            try
            {
                workItem.Task.FireTaskStarted();
            }
            catch (Exception ex) 
            {
                workItem.Task.FireTaskException(ex);
            }
#endif

            workItem.Execute();

            e.Result = workItem;
        }

        private void m_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)  
        {
            Worker worker = sender as Worker;
            WorkItem workItem = worker.AttachWorkItem;

            if (e.Cancelled) {
                workItem.SetWorkItemState(WorkItemState.Canceled);
            }
            else if (e.Error != null) {
                workItem.SetWorkItemState(WorkItemState.Canceled);
#if DEBUG
                throw e.Error;
#else
                workItem.Task.FireTaskException(e.Error);
#endif
            }
            else {
                workItem.SetWorkItemState(WorkItemState.Completed);

#if DEBUG
                workItem.Task.FireTaskCompleted();
#else
                try
                {
                    workItem.Task.FireTaskCompleted();
                }
                catch (Exception ex)
                {
                    workItem.Task.FireTaskException(ex);
                }
#endif

                if (workItem.Stuck)
                    workItem.SetOne();
            }

            workItem.Task.Dispose();
        }

        public void Dispose() 
        {
            m_workerList.Dispose();
        }

        #endregion
    }
}
