using System.Xml.Linq;

namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 项目项模板
    /// </summary>
    public sealed class ProjectItemTemplate : BaseProjectTemplate<ProjectItemTemplate>
    {
        public override string TemplateExtension
        {
            get { return "wpt"; }
        }

        public override string TemplatesRoot
        {
            get { return "items"; }
        }

        protected override void OnLoad(XElement xelDoc)
        {
        }
    }
}
