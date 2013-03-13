using System;

namespace Play.Studio.Core.Command.TaskPool
{
    public class FuncTask<TResult> : Task
    {
        public FuncTask(Func<TResult> func) : base(func) { }

        public TResult Invoke() 
        {
            return this.Invoke(TaskStartInfo.Default);
        }

        public TResult Invoke(TaskStartInfo info) 
        {
            return (TResult)base.Invoke(info);
        }
    }

    public class FuncTask<TResult, T> : Task
    {
        public FuncTask(Func<TResult, T> func) : base(func) { }

        public TResult Invoke(T arg)
        {
            return this.Invoke(TaskStartInfo.Default, arg);
        }

        public TResult Invoke(TaskStartInfo info, T arg)
        {
            return (TResult)base.Invoke(info, arg);
        }
    }

    public class FuncTask<T1, T2, TResult> : Task
    {
        public FuncTask(Func<T1, T2, TResult> func) : base(func) { }

        public TResult Invoke(T1 arg1, T2 arg2)
        {
            return this.Invoke(TaskStartInfo.Default, arg1, arg2);
        }

        public TResult Invoke(TaskStartInfo info, T1 arg1, T2 arg2)
        {
            return (TResult)base.Invoke(info, arg1, arg2);
        }
    }

    public class FuncTask<T1, T2, T3, TResult> : Task
    {
        public FuncTask(Func<T1, T2, T3, TResult> func) : base(func) { }

        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            return this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3);
        }

        public TResult Invoke(TaskStartInfo info, T1 arg1, T2 arg2, T3 arg3)
        {
            return (TResult)base.Invoke(info, arg1, arg2, arg3);
        }
    }

    public class FuncTask<T1, T2, T3, T4, TResult> : Task
    {
        public FuncTask(Func<T1, T2, T3, T4, TResult> func) : base(func) { }

        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3, arg4);
        }

        public TResult Invoke(TaskStartInfo info, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return (TResult)base.Invoke(info, arg1, arg2, arg3, arg4);
        }
    }

    public class FuncTask<T1, T2, T3, T4, T5, TResult> : Task
    {
        public FuncTask(Func<T1, T2, T3, T4, T5, TResult> func) : base(func) { }

        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3, arg4, arg5);
        }

        public TResult Invoke(TaskStartInfo info, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return (TResult)base.Invoke(info, arg1, arg2, arg3, arg4, arg5);
        }
    }

}
