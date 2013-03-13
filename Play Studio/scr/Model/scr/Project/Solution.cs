using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 解决方案
    /// </summary>
    public class Solution : ProjectDirectory, ISolution
    {
        public bool AddProject(IProject project)
        {
            throw new NotImplementedException();
        }

        public bool RemoveProject(IProject project)
        {
            throw new NotImplementedException();
        }

        public bool Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Save(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Close(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
