using System;
using System.Collections.Generic;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Command;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool
{
    static class TaskPool
    {
        #region Config

        const int MinThreadCount = 1;
        const int MaxThreadCount = 3;
        const int IdeaInterval = 20;

        #endregion

        static int _nextGroup;

        /*
        static STPStartInfo threadPoolInfo = new STPStartInfo() { 
            StartSuspended = true,
            MinWorkerThreads = 2,
            MaxWorkerThreads = 10,
            IdleTimeout = 10000,
        };
        */

        static readonly HashSet<Task> m_removeList = new HashSet<Task>();
        static readonly List<WorkGroup> workGroups = new List<WorkGroup>(MaxThreadCount);
        static readonly WorkItemWaitingList m_taskInfosWaitingList = new WorkItemWaitingList();

        public static bool Cancel(Task task)
        {
            lock (m_taskInfosWaitingList.SyncRoot) {
                if (task.WorkItem.CaneclThread != null) {
                    try {
                        task.WorkItem.CaneclThread.Abort();
                        return true;
                    }
                    catch { return false; }
                }
                else {
                    m_removeList.Add(task);
                    return true;
                }
            }
        }

        /// <summary>
        /// 重置任务
        /// </summary>
        public static bool Restart(Task task)
        {
            if (Cancel(task)) {
                Queue(task);

                return true;
            }
            return false;
        }


        static TaskPool() 
        {
            for (int i = 0; i < MaxThreadCount; i++)
                workGroups.Add(new WorkGroup());

            Thread thread = new Thread(DoGroupThreading);
            thread.Start();

            // 子线程异常捕获
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e) 
        {
            //Application.ApplicationExit -= Application_ApplicationExit;

            // 停止所有线程
            foreach (var workGroup in workGroups)
                workGroup.Dispose();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) 
        {
            
        }

        public static void Queue(Task task)
        {
            Queue(task, Thread.CurrentThread);
        }

        public static void Queue(Task task, Thread executingThread)
        {
            task.WorkItem.ExecutingThread = executingThread;

            task.WorkItem.InQueue();
            m_taskInfosWaitingList.Add(task.WorkItem);

            if (task.WorkItem.Stuck) {
                task.WorkItem.WaitOne(true);
            }
        }

        private static WorkGroup GetOptimalWorkGroup()      
        {
            WorkGroup worker = null;
            int index = 0;
            if (workGroups[_nextGroup].IsBusy) {
                int count = -1;
                for (int i = 0; i < workGroups.Count; i++) {
                    if (!workGroups[i].IsBusy) {
                        return workGroups[i];
                    }
                    else {
                        var busyDegree = workGroups[i].BusyDegree;
                        if (busyDegree > count) {
                            index = i;
                            count = busyDegree;
                        }
                    }
                }
            }
            else {
                index = _nextGroup;
            }

            worker = workGroups[index];

            // get next inex
            if (index == workGroups.Count - 1)
                _nextGroup = 0;
            else
                _nextGroup = index + 1;

            return worker;
        }

        private static void DoOne() 
        {
            WorkItem workItem = GetOne();
            if (workItem != null) {
                if (!GetOptimalWorkGroup().Execute(workItem)) {
                    m_taskInfosWaitingList.Add(workItem);
                }
            }
        }

        private static void DoAll()
        {
            for (int i = 0; i < workGroups.Count; i++) {
                WorkItem workItem = GetOne();
                if (workItem != null) {
                    if (!workGroups[i].Execute(workItem)) {
                        m_taskInfosWaitingList.Add(workItem);
                    }
                }
                else {
                    break;
                }
            }
        }

        private static void DoGroupThreading()              
        {
            while (true) {
                if (m_taskInfosWaitingList.Count > 0)
                {
                    if (m_taskInfosWaitingList.Count > workGroups.Count)
                        DoAll();
                    else
                        DoOne();
                }

                Thread.Sleep(IdeaInterval);
            }
        }

        private static WorkItem GetOne() 
        {
            WorkItem workItem = m_taskInfosWaitingList.Get();

            while (workItem != null && m_removeList.Contains(workItem.Task)) {
                m_removeList.Remove(workItem.Task);
                workItem = m_taskInfosWaitingList.Get();
            }

            return workItem;

        }
    }
}
