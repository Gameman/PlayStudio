using System;

namespace Play.Studio.Core.Command.TaskPool
{
    public class ActionTask : Task
    {
        public ActionTask(Action action) 
            : base(action)
        {
        }

        public virtual void Invoke()
        {
            this.Invoke(TaskStartInfo.Default);
        }

        public virtual void Invoke(TaskStartInfo startInfo) 
        {
            base.Invoke(startInfo, null);
        }
    }

    public class ActionTask<T> : Task
    {
        public ActionTask(Action<T> action)
            : base(action)
        {
        }

        public virtual void Invoke(T arg)
        {
            this.Invoke(TaskStartInfo.Default, arg);
        }

        public virtual void Invoke(TaskStartInfo startInfo, T arg)
        {
            base.Invoke(startInfo, arg);
        }
    }

    public class ActionTask<T1, T2> : Task
    {
        public ActionTask(Action<T1, T2> action)
            : base(action)
        {
        }

        public virtual void Invoke(T1 arg1, T2 arg2)
        {
            this.Invoke(TaskStartInfo.Default, arg1, arg2);
        }

        public virtual void Invoke(TaskStartInfo startInfo, T1 arg1, T2 arg2)
        {
            base.Invoke(startInfo, arg1, arg2);
        }
    }

    public class ActionTask<T1, T2, T3> : Task
    {
        public ActionTask(Action<T1, T2, T3> action)
            : base(action)
        {
        }

        public virtual void Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3);
        }

        public virtual void Invoke(TaskStartInfo startInfo, T1 arg1, T2 arg2, T3 arg3)
        {
            base.Invoke(startInfo, arg1, arg2, arg3);
        }
    }

    public class ActionTask<T1, T2, T3, T4> : Task
    {
        public ActionTask(Action<T1, T2, T3, T4> action)
            : base(action)
        {
        }

        public virtual void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3, arg4);
        }

        public virtual void Invoke(TaskStartInfo startInfo, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            base.Invoke(startInfo, arg1, arg2, arg3, arg4);
        }
    }

    public class ActionTask<T1, T2, T3, T4, T5> : Task
    {
        public ActionTask(Action<T1, T2, T3, T4, T5> action)
            : base(action)
        {
        }

        public virtual void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            this.Invoke(TaskStartInfo.Default, arg1, arg2, arg3, arg4, arg5);
        }

        public virtual void Invoke(TaskStartInfo startInfo, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            base.Invoke(startInfo, arg1, arg2, arg3, arg4, arg5);
        }
    }

}
