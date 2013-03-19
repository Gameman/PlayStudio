using System;
using System.Collections.Generic;
using System.Globalization;

namespace Play.Studio.Module.Language
{
    /// <summary>
    /// 语言包
    /// </summary>
    public class LanguagePackage : Dictionary<string, string>, IDisposable
    {
        /// <summary>
        /// 获取特定区域性信息
        /// </summary>
        public CultureInfo  CultureInfo     { get; private set; }

        /// <summary>
        /// 是否是有效的语言包
        /// </summary>
        public bool         IsValid         { get{ return CultureInfo != null; } } 

        /// <summary>
        /// 新建语言包
        /// </summary>
        public LanguagePackage(CultureInfo cultureInfo) 
        {
            CultureInfo = cultureInfo;
        }

        /// <summary>
        /// 释放该语言包
        /// </summary>
        public void Dispose()
        {
            Languages.Disposable(this);
        }
    }
}
