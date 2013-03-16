using System.Linq;
using System.Xml.Linq;

namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 菜单模板
    /// </summary>
    public sealed class MenuTemplate : BaseUserInterfaceTemplate<MenuTemplate>
    {
        public override string TemplatesRoot
        {
            get { return @"ui\menu"; }
        }

        public MenuTemplateUnit[] Units
        {
            get;
            private set;
        }

        protected override void OnLoad(XElement templateNode)
        {
            base.OnLoad(templateNode);

            var unitsNode = templateNode.Element(XName.Get("Units", templateNode.Name.NamespaceName));
            if (unitsNode != null)
            {
                Units = unitsNode.Elements(XName.Get("Unit", templateNode.Name.NamespaceName)).Select(X => QueryUints(X)).ToArray();
            }
            else
            {
                Units = new MenuTemplateUnit[0];
            }
        }

        private static MenuTemplateUnit QueryUints(XElement node) 
        {
            var headerNode      = node.Attribute(XName.Get("header", node.Name.NamespaceName));
            var iconNode        = node.Attribute(XName.Get("icon", node.Name.NamespaceName));
            var commandNode     = node.Attribute(XName.Get("command", node.Name.NamespaceName));
            var commandParamNode= node.Attribute(XName.Get("commandParameter", node.Name.NamespaceName));
            var shortKeyNode    = node.Attribute(XName.Get("shortkey", node.Name.NamespaceName));

            return new MenuTemplateUnit() 
            {
                Header          = headerNode    == null ? string.Empty : headerNode.Value,
                Icon            = iconNode      == null ? string.Empty : iconNode.Value,
                Command         = commandNode   == null ? string.Empty : commandNode.Value,
                CommandParameter=commandParamNode == null? string.Empty : commandParamNode.Value,
                ShortKey        = shortKeyNode  == null ? string.Empty : shortKeyNode.Value,
                Subs            = node.Elements(XName.Get("Unit", node.Name.NamespaceName)).Select(X => QueryUints(X)).ToArray()
            };
        }
    }

    /// <summary>
    /// 菜单模板单元(TODO:需要重新抽象一套界面元素)
    /// </summary>
    public class MenuTemplateUnit 
    {
        public string               Header          { get; set; }
        public string               Icon            { get; set; }
        public string               ShortKey        { get; set; }
        public string               Command         { get; set; }
        public string               CommandParameter{ get; set; }

        public MenuTemplateUnit[]   Subs            { get; set; }

        public override string ToString()
        {
            return Header;
        }
    }
}
