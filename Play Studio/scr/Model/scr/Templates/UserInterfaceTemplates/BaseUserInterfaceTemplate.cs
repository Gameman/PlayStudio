using System;
using System.Linq;
using System.Xml.Linq;

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
            Supports = new ExpandControlType[supports.Length];
            for (int i = 0; i < supports.Length; i++)
                Supports[i] = (ExpandControlType)Enum.Parse(typeof(ExpandControlType), supports[i]);
        }

        public static T[] GetTemplate(ExpandControlType type)         
        {
            return Templates.Where(X => (X as BaseUserInterfaceTemplate<T>).Supports.Count(T => T == type) > 0).ToArray();
        }
    }
}
