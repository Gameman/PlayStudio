using System.Collections.Generic;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool.Threading.Command
{
    static class WorkGroupWaitHandle
    {
        public const int WaitTimeout = Timeout.Infinite;

        static SynchronizedDictionary<WorkItem, ManualResetEvent> _waitHandles =
                                new SynchronizedDictionary<WorkItem, ManualResetEvent>();

        public static void WaitOne(this WorkItem workItem, bool initialState) 
        {
            lock (workItem) {
                if (!_waitHandles.Contains(workItem)) {
                    _waitHandles[workItem] = GetOne(true);
                }

                _waitHandles[workItem].WaitOne();
            }
        }

        public static void SetOne(this WorkItem workItem) 
        {
            lock (workItem) {
                if (!_waitHandles.Contains(workItem)) {
                    return;
                }

                _waitHandles[workItem].Set();
            }
        }


        public static ManualResetEvent GetOne(bool initialState) 
        {
            return new ManualResetEvent(initialState);
        }

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return WaitHandle.WaitAll(waitHandles, millisecondsTimeout);
        }

        public static int WaitAny(WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAny(waitHandles);
        }

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
        }

        public static bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout, bool exitContext)
        {
            return waitHandle.WaitOne(millisecondsTimeout);
        }
    }
}
