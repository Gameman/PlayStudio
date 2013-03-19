using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Play.Studio.Core.Utility;
using System.Collections.Generic;
using System.Xml;
using System;
using Play.Studio.Core.Utility;
using System.Collections.ObjectModel;

namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 项目模板
    /// </summary>
    public sealed class ProjectTemplate : BaseProjectTemplate<ProjectTemplate>
    {
        public override string TemplatesRoot
        {
            get { return "project"; }
        }

        private Queue<string> WriterQueue = new Queue<string>();

        /// <summary>
        /// 模板文件合集
        /// </summary>
        public ReadOnlyCollection<ProjectTemplateFile> Files { get; private set; }

        /// <summary>
        /// 模板引用合集
        /// </summary>
        public ReadOnlyCollection<ProjectTemplateReference> References { get; private set; }

        protected override void OnLoad(XElement templateNode)
        {
            var projectNode                     = templateNode.Element(XName.Get("Project", templateNode.Name.NamespaceName));

            XElement files                      = projectNode.Element(XName.Get("Files", templateNode.Name.NamespaceName));
            XElement references                 = projectNode.Element(XName.Get("ReferenceFolder", templateNode.Name.NamespaceName));

            IEnumerable<XElement> fileMany      = files == null ? null : files.Elements(XName.Get("File", templateNode.Name.NamespaceName));
            IEnumerable<XElement> referenceMany = references == null ? null : references.Elements(XName.Get("Reference", templateNode.Name.NamespaceName));

            // 读取PropertyGroup
            var propertyGroups                  = projectNode.Elements(XName.Get("PropertyGroup", templateNode.Name.NamespaceName));
            foreach (var property in propertyGroups)
            {
                WriterQueue.Enqueue(property.ToString(SaveOptions.OmitDuplicateNamespaces));
            }

            // files
            Files = new ReadOnlyCollection<ProjectTemplateFile>(fileMany == null ? new ProjectTemplateFile[0] : fileMany.Select(X => LoadFile(X)).ToArray());

            // references
            References = new ReadOnlyCollection<ProjectTemplateReference>(referenceMany == null ? new ProjectTemplateReference[0] : referenceMany.Select(X => LoadReference(X)).ToArray());
        }

        private static ProjectTemplateFile LoadFile(XElement xelNode)
        {
            ProjectTemplateFile file = new ProjectTemplateFile();
            file.Include = xelNode.Attribute(XName.Get("Include", xelNode.Name.NamespaceName)).Value;
            file.Content = xelNode.Value;
            return file;
        }

        private static ProjectTemplateReference LoadReference(XElement xelNode)
        {
            ProjectTemplateReference reference = new ProjectTemplateReference();
            reference.ReferneceType = xelNode.Name.LocalName;
            reference.Include = xelNode.Attribute(XName.Get("Include", xelNode.Name.NamespaceName)).Value;
            return reference;
        }

        /// <summary>
        /// 将项目模板替换指定参数后写入文件
        /// </summary>
        protected override void OnWriteTo(string fileName, Dictionary<string, string> replaceParams = null)
        {
            /*  项目模板预加载说明
             *  @{StandardNamespace} 命名空间
             *  
             * 
             */


            string root = ReplaceParams["ProjectPath"];

            // 替换文字

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding =\"utf-8\"?>");
                    sw.WriteLine("<Project>");
                    // 写入等待队列
                    while (WriterQueue.Count > 0)
                        sw.WriteLine(StringHelper.Replace(WriterQueue.Dequeue(), ReplaceParams));

                    // 创建引用
                    foreach (var reference in References)
                    {
                        sw.WriteLine(string.Format("<{0} Include=\"{1}\" />", reference.ReferneceType, reference.Include));
                    }

                    // 创建文件
                    foreach (var file in Files)
                    {
                        sw.WriteLine(string.Format("<{0} Include=\"{1}\" />", Path.GetExtension(file.Include) == ".ps" ? "Compile" : "Content", file.Include));

                        string filePath = Path.Combine(root, file.Include);
                        if (!File.Exists(filePath))
                        {
                            File.WriteAllText(filePath, StringHelper.Replace(file.Content, ReplaceParams), System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                    }
                    sw.WriteLine("</Project>");
                }
            }
        }


    }
}
