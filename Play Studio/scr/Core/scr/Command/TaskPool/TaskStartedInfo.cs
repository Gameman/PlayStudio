using System;
using System.Threading;
using Play.Studio.Core.Command.TaskPool.Threading.Internal;

namespace Play.Studio.Core.Command.TaskPool
{
    public class TaskStartInfo
    {
        internal static TaskStartInfo Default = new TaskStartInfo();

        internal Task task;

        public static TaskStartInfo FirstTask 
        {
            get {
                return new TaskStartInfo() { 
                    Category = "First Task",
                    Description = "First Task",
                    Name = "First Task",
                    Priority = ThreadPriority.Highest,
                    RunMode = WorkItemRunMode.Async
                };
            }
        }

        /// <summary>
        /// 获取或设置任务优先级
        /// </summary>
        public ThreadPriority Priority { get; set; }

        /// <summary>
        /// 获取或设置任务执行模式
        /// </summary>
        public WorkItemRunMode RunMode { get; set; }

        /// <summary>
        /// 触发器
        /// </summary>
        public Func<bool> Trigger { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务类别
        /// </summary>
        public string Category { get; set; }
    }
}
