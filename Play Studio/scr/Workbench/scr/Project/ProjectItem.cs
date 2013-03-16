using System;
using System.IO;
using Play.Studio.Module.Project;
using Play.Studio.View;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 项目项
    /// </summary>
    public abstract class ProjectItem : SolutionExplorerNode, IProjectItem
    {
        public abstract ProjectItemType     ItemType    { get; }
        public abstract bool                Exists      { get; }
        public bool                         Builded     { get { return false; } }
        public IProject                     Project     { get; set; }
        public string                       Include     { get; set; }
        public string                       IconPath    { get; set; }
        public string                       Name        { get { return Path.GetFileName(FileName); } }
        public string                       FileName    { get; set; }
        public string                       Extension   { get { return Path.GetExtension(FileName); } }
        public object                       SyncRoot    { get { throw new NotImplementedException(); } }

        public IProjectItem CloneFor(IProject targetProject)
        {
            throw new NotImplementedException();
        }

        public object Clone() 
        {
            throw new NotImplementedException();
        }
    }
}
