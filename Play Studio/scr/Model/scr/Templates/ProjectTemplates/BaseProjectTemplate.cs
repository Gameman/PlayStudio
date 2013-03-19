using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Play.Studio.Module.Templates
{
    public abstract class BaseProjectTemplate<T> : Template<T> where T : Template<T>, new()
    {
        /// <summary>
        /// 替换参数
        /// </summary>
        protected Dictionary<string, string> ReplaceParams
        {
            get;
            private set;
        }

        public override string TemplateExtension                                            
        {
            get { return "wpt"; }
        }

        /// <summary>
        /// 写入到文件内(如果文件存在则覆盖)
        /// </summary>
        public void WriteTo(string fileName, Dictionary<string, string> replaceParams = null)
        {
            ReplaceParams = replaceParams == null ? new Dictionary<string, string>() : replaceParams;

            var root = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            OnWriteTo(fileName, replaceParams);
        }

        /// <summary>
        /// 当写入文件时
        /// </summary>
        protected abstract void OnWriteTo(string fileName, Dictionary<string, string> replaceParams = null);
    }
}
