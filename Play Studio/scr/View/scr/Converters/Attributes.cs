using System;

namespace Play.Studio.View.Converters
{
    /// <summary>
    /// 模板转换器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateConverterAttribute : Attribute 
    {
        public Type TemplateType { get; private set; }

        public TemplateConverterAttribute(Type type) 
        {
            TemplateType = type;
        }
    }
}
