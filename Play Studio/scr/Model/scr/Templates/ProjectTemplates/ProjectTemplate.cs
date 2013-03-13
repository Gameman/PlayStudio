using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 项目模板
    /// </summary>
    public sealed class ProjectTemplate : BaseProjectTemplate<ProjectTemplate>
    {
        public override string TemplatesRoot
        {
            get { return "items"; }
        }

        protected override void OnLoad(XElement xelDoc)
        {
        }
    }
}
