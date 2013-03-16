using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 项目引用
    /// </summary>
    class ProjectReference : ProjectItem, IProjectReference
    {
        internal ProjectItemType m_itemType;

        public override ProjectItemType ItemType
        {
            get { return m_itemType; }
        }

        public override bool Exists
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}

