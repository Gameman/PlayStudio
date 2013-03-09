using System;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool.Threading.Command
{
    class WorkItemQueue
    {
        /// <summary>
        /// Work items queue
        /// </summary>
        private readonly PriorityQueue _tasks = new PriorityQueue();

        #region Public Methods

        public bool Enqueue(WorkItem workItem)              
        {
            // A work item cannot be null, since null is used in the
            // WaitForWorkItem() method to indicate timeout or cancel
            if (null == workItem) {
                throw new ArgumentNullException("taskInfo", "taskInfo cannot be null");
            }

            ValidateNotDisposed();

            _tasks.Enqueue(workItem);

            return true;
        }

        public WorkItem Dequeue()                           
        {
            return _tasks.Dequeue() as WorkItem;
        }

        public int Count                                    
        {
            get {
                return _tasks.Count;
            }
        }

        #endregion

        #region Private Methods

        private void Cleanup()
        {
            lock (this) {
                _tasks.Clear();
            }
        }

        #endregion

        #region IDisposable Members

        bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed) {
                Cleanup();
            }
            _isDisposed = true;
        }

        private void ValidateNotDisposed()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().ToString(), "The TheadPool has been shutdown");
            }
        }

        #endregion
    }
}
