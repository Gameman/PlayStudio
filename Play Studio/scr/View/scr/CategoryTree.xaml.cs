using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Play.Studio.View
{
    /// <summary>
    /// CategoryTree.xaml 的交互逻辑
    /// </summary>
    public partial class CategoryTree : UserControl
    {
        public CategoryTree()                   
        {
            InitializeComponent();
        }

        /// <summary>
        /// 数据源
        /// </summary>
        public Category     DataSource          
        {
            get { return _treeViewEx1.HeadNode as Category; }
            set { _treeViewEx1.HeadNode = value; }
        }
        
        /// <summary>
        /// 获取选中分类
        /// </summary>
        public Category     SelectionCategroy   { get { return _treeViewEx1.SelectedItem as Category;  } }

        /// <summary>
        /// 获取选中物体
        /// </summary>
        public CategoryItem SelectionItem       { get { return _listView1.SelectedItem as CategoryItem; } }

        /// <summary>
        /// 当选择分类变更后设置新的物体源
        /// </summary>
        private void _treeViewEx1_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            _listView1.ItemsSource = SelectionCategroy.Items;
        }
    }

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
        public Collection<Category>         SubCategorys    { get; set; }

        /// <summary>
        /// 当前物体
        /// </summary>
        public Collection<CategoryItem>     Items           { get; set; }

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
        public ImageSource                  Icon            { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string                       Description     { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public object                       Tag             { get; set; }

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
