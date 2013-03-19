using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Play.Studio.Module.Views
{
    /// <summary>
    /// 分类
    /// </summary>
    public class Category : TreeViewExNode
    {
        public override bool AllowCopy
        {
            get
            {
                return false;
            }
            protected set
            {
            }
        }
        public override bool AllowCut
        {
            get
            {
                return false;
            }
            protected set
            {
            }
        }
        public override bool AllowDelete
        {
            get
            {
                return false;
            }
            protected set
            {
            }
        }
        public override bool AllowDrop
        {
            get
            {
                return false;
            }
            protected set
            {
            }
        }

        /// <summary>
        /// 下级分类
        /// </summary>
        public Collection<Category> SubCategorys { get; set; }

        /// <summary>
        /// 当前物体
        /// </summary>
        public Collection<CategoryItem> Items { get; set; }

        public override TreeViewExNodeCollection Nodes
        {
            get
            {
                return new TreeViewExNodeCollection(SubCategorys);
            }
            internal set
            {
                base.Nodes = value;
            }
        }

        public Category()
        {
            Items = new Collection<CategoryItem>();
            SubCategorys = new Collection<Category>();
        }
    }

    /// <summary>
    /// 分类项
    /// </summary>
    public class CategoryItem : ListBoxItem
    {
        /// <summary>
        /// 图标
        /// </summary>
        public ImageSource Icon { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

    }

    /// <summary>
    /// 分类树异常
    /// </summary>
    [Serializable]
    public class CategoryTreeException : Exception
    {
        public CategoryTreeException() { }
        public CategoryTreeException(string message) : base(message) { }
        public CategoryTreeException(string message, System.Exception inner) : base(message, inner) { }
        protected CategoryTreeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
