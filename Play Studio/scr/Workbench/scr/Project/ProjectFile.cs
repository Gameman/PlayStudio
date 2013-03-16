using System.IO;
using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    public class ProjectFile : ProjectItem, IProjectFile
    {
        internal        ProjectItemType     m_itemType;

        public override ProjectItemType     ItemType    { get { return m_itemType; } }
        public override bool                Exists      { get { return File.Exists(FileName); } }
    }
}
