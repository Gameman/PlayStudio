using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Play.Studio.Core;
using Play.Studio.Core.Services;

namespace Play.Studio.Model.Templates
{
    public interface ITemplate 
    {
        /// <summary>
        /// 作家
        /// </summary>
        string           Author      { get; }

        /// <summary>
        /// 版本
        /// </summary>
        string           Version     { get; }

        /// <summary>
        /// 模板描述信息
        /// </summary>
        string           Description { get; }

        /// <summary>
        /// 模板控制文件
        /// </summary>
        TemplateConfig   Config      { get; }
    }

    /// <summary>
    /// 模板
    /// </summary>
    public abstract class Template<T> : Resource<T, Template<T>>, ITemplate where T : Template<T>, new()
    {
        private static ReadOnlyCollection<string>       s_templateNames;
        private static ReadOnlyCollection<T>            s_templates;

        /// <summary>
        /// 模板相对路径(会搜索子目录)
        /// </summary>
        public abstract string                          TemplatesRoot       { get; }

        /// <summary>
        /// 模板扩展名
        /// </summary>
        public abstract string                          TemplateExtension   { get; }

        /// <summary>
        /// 模板合集
        /// </summary>
        public static ReadOnlyCollection<T>             Templates           
        { 
            get 
            { 
#if DEBUG
                UpdateTemplates();
#else
                if(s_templates == null)
                    UpdateTemplates();
#endif
                return s_templates;
            } 
        }

        /// <summary>
        /// 作家
        /// </summary>
        public string                                   Author              { get; private set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string                                   Version             { get; private set; }

        /// <summary>
        /// 模板描述信息
        /// </summary>
        public string                                   Description         { get; private set; }

        /// <summary>
        /// 模板控制文件
        /// </summary>
        public TemplateConfig                           Config              { get; private set; }

        /// <summary>
        /// 更新模板
        /// </summary>
        internal static void                UpdateTemplates()                           
        {
            var templateType = typeof(T);
            var searchTemplate =  TypeService.CreateInstance(templateType) as Template<T>;
            // 搜索到所有目录下扩展名相匹配的模板
            var templatePaths = FileService.SearchDirectory(Path.Combine(FileService.TemplatePath, searchTemplate.TemplatesRoot), 
                                                            "*." + searchTemplate.TemplateExtension, 
                                                            true);

            // 生成模板集合
            s_templateNames = new ReadOnlyCollection<string>(templatePaths);
            s_templates = new ReadOnlyCollection<T>(
                templatePaths.Select(X => Resource<T>.Read(templateType, new Uri(X, UriType.Absolute))).Where(X => X != null).OrderBy(X => X.Config.Contains("index") ? int.Parse(X.Config["index"]) : int.MaxValue).ToArray());
        }

        protected static T                  Load(Stream stream)                       
        {
            XElement templateNode = XElement.Load(stream); //.Load(fullName);
            if (templateNode == null)
            {
                //MessageService.ShowException(new TemplateLoadException(fullName));
                return null;
            }

            XElement cfgNode                    = templateNode.Element(XName.Get("Config", templateNode.Name.NamespaceName));
            XElement desNode                    = templateNode.Element(XName.Get("Description", templateNode.Name.NamespaceName));

            T template = new T();
            //template.FullName                   = fullName;
            template.Author                     = templateNode.Attribute(XName.Get("author", templateNode.Name.NamespaceName)).Value;
            template.Version                    = templateNode.Attribute(XName.Get("version", templateNode.Name.NamespaceName)).Value;
            
            // config
            template.Config                     = LoadConfig(cfgNode);

            // description
            template.Description                = desNode.Value;

            // child load
            template.OnLoad(templateNode);

            return template;
        }

        protected override T OnRead(Stream stream) 
        {
            return Load(stream);
        }

        protected abstract void             OnLoad(XElement templateNode);

        private static TemplateConfig       LoadConfig(XElement xelNode)                
        {
            int configCount = xelNode.Attributes().Count();
            string[] keys = new string[configCount];
            string[] values = new string[configCount];

            int index = 0;
            foreach (var attribute in xelNode.Attributes()) 
            {
                keys[index] = attribute.Name.LocalName;
                values[index] = attribute.Value;
                index++;
            }

            return new TemplateConfig(keys, values);
        }

        public static T                     GetTemplate(string name)                    
        {
            var index = s_templateNames.IndexOf(name);
            if (index > 0 && index < s_templates.Count)
                return s_templates[s_templateNames.IndexOf(name)];
            else
                return null;
        }
    }
}
