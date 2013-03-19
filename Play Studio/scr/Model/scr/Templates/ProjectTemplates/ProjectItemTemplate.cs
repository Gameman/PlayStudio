using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;


namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 项目项模板
    /// </summary>
    public sealed class ProjectItemTemplate : BaseProjectTemplate<ProjectItemTemplate>
    {
        public override string TemplatesRoot
        {
            get { return "item"; }
        }

        protected override void OnLoad(XElement xelDoc)
        {
        }

        protected override void OnWriteTo(string fileName, Dictionary<string, string> replaceParams = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
