using System.Collections.Generic;

namespace Play.Studio.Module.Project
{
    public interface IProjectFolder : IProjectItem
    {
        /// <summary>
        /// 目录地址
        /// </summary>
        string Root { get; }

        /// <summary>
        /// 是否包含某文件
        /// </summary>
        bool Contains(string fileName);

        /// <summary>
        /// 目录包含的项
        /// </summary>
        IList<IProjectItem> Items { get; }

        /// <summary>
        /// 根据类型获取项
        /// </summary>
        IEnumerable<IProjectItem> GetItemsOfType(ProjectItemType type);
    }
}
