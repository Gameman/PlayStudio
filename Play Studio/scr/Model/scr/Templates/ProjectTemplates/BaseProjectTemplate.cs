using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Play.Studio.Module.Templates
{
    public abstract class BaseProjectTemplate<T> : Template<T> where T : Template<T>, new()
    {
        public override string TemplateExtension                                            
        {
            get { return "wpt"; }
        }

        /// <summary>
        /// 模板文件合集
        /// </summary>
        public ReadOnlyCollection<ProjectTemplateFile>         Files       { get; private set; }

        /// <summary>
        /// 模板引用合集
        /// </summary>
        public ReadOnlyCollection<ProjectTemplateReference>    Refernces   { get; private set; }

        protected override void                 OnLoad(XElement templateNode)               
        {
            XElement files                      = templateNode.Element(XName.Get("Files", templateNode.Name.NamespaceName));
            XElement references                 = templateNode.Element(XName.Get("Referneces", templateNode.Name.NamespaceName));

            IEnumerable<XElement> fileMany      = files == null ? null : files.Elements(XName.Get("File", templateNode.Name.NamespaceName));
            IEnumerable<XElement> referenceMany = references == null ? null : references.Elements(XName.Get("Reference", templateNode.Name.NamespaceName));

            // files
            Files = new ReadOnlyCollection<ProjectTemplateFile>(fileMany == null ? new ProjectTemplateFile[0] : fileMany.Select(X => LoadFile(X)).ToArray());

            // references
            Refernces = new ReadOnlyCollection<ProjectTemplateReference>(referenceMany == null ? new ProjectTemplateReference[0] : referenceMany.Select(X => LoadReference(X)).ToArray());
        }

        private static ProjectTemplateFile      LoadFile(XElement xelNode)                  
        {
            ProjectTemplateFile file   = new ProjectTemplateFile();
            file.FullName       = xelNode.Attribute(XName.Get("filename", xelNode.Name.NamespaceName)).Value;
            file.Language       = xelNode.Attribute(XName.Get("language", xelNode.Name.NamespaceName)).Value;
            file.Content        = xelNode.Value;
            return file;
        }

        private static ProjectTemplateReference LoadReference(XElement xelNode)             
        {
            ProjectTemplateReference reference = new ProjectTemplateReference();
            reference.ReferneceType     = xelNode.Attribute(XName.Get("type", xelNode.Name.NamespaceName)).Value;
            reference.ReferencePath     = xelNode.Attribute(XName.Get("path", xelNode.Name.NamespaceName)).Value;
            return reference;
        }
    }
}
