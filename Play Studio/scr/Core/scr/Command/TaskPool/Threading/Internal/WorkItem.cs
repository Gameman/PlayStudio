using System;
using System.Diagnostics;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Command;

namespace Play.Studio.Core.Command.TaskPool.Threading.Internal
{
    class WorkItem : IHasPriority, IDisposable
    {
        private static bool IsValidStatesTransition(WorkItemState currentState, WorkItemState nextState)
        {
            bool valid = false;

            switch (currentState) {
                case WorkItemState.Idea:
                    valid = WorkItemState.InQueue == nextState;
                    break;
                case WorkItemState.InQueue:
                    valid = (WorkItemState.InProgress == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                case WorkItemState.InProgress:
                    valid = (WorkItemState.Completed == nextState) || (WorkItemState.Suspend == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                case WorkItemState.Suspend:
                    valid = WorkItemState.InProgress == nextState;
                    break;
                case WorkItemState.Completed:
                case WorkItemState.Canceled:
                    // Cannot be changed
                    break;
                default:
                    // Unknown state
                    Debug.Assert(false);
                    break;
            }

            return valid;
        }

        /// <summary>
        /// Hold the state of the work item
        /// </summary>
        private WorkItemState _workItemState;

        /// <summary>
        /// A ManualResetEvent to indicate that the result is ready
        /// </summary>
        private ManualResetEvent _workItemCompleted;

        /// <summary>
        /// A reference count to the _workItemCompleted. 
        /// When it reaches to zero _workItemCompleted is Closed
        /// </summary>
        private int _workItemCompletedRefCount;

        /// <summary>
        /// The thread that executes this workitem.
        /// This field is available for the period when the work item is executed, before and after it is null.
        /// </summary>
        private Thread _executingThread;

        /// <summary>
        /// The absulote time when the work item will be timeout
        /// </summary>
        private long _expirationTime;

        private object _result;
        private readonly bool _stuck;
        private readonly object[] _state;
        private readonly WorkItemCallback _callback;
        private Exception _exception;

        /// <summary>
        /// Stores how long the work item waited on the stp queue
        /// </summary>
        private Stopwatch _waitingOnQueueStopwatch;

        /// <summary>
        /// Stores how much time it took the work item to execute after it went out of the queue
        /// </summary>
        private Stopwatch _processingStopwatch;


        internal WorkItem(Delegate func, object[] state)              
        {
            _state = state;
            _stuck = func.Method.ReturnType != typeof(void);

            _callback = new WorkItemCallback(
                delegate {
                    if (_stuck) {
                        return func.Method.Invoke(func.Target, state);
                    }
                    else {
                        func.Method.Invoke(func.Target, state);
                        return null;
                    }
                });
        }



        internal void Initialize()
        {
            // The _workItemState is changed directly instead of using the SetWorkItemState
            // method since we don't want to go throught IsValidStateTransition.
            _workItemState = WorkItemState.Idea;

            _workItemCompleted = null;
            _workItemCompletedRefCount = 0;
            _waitingOnQueueStopwatch = new Stopwatch();
            _processingStopwatch = new Stopwatch();
            _expirationTime = long.MaxValue;

                /*
                _workItemInfo.Timeout > 0 ?
                DateTime.UtcNow.Ticks + _workItemInfo.Timeout * TimeSpan.TicksPerMillisecond :
                long.MaxValue;
                */
        }

        #region Public Properies

        public Exception Exception                  
        {
            get {
                return _exception;
            }
            internal set {
                _exception = value;
            }
        }

        public TimeSpan WaitingTime                 
        {
            get
            {
                return _waitingOnQueueStopwatch.Elapsed;
            }
        }

        public TimeSpan ProcessTime                 
        {
            get {
                return _processingStopwatch.Elapsed;
            }
        }

        public ThreadPriority Priority            
        {
            get;
            internal set;
        }

        public object Result                        
        {
            get {
                return _result;
            }
        }

        public Task Task                            
        {
            get;
            internal set;
        }

        public Thread ExecutingThread               
        {
            get {
                return _executingThread;
            }
            internal set {
                _executingThread = value;
            }
        }

        public Thread CaneclThread
        {
            get;
            private set;
        }

        public WorkItemState WorkItemState 
        {
            get {
                return GetWorkItemState();
            }
        }

        internal bool Stuck
        {
            get {
                return _stuck;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
		/// Execute the work item and the post execute
		/// </summary>
        public void Execute() 
        {
            CaneclThread = Thread.CurrentThread;

            _waitingOnQueueStopwatch.Stop();
            _processingStopwatch.Start();

            _result = _callback(_state);

            _processingStopwatch.Stop();
        }

        #endregion

        #region Private Methods

        internal void InQueue()                                         
        {
            SetWorkItemState(WorkItemState.InQueue);
            _waitingOnQueueStopwatch.Start();
        }

        /// <summary>
        /// Cancel the work item if it didn't start running yet.
        /// </summary>
        /// <returns>Returns true on success or false if the work item is in progress or already completed</returns>
        internal bool Cancel(bool abortExecution)                       
        {
#if (_WINDOWS_CE)
            if(abortExecution)
            {
                throw new ArgumentOutOfRangeException("abortExecution", "WindowsCE doesn't support this feature");
            }
#endif
            bool success = false;
            bool signalComplete = false;

            lock (this) {
                switch (GetWorkItemState()) {
                    case WorkItemState.Canceled:
                        //Debug.WriteLine("Work item already canceled");
                        success = true;
                        break;
                    case WorkItemState.Completed:
                        //Debug.WriteLine("Work item cannot be canceled");
                        break;
                    case WorkItemState.InProgress:
                        if (abortExecution) {
                            Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);
                            if (null != executionThread) {
                                CaneclThread.Abort();
                                //executionThread.Abort(); // "Cancel"
                                success = true;
                                signalComplete = true;
                            }
                        }
                        else {
                            success = true;
                            signalComplete = true;
                        }
                        break;
                    case WorkItemState.InQueue:
                        // Signal to the wait for completion that the work
                        // item has been completed (canceled). There is no
                        // reason to wait for it to get out of the queue
                        signalComplete = true;
                        //Debug.WriteLine("Work item canceled");
                        success = true;
                        break;
                }

                if (signalComplete) {
                    SignalComplete(true);
                }
            }
            return success;
        }

        /// <summary>
        /// Signals that work item has been completed or canceled
        /// </summary>
        /// <param name="canceled">Indicates that the work item has been canceled</param>
        private void SignalComplete(bool canceled)                      
        {
            SetWorkItemState(canceled ? WorkItemState.Canceled : WorkItemState.Completed);
            lock (this) {
                // If someone is waiting then signal.
                if (null != _workItemCompleted) {
                    _workItemCompleted.Set();
                }
            }
        }

        private WorkItemState GetWorkItemState()                        
        {
            lock (this) {
                if (WorkItemState.Completed == _workItemState) {
                    return _workItemState;
                }

                long nowTicks = DateTime.UtcNow.Ticks;

                if (WorkItemState.Canceled != _workItemState && nowTicks > _expirationTime) {
                    _workItemState = WorkItemState.Canceled;
                }

                if (WorkItemState.InProgress == _workItemState) {
                    return _workItemState;
                }

                /*
                if (CanceledSmartThreadPool.IsCanceled || CanceledWorkItemsGroup.IsCanceled) {
                    return WorkItemState.Canceled;
                }
                */

                return _workItemState;
            }
        }

        /// <summary>
        /// Set the result of the work item to return
        /// </summary>
        /// <param name="result">The result of the work item</param>
        /// <param name="exception">The exception that was throw while the workitem executed, null
        /// if there was no exception.</param>
        internal void SetResult(object result, Exception exception)     
        {
            _result = result;
            _exception = exception;
            SignalComplete(false);
        }

        /// <summary>
        /// Sets the work item's state
        /// </summary>
        /// <param name="workItemState">The state to set the work item to</param>
        internal void SetWorkItemState(WorkItemState workItemState)     
        {
            lock (this) {
                if (IsValidStatesTransition(_workItemState, workItemState)) {
                    _workItemState = workItemState;
                }
            }
        }

        #region GetResult

        public object GetResult()
        {
            return GetResult(Timeout.Infinite, true, null);
        }

        public object GetResult(int millisecondsTimeout, bool exitContext)
        {
            return GetResult(millisecondsTimeout, exitContext, null);
        }

        public object GetResult(TimeSpan timeout, bool exitContext)
        {
            return GetResult((int)timeout.TotalMilliseconds, exitContext, null);
        }

        public object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            return GetResult((int)timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
        }

        public object GetResult(out Exception e)
        {
            return GetResult(Timeout.Infinite, true, null, out e);
        }

        public object GetResult(int millisecondsTimeout, bool exitContext, out Exception e)
        {
            return GetResult(millisecondsTimeout, exitContext, null, out e);
        }

        public object GetResult(TimeSpan timeout, bool exitContext, out Exception e)
        {
            return GetResult((int)timeout.TotalMilliseconds, exitContext, null, out e);
        }

        public object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle, out Exception e)
        {
            return GetResult((int)timeout.TotalMilliseconds, exitContext, cancelWaitHandle, out e);
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the method throws and exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        public object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)                                
        {
            Exception e;
            object result = GetResult(millisecondsTimeout, exitContext, cancelWaitHandle, out e);
            if (null != e) {
                throw new WorkItemResultException("The work item caused an excpetion, see the inner exception for details", e);
            }
            return result;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the e argument is filled with the exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        public object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle,
            out Exception e)
        {
            e = null;

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState()) {
                throw new WorkItemCancelException("Work item canceled");
            }

            // Check for completion
            if (IsCompleted) {
                e = _exception;
                return _result;
            }

            // If no cancelWaitHandle is provided
            if (null == cancelWaitHandle) {
                WaitHandle wh = GetWaitHandle();

                bool timeout = !WorkGroupWaitHandle.WaitOne(wh, millisecondsTimeout, exitContext);

                ReleaseWaitHandle();

                if (timeout) {
                    throw new WorkItemTimeoutException("Work item timeout");
                }
            }
            else {
                WaitHandle wh = GetWaitHandle();
                int result = WorkGroupWaitHandle.WaitAny(new WaitHandle[] { wh, cancelWaitHandle });
                ReleaseWaitHandle();

                switch (result) {
                    case 0:
                        // The work item signaled
                        // Note that the signal could be also as a result of canceling the 
                        // work item (not the get result)
                        break;
                    case 1:
                    case WorkGroupWaitHandle.WaitTimeout:
                        throw new WorkItemTimeoutException("Work item timeout");
                    default:
                        Debug.Assert(false);
                        break;

                }
            }

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState()) {
                throw new WorkItemCancelException("Work item canceled");
            }

            Debug.Assert(IsCompleted);

            e = _exception;

            // Return the result
            return _result;
        }


        #endregion

        /// <summary>
        /// A wait handle to wait for completion, cancel, or timeout 
        /// </summary>
        private WaitHandle GetWaitHandle()                              
        {
            lock (this) {
                if (null == _workItemCompleted) {
                    _workItemCompleted = new ManualResetEvent(IsCompleted);
                }
                ++_workItemCompletedRefCount;
            }
            return _workItemCompleted;
        }

        private void ReleaseWaitHandle()                                
        {
            lock (this) {
                if (null != _workItemCompleted) {
                    --_workItemCompletedRefCount;
                    if (0 == _workItemCompletedRefCount) {
                        _workItemCompleted.Close();
                        _workItemCompleted = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has completed or canceled
        /// </summary>
        private bool IsCompleted                                        
        {
            get {
                lock (this) {
                    WorkItemState workItemState = GetWorkItemState();
                    return ((workItemState == WorkItemState.Completed) ||
                            (workItemState == WorkItemState.Canceled));
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has canceled
        /// </summary>
        public bool IsCanceled                                          
        {
            get {
                lock (this) {
                    return (GetWorkItemState() == WorkItemState.Canceled);
                }
            }
        }


        #endregion

        #region IDisposable Members

        /// <summary>
        /// A flag that indicates if the WorkItemsQueue has been disposed.
        /// </summary>
        private bool _isDisposed = false;

        public void Dispose()                   
        {
            if (!_isDisposed) {
            }
            _isDisposed = true;
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
