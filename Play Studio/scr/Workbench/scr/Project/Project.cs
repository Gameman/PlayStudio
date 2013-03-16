using System;
using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 项目
    /// </summary>
    public sealed class Project : ProjectFolder, IProject
    {


        public string AssemblyName
        {
            get { throw new NotImplementedException(); }
        }

        public string OutputAssemblyFullPath
        {
            get { throw new NotImplementedException(); }
        }

        public string UserConfigFullPath
        {
            get { throw new NotImplementedException(); }
        }

        public string RootNamespace
        {
            get { throw new NotImplementedException(); }
        }
    }
}
