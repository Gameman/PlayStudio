using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Play.Studio.Module.Views;
using Clipboard = System.Windows.Forms.Clipboard;

namespace Play.Studio.View
{
    /// <summary>
    /// Play.Studio.View.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewEx : TreeView                                                              
    {
        public TreeViewEx()
        {
            InitializeComponent();

            //this._input = new InputState() { Attach = this };
            this.MouseDoubleClick += TreeViewItem_MouseDoubleClick;
            this.KeyDown += TreeViewEx_KeyDown;
            this.Drop += new DragEventHandler(TreeViewEx_Drop);
            this.DragEnter += new DragEventHandler(TreeViewEx_DragEnter);

            this.AllowDrop = true;
        }

        #region 成员

        bool _isCut;

        TreeViewItem _hitItem;
        TreeViewItem _copyItem;
        TreeViewExNode _headNode;
        TreeViewExNode _hitNode;
        TreeViewExNode _copyNode;
        TreeViewExNode _dropNode;

        #endregion

        #region 输入循环器

        static Key[] comm_copy = new Key[2] { Key.LeftCtrl, Key.C };
        static Key[] comm_paste = new Key[2] { Key.LeftCtrl, Key.V };
        static Key[] comm_cut = new Key[2] { Key.LeftCtrl, Key.X };

        private void TreeViewEx_KeyDown(object obj, KeyboardEventArgs args)
        {
            /*
            if (_input.IsKeyPressed(Key.Enter)) {
                if (SelectedItem != null) {
                    _hitNode = SelectedItem as Node;
                    if (TreeViewExItemMouseDoubleClick != null) TreeViewExItemMouseDoubleClick(null, null);
                    if (_hitNode.DoubleClick() || !_hitNode.IsFolder) {
                        args.Handled = true;
                    }
                }
            }
            // 组合键
            else if (_input.IsCombinKeyPressed(comm_copy))
            {
                PCSolve(false);
            }

            else if (_input.IsCombinKeyPressed(comm_cut))
            {
                PCSolve(true);
            }

            else if (_input.IsCombinKeyPressed(comm_paste))
            {
                TreeViewItem_Paste();
            }
             */
        }

        #endregion

        #region 节点

        public TreeViewExNode HeadNode
        {
            get { return _headNode; }
            set
            {
                if (_headNode != value)
                {
                    _headNode = value;
                    ItemsSource = _headNode.Nodes;
                }
            }
        }

        /// <summary>
        /// 获取命中节点
        /// </summary>
        public TreeViewExNode HitNode
        {
            get { return _hitNode; }
            private set
            {
                if (_hitNode != value)
                {
                    _hitNode = value;
                    if (SelectChanged != null) SelectChanged(null, null);
                }
            }
        }

        /// <summary>
        /// 获取命中节点项
        /// </summary>
        public TreeViewItem HitNodeItem { get { return _hitItem; } }

        /// <summary>
        /// 取得当前焦点的节点
        /// </summary>
        public TreeViewExNode GetFocusNode { get { return this.SelectedItem as TreeViewExNode; } }

        /// <summary>
        /// 节点展开事件
        /// </summary>
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            _hitNode.IsExpand = true;
        }

        /// <summary>
        /// 节点关闭事件
        /// </summary>
        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            _hitNode.IsExpand = false;
        }

        #endregion

        #region 事件

        public event EventHandler<TreeViewExNodeEventArgs> SelectChanged;
        public event EventHandler<TreeViewExNodeEventArgs> BeforeCopyEvent;
        public event EventHandler<TreeViewExNodeEventArgs> AfterCopyEvent;
        public event EventHandler<TreeViewExNodeEventArgs> BeforePasteEvent;
        public event EventHandler<TreeViewExNodeEventArgs> AfterPasteEvent;
        public event EventHandler<TreeViewExNodeEventArgs> BeforeCutEvent;
        public event EventHandler<TreeViewExNodeEventArgs> AfterCutEvent;
        public event EventHandler<TreeViewExNodeOperationEventArgs> BeforeNodeOperationEvent;
        public event EventHandler<TreeViewExNodeOperationEventArgs> AfterNodeOperationEvent;
        public event EventHandler<TreeViewExNodeEventArgs> BeforeDropEvent;
        public event EventHandler<TreeViewExNodeEventArgs> AfterDropEvent;

        #endregion

        #region 粘贴&剪切&复制

        public void PCSolve(bool isCut)
        {
            _isCut = isCut;

            var target = this.SelectedItem as TreeViewExNode;
            if (isCut)
                TreeViewItem_Cut(target);
            else
                TreeViewItem_Copy(target);
        }

        private void TreeViewItem_Copy(TreeViewExNode targetNode)
        {
            TreeViewExNodeEventArgs e = new TreeViewExNodeEventArgs(null, targetNode);
            if (BeforeCopyEvent != null) BeforeCopyEvent(null, e);
            if (!e.Handle)
            {
                if (_copyItem != null) _copyItem.Opacity = 1;
                _copyItem = _hitItem;
                _copyNode = _hitItem.DataContext as TreeViewExNode;
                if (AfterCopyEvent != null) AfterCopyEvent(null, e);
            }
        }

        private void TreeViewItem_Paste()
        {
            TreeViewExNodeEventArgs e = new TreeViewExNodeEventArgs(_copyNode, _hitNode);
            if (BeforePasteEvent != null) BeforePasteEvent(_isCut, e);
            if (!e.Handle)
            {
                var obj = Clipboard.GetDataObject();

                NodeOperation((_isCut) ? TreeViewExNodeOperationType.Cut : TreeViewExNodeOperationType.Copy, _copyNode, _hitNode);
                if (AfterPasteEvent != null) AfterPasteEvent(null, e);
            }
        }

        private void TreeViewItem_Cut(TreeViewExNode node)
        {
            if (BeforeCutEvent != null) BeforeCutEvent(null, null);
            if (_copyItem != null) _copyItem.Opacity = 1;
            _copyItem = _hitItem;
            _copyNode = _hitItem.DataContext as TreeViewExNode;
            _copyItem.Opacity = 0.5;
            if (AfterCutEvent != null) AfterCutEvent(null, null);
        }

        #endregion

        #region 拖动

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                _hitItem = treeViewItem;
                HitNode = _hitItem.DataContext as TreeViewExNode;
                if (_hitNode.AllowDrop)
                    _dropNode = _hitNode;
            }
        }

        private void TreeViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_dropNode == null) return;

            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                var t_n = treeViewItem.DataContext as TreeViewExNode;
                if (_dropNode != t_n && t_n != null)
                {
                    _hitItem = treeViewItem;
                    HitNode = _hitItem.DataContext as TreeViewExNode;
                    NodeOperation(TreeViewExNodeOperationType.Cut, _dropNode, _hitNode);
                    _hitItem.Focus();
                }
            }

            _dropNode = null;
        }

        private void TreeViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
        }

        #endregion

        #region 右键菜单

        public event EventHandler TreeViewExMouseRightButtonDown;
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                _hitItem = treeViewItem;
                HitNode = _hitItem.DataContext as TreeViewExNode;
                _hitItem.Focus();
                if (_hitItem.ContextMenu == null)
                {
                    if (TreeViewExMouseRightButtonDown != null) TreeViewExMouseRightButtonDown(HitNode, null);
                    ContextMenu menu = (_hitItem.DataContext as TreeViewExNode).ContextMenu;
                    if (menu != null)
                    {
                        _hitItem.ContextMenu = menu;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion

        #region 双击事件

        public event EventHandler TreeViewExItemMouseDoubleClick;
        public void TreeViewItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                var hitNode = treeViewItem.DataContext as TreeViewExNode;
                if (TreeViewExItemMouseDoubleClick != null) TreeViewExItemMouseDoubleClick(null, null);
                if (hitNode.DoubleClick() || !hitNode.IsFolder)
                {
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region 其他

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        void NodeOperation(TreeViewExNodeOperationType operation, TreeViewExNode dstNode, TreeViewExNode targetNode)
        {
            TreeViewExNodeOperationEventArgs e = new TreeViewExNodeOperationEventArgs(dstNode, targetNode, operation);
            if (BeforeNodeOperationEvent != null) BeforeNodeOperationEvent(null, e);
            if (!e.Handle)
            {
                if (!targetNode.IsFolder)
                {
                    throw new Exception("无法拖动节点到非节点文件夹内.");
                }

                if (dstNode != targetNode && dstNode != null)
                {
                    switch (operation)
                    {
                        case TreeViewExNodeOperationType.Copy:
                            if (dstNode.AllowCopy)
                                targetNode.Nodes.Add(dstNode.Clone());
                            break;
                        case TreeViewExNodeOperationType.Cut:
                            if (dstNode.AllowCut)
                            {
                                TreeViewExNode.Shift(dstNode, targetNode);
                                if (_copyItem != null) _copyItem.Opacity = 1;
                                dstNode = null;
                            }
                            break;
                        case TreeViewExNodeOperationType.Delete:
                            if (dstNode.AllowDelete)
                                dstNode.ParentNode.Nodes.Remove(dstNode);
                            break;
                    }
                }
                if (AfterNodeOperationEvent != null) AfterNodeOperationEvent(null, e);
            }
        }

        #endregion

        #region 外部文件

        private void TreeViewEx_Drop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (fileNames.Length != 0)
            {
                var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
                if (treeViewItem != null)
                {
                    var target = treeViewItem.DataContext as TreeViewExNode;
                    TreeViewExNodeEventArgs ev = new TreeViewExNodeEventArgs(null, target);
                    if (BeforeDropEvent != null) BeforeDropEvent(fileNames, ev);
                    if (AfterDropEvent != null) AfterDropEvent(null, ev);
                }
            }
        }

        private void TreeViewEx_DragEnter(object sender, DragEventArgs e)
        {
        }

        #endregion
    }

    class TreeViewExLineConverter : IValueConverter                                                         
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}