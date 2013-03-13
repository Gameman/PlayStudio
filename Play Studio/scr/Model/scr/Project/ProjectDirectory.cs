using System;
using System.Collections.Generic;

namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 目录项
    /// </summary>
    public class ProjectDirectory : ProjectItem, IProjectDirectory
    {
        public string Root
        {
            get { return FileName; }
        }

        public bool Contains(string fileName)
        {
            if (Exists)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public IEnumerable<IProjectItem> Items
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IProjectItem> GetItemsOfType(ProjectItemType type)
        {
            throw new NotImplementedException();
        }

        public override ProjectItemType ItemType
        {
            get { return ProjectItemType.Directory; }
        }

        public override bool Exists
        {
            get { return System.IO.Directory.Exists(FileName); }
        }
    }
}
