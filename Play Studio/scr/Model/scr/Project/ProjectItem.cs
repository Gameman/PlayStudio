using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 项目项
    /// </summary>
    public abstract class ProjectItem : IProjectItem
    {
        public abstract ProjectItemType    ItemType { get; }
        public abstract bool        Exists { get; }

        public bool Builded
        {
            get { return false; }
        }

        public IProject Project
        {
            get;
            set;
        }

        public string Include
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string IconPath
        {
            get;
            set;
        }

        public string Name
        {
            get { return Path.GetFileName(FileName); }
        }

        public string FileName
        {
            get;
            set;
        }

        public string Extension
        {
            get { return Path.GetExtension(FileName); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IProjectItem CloneFor(IProject targetProject)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
