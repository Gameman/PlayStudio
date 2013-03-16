using System;
using System.Collections.Generic;
using System.IO;
using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 目录项
    /// </summary>
    public class ProjectFolder : ProjectItem, IProjectFolder
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

        public virtual IList<IProjectItem> Items                                        
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IEnumerable<IProjectItem> GetItemsOfType(ProjectItemType type)   
        {
            throw new NotImplementedException();
        }

        public override ProjectItemType ItemType                                        
        {
            get { return ProjectItemType.Folder; }
        }

        public override bool Exists                                                     
        {
            get { return Directory.Exists(FileName); }
        }
    }
}
