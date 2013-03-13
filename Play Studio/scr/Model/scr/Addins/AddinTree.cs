using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Play.Studio.Module.Addins
{
    /// <summary>
    /// 插件树
    /// </summary>
    public static class AddinTree
    {
        public static List<Addin> s_addinCahce = new List<Addin>();

        /// <summary>
        /// 通过地址加载插件
        /// </summary>
        public static Addin Load(string fileName)                                   
        {
            List<AddinContent> _contents = new List<AddinContent>();

            // scan 所有ExtensionAttribute
            try
            {
                Type eType = typeof(AddinContentAttribute);

                foreach (var type in Assembly.LoadFrom(fileName).GetExportedTypes())
                {
                    var es = type.GetCustomAttributes(eType, false);
                    if (es.Length > 0)
                        _contents.Add(new AddinContent(type));
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                throw exp;
#else
#endif
            }

            var addin =  new Addin(fileName, _contents);
            s_addinCahce.Add(addin);
            return addin;
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public static bool UnLoad(string fileName)                                  
        {
            return UnLoad(s_addinCahce.FirstOrDefault(X => X.FileName.Equals(fileName)));
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public static bool UnLoad(Addin addin)                                      
        {
            if (addin == null)
                return false;
            else
                return s_addinCahce.Remove(addin);
        }

        public static object[] GetContents()                                        
        {
            return GetContents(string.Empty);
        }

        public static object[] GetContents(string category)                         
        {
            return GetContents(category, Type.EmptyTypes);
        }

        public static object[] GetContents(string category, Type[] types)           
        {
            var addinContents = GetAddinContents(category, types);

            var result = new object[addinContents.Length];

            for(int i =0; i < result.Length; i++)
            {
                result[i] = addinContents[i].GetInstance();
            }

            return result;
        }

        public static AddinContent[] GetAddinContents()                             
        {
            return GetAddinContents(string.Empty);
        }

        public static AddinContent[] GetAddinContents(string category)              
        {
            return GetAddinContents(category, Type.EmptyTypes);
        }

        public static AddinContent[] GetAddinContents(string category, Type[] types)
        {
            List<AddinContent> result = new List<AddinContent>();

            foreach(var addin in s_addinCahce)
            {
                foreach(var content in addin) 
                {
                    if(content.Category.Equals(category) && types.FirstOrDefault(X => X.IsAssignableFrom(content.ContentType)) != null)
                    {
                        result.Add(content);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
