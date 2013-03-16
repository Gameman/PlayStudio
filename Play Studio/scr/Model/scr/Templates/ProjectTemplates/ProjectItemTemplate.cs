using System.Xml.Linq;

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
    }
}
