using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.IO;

namespace Play.Studio.Module.Language
{
    /// <summary>
    /// 多国语言
    /// </summary>
    public static class Languages
    {
        /// <summary>
        /// 初始化属性
        /// </summary>
        static Languages() 
        {
            LastCultrueInfo = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// 上一次使用的区域语言
        /// </summary>
        public static CultureInfo LastCultrueInfo { get; private set; }

        /// <summary>
        /// 默认语言包
        /// </summary>
        public static LanguagePackage DefaultLanguagePackage { get; private set; }

        /// <summary>
        /// 语言包集合
        /// </summary>
        static Dictionary<CultureInfo, List<LanguagePackage>> Langs = new Dictionary<CultureInfo, List<LanguagePackage>>();

        /// <summary>
        /// 转换语言
        /// </summary>
        public static string Tran(this string message) 
        {
            return Tran(message, Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        /// 使用特定区域信息转换语言
        /// </summary>
        public static string Tran(this string message, CultureInfo culture) 
        {
            if (Langs.ContainsKey(culture))
            {
                foreach (var package in Langs[culture]) 
                {
                    if (package.ContainsKey(message))
                    {
                        return package[message];
                    }
                }

                goto DefaultTran;
            }
            else 
            {
                goto DefaultTran;
            }

DefaultTran:
            if (DefaultLanguagePackage != null && DefaultLanguagePackage.ContainsKey(message))
            {
                return DefaultLanguagePackage[message];
            }
            else
            {
                return message;
            }
        }


        /// <summary>
        /// 应用语言包
        /// </summary>
        public static LanguagePackage Append(string fileName) 
        {
            using(var stream = File.OpenRead(fileName))
            {
                return Append(stream);
            }
        }

        /// <summary>
        /// 应用语言包
        /// </summary>
        public static LanguagePackage Append(Stream stream) 
        {
            LanguageResource resource;
            if (stream is FileStream)
            {
                resource = LanguageResource.Create(Play.Studio.Module.Resource.Uri.From((stream as FileStream).Name)) as LanguageResource;
            }
            else 
            {
                resource = new LanguageResource();
            }
            resource.OnRead(stream);
            return resource.GetResult() as LanguagePackage;
        }

        /// <summary>
        /// 释放语言包
        /// </summary>
        public static bool Disposable(LanguagePackage package) 
        {
            if (Langs.ContainsKey(package.CultureInfo))
            {
                return Langs[package.CultureInfo].Remove(package);
            }
            else 
            {
                return false;
            }
        }
    }
}
