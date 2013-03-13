using System;
using System.Collections.Generic;
using System.Linq;
using Play.Studio.Core.Services;
using Play.Studio.Module.Templates;

namespace Play.Studio.View.Converters
{
    /// <summary>
    /// 模板数据转换器
    /// </summary>
    public abstract class TemplateConverter<TResult>
    {
        static Dictionary<Type, TemplateConverter<TResult>> s_converters = new Dictionary<Type, TemplateConverter<TResult>>();

        static TemplateConverter() 
        {
            // 更新所有模板数据转换器
            UpdateConverters();
        }

        /// <summary>
        /// 更新所有模板数据转换器
        /// </summary>
        private static void UpdateConverters() 
        {
            // 查找所有模板转换器
            foreach (var type in
                TypeService.FindProxyClasses<TemplateConverter<TResult>>(typeof(TemplateConverter<>).Assembly).Where(X => !X.IsAbstract))
            {
                var attribute = TypeService.GetAttribute<TemplateConverterAttribute>(type);
                if (attribute != null)
                {
                    s_converters[attribute.TemplateType] = TypeService.CreateInstance(type) as TemplateConverter<TResult>;
                }
            }
        }

        /// <summary>
        /// 转换模板到数据
        /// </summary>
        /// <param name="args">输入的参数</param>
        /// <returns>具体转换后的数据</returns>
        public static TResult Convert<TInput>(TInput arg) where TInput : ITemplate
        {
            var type = arg.GetType();
            if (s_converters.ContainsKey(type))
                return s_converters[type].OnConvert<TInput>(arg);
            else
                throw new TemplateConverterNotSupportException("Not implemented.");
        }

        /// <summary>
        /// 当真正转换
        /// </summary>
        protected abstract TResult OnConvert<TInput>(TInput arg) where TInput : ITemplate;
    }
}
