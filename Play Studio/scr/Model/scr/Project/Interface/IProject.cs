using System;
using System.Collections.Generic;

namespace Play.Studio.Model.Project
{
    public interface IProject : IProjectDirectory
    {
        /// <summary>
        /// 获得项目项
        /// </summary>
        IEnumerable<IProjectItem>          ProjectItems            { get; }

        /// <summary>
        /// 获得项目程序集名
        /// </summary>
        string                      AssemblyName            { get; }

        /// <summary>
        /// 获得输出程序集完整路径
        /// </summary>
        string                      OutputAssemblyFullPath  { get; }

        /// <summary>
        /// 获得项目用户设置完整路径
        /// </summary>
        string                      UserConfigFullPath      { get; }

        /// <summary>
        /// 获得项目目录命名空间
        /// </summary>
        string                      RootNamespace           { get; }

        /// <summary>
        /// 获得项目全部类型
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type>           GetRuntimeTypes();

        /// <summary>
        /// 获取默认引用项
        /// </summary>
        IEnumerable<IProjectReference> GetDefaultAssemblies();

        /// <summary>
        /// 生成项目
        /// </summary>
        /// <returns></returns>
        void                        Build();

        /// <summary>
        /// 加载项目
        /// </summary>
        bool                        Load(string fileName);

        /// <summary>
        /// 卸载项目
        /// </summary>
        bool                        Unload();

        /// <summary>
        /// 保存项目
        /// </summary>
        bool                        Save(string fileName);
    }
}
