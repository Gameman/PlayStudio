using System;

namespace Play.Studio.Core.Command.Addins
{
    /// <summary>
    /// 插件基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AddinAttribute : Attribute
    {
         
    }

    /// <summary>
    /// 插件内容基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class AddinContentAttribute : Attribute 
    {
        
    }
}
