using System;
using System.Collections.Generic;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool.Threading.Command
{
    class WorkerList : IDisposable
    {
        List<Worker> _workers;
        object _lock;

        public WorkerList() 
        {
            _lock = new object();
            _workers = new List<Worker>();
        }

        public bool IsBusy 
        {
            get {
                lock (_lock) {
                    for (int i = 0; i < _workers.Count; i++) {
                        if (!_workers[i].IsBusy)
                            return true;
                    }
                }

                return false;
            }
        }

        public int BusyDegree 
        {
            get {
                int p = 0;
                lock (_lock) {
                    for (int i = 0; i < _workers.Count; i++) {
                        if (!_workers[i].IsBusy)
                            p++;
                    }
                }

                return p;
            }
        }

        public Worker this[int index] 
        {
            get {
                lock (_lock)
                    return _workers[index];
            }
        }

        public void AddWorker(Worker worker) 
        {
            lock (_lock) {
                _workers.Add(worker);
            }
        }

        public void RemoveWorker(Worker worker) 
        {
            lock (_lock) {
                worker.Dispose();
                _workers.Remove(worker);
            }
        }

        public Worker GetIdeaWoker() 
        {
            lock (_lock) {
                for (int i = 0; i < _workers.Count; i++) {
                    if (!_workers[i].IsBusy)
                        return _workers[i];
                }
            }

            return null;
        }

        public int Count 
        {
            get {
                lock (_lock)
                    return _workers.Count;
            }
        }

        public void Dispose() 
        {
            foreach (var worker in _workers)
                worker.Dispose();
        }
    }
}
