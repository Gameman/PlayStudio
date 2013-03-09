using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Play.Studio.Core.Command.Addins
{
    /// <summary>
    /// 扩展
    /// </summary>
    public sealed class Addin : ReadOnlyCollection<AddinContent>
    {
        public string FileName { get; private set; }

        public AddinContent this[string str] 
        {
            get { return base.Items.FirstOrDefault(X => X.Name.Equals(str)); }
        }
        
        internal Addin(string fileName, IList<AddinContent> contents) 
            : base(contents)
        {
            FileName = fileName;
        }

        
    }
}
