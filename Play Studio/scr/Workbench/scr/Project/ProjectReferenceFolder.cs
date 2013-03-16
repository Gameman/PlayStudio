using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 项目引用文件夹
    /// </summary>
    class ProjectReferenceFolder : ProjectFolder 
    {
        public override ProjectItemType ItemType
        {
            get { return ProjectItemType.ReferneceFolder; }
        }

    }
}
