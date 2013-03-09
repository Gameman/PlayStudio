using System;
using System.Diagnostics;
using System.Threading;

namespace Play.Studio.Core.Utility
{
    public class CallbackOnDispose : IDisposable
    {
        Action<object> callback;

        public CallbackOnDispose(Action<object> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            this.callback = callback;
        }

        public void Dispose()
        {
            Action<object> action = Interlocked.Exchange(ref callback, null);
            if (action != null)
            {
                action(null);
#if DEBUG
                GC.SuppressFinalize(this);
#endif
            }
        }

#if DEBUG
        ~CallbackOnDispose()
        {
            Debug.Fail("CallbackOnDispose was finalized without being disposed.");
        }
#endif
    }
}
