using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Play.Studio.Core.Utility;

namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// UI模板基类
    /// </summary>
    public abstract class BaseUserInterfaceTemplate<T> : Template<T> where T : Template<T>, new()
    {
        /// <summary>
        /// 模板扩展名
        /// </summary>
        public override string TemplateExtension
        {
            get { return "uit"; }
        }

        /// <summary>
        /// 模板支持类型
        /// </summary>
        public ExpandControlType[] Supports { get; set; }

        protected override void OnLoad(XElement templateNode)
        {
            var supports = Config["support"].Split(';');
            List<ExpandControlType> supportList = new List<ExpandControlType>(3);
            for (int i = 0; i < supports.Length; i++) 
            {
                ExpandControlType result;
                if (Enum.TryParse<ExpandControlType>(supports[i], out result))
                {
                    supportList.Add(result);
                }
            }
            Supports = supportList.ToArray();
        }

        protected override TextReader PreLoad(Stream stream)
        {
            // 替换文字
            var sr = new StreamReader(stream);
            var xmlTxt = sr.ReadToEnd();
            // @\{.*?\} 
            Parallel.ForEach<Capture>(StringHelper.Parse(xmlTxt, "@+{[^}]+}").GroupBy(X => X.Value).Where(g => g.Count() == 1)
                    .Select(g => g.ElementAt(0)), (o, s) =>
                    {
                        xmlTxt = xmlTxt.Replace(o.Value, Resource.Resource.Read(o.Value.TrimStart('@', '{').TrimEnd('}')).ToString());
                    });

            return new StringReader(xmlTxt);
        }

        public static T[] GetTemplate(ExpandControlType type)         
        {
            return Templates.Where(X => (X as BaseUserInterfaceTemplate<T>).Supports.Count(T => T == type) > 0).ToArray();
        }
    }
}
