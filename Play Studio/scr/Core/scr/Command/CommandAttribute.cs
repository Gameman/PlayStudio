using System;

namespace Play.Studio.Core.Command
{
    /// <summary>
    /// 指令标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Header { get; set; }
    }
}
