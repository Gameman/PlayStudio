using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Play.Studio.Model.Project
{
    class Project : ProjectDirectory, IProject
    {
        public IEnumerable<IProjectItem> ProjectItems
        {
            get { throw new NotImplementedException(); }
        }

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

        public IEnumerable<Type> GetRuntimeTypes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IProjectReference> GetDefaultAssemblies()
        {
            throw new NotImplementedException();
        }

        public void Build()
        {
            throw new NotImplementedException();
        }

        public bool Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Unload()
        {
            throw new NotImplementedException();
        }

        public bool Save(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
