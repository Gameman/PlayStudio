using System;
using System.Windows.Controls;
using Play.Studio.Module.Views;

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

        public event EventHandler<SelectionChangedEventArgs> OnSelectedItemChanged;

        private void _listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnSelectedItemChanged != null)
                OnSelectedItemChanged(this, e);
        }
    }
}
