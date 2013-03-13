using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            this.Style = this.Resources["Play.Studio.ViewViewStyle"] as Style;
            this.ItemTemplate = this.Resources["Play.Studio.ViewDataTemplate"] as HierarchicalDataTemplate;
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
            set {
                if (_headNode != value) {
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
            if (!e.Handle) {
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
            if (!e.Handle) {
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
            if (treeViewItem != null) {
                var t_n = treeViewItem.DataContext as TreeViewExNode;
                if (_dropNode != t_n && t_n != null) {
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
                    ContextMenu menu = (_hitItem.DataContext as TreeViewExNode)._contextMenu;
                    if (menu != null)
                        _hitItem.ContextMenu = menu;
                    else
                        e.Handled = true;
                }
            }
        }

        #endregion

        #region 双击事件

        public event EventHandler TreeViewExItemMouseDoubleClick;
        public void TreeViewItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null) {
                var hitNode = treeViewItem.DataContext as TreeViewExNode;
                if (TreeViewExItemMouseDoubleClick != null) TreeViewExItemMouseDoubleClick(null, null);
                if (hitNode.DoubleClick() || !hitNode.IsFolder) {
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
            if (fileNames.Length != 0) {
                var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
                if (treeViewItem != null) {
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

    public class TreeViewExNodeEventArgs : EventArgs, IDisposable                                           
    {
        #region IDisposable Members

        public bool IsDisposed;

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed || disposing)
            {
                try
                {
                    if (DisposeEvent != null) DisposeEvent(null, null);
                }
                catch
                {
                }
                IsDisposed = true;
            }
        }

        public event EventHandler DisposeEvent;

        #endregion

        #region 成员

        bool _handle;
        TreeViewExNode _sourceNode;
        TreeViewExNode _targetNode;

        #endregion

        #region 访问器

        /// <summary>
        /// 是否终止事件路由
        /// </summary>
        public bool Handle { get { return _handle; } set { _handle = value; } }

        /// <summary>
        /// 源节点
        /// </summary>
        public TreeViewExNode SourceNode { get { return _sourceNode; } }

        /// <summary>
        /// 目标节点
        /// </summary>
        public TreeViewExNode TargetNode { get { return _targetNode; } }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object Tag { get; set; }

        #endregion

        public TreeViewExNodeEventArgs(TreeViewExNode sourceNode, TreeViewExNode targetNode)
        {
            _sourceNode = sourceNode;
            _targetNode = targetNode;
        }
    }

    public class TreeViewExNodeOperationEventArgs : TreeViewExNodeEventArgs                                 
    {
        #region 成员

        TreeViewExNodeOperationType _operation;

        #endregion

        #region 访问器

        public TreeViewExNodeOperationType Operation { get { return _operation; } }

        #endregion

        #region 构造函数

        public TreeViewExNodeOperationEventArgs(TreeViewExNode sourceNode, TreeViewExNode targetNode, TreeViewExNodeOperationType operation) 
            : base(sourceNode, targetNode)
        {
            _operation = operation;
        }

        #endregion
    }

    public enum TreeViewExNodeOperationType                                                                 
    {
        Delete,
        Cut,
        Copy,
    }

    public class TreeViewExNode : INotifyPropertyChanged, IComparable<TreeViewExNode>, IDisposable          
    {
        #region IDisposable Members

        public bool IsDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed || disposing)
            {
                try
                {
                    if (DisposeEvent != null) DisposeEvent(null, null);
                }
                catch
                {
                }
                IsDisposed = true;
            }
        }

        public event EventHandler DisposeEvent;

        #endregion

        #region 事件

        public event EventHandler DoubleClickEvent;
        public event EventHandler<TreeViewExNodeEventArgs> ParentNodeChanged;
        public event EventHandler<TreeViewExNodeEventArgs> BeforeShift;

        #endregion

        #region 成员

        string _content;
        BitmapSource _expandIcon;
        BitmapSource _shutIcon;
        bool _isExpand;
        bool _isFolder;
        TreeViewExNode _preNode;
        TreeViewExNodeCollection _nodes;
        internal ContextMenu _contextMenu;

        bool _allowDrop;
        bool _allowCopy;
        bool _allowCut;
        bool _allowDelete;

        #endregion

        #region 访问器

        /// <summary>
        /// 节点标记
        /// </summary>
        public string Token { get; internal set; }

        /// <summary>
        /// 获取或设置右键菜单
        /// </summary>
        public ContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { _contextMenu = value; }
        }

        /// <summary>
        /// 获取或设置内容
        /// </summary>
        public string Content
        {
            get { return _content; }
            set 
            { 
                _content = value;
                NotifyPropertyChanged("Content");
            }
        }

        /// <summary>
        /// 获得展开时图标
        /// </summary>
        public BitmapSource ExpandIcon { 
            get { return _expandIcon; } 
            set 
            { 
                _expandIcon = value;
                NotifyPropertyChanged("ExpandIcon");
            }
        }

        /// <summary>
        /// 获得关闭时图标
        /// </summary>
        public BitmapSource ShutIcon 
        { 
            get { return _shutIcon; } 
            set 
            { 
                _shutIcon = value;
                NotifyPropertyChanged("ShutIcon");
            } 
        }

        /// <summary>
        /// 获得图标控件
        /// </summary>
        public BitmapSource Icon { get { return (_isExpand) ? _expandIcon : _shutIcon; } }

        /// <summary>
        /// 获得节点是否为展开状态
        /// </summary>
        public bool IsExpand
        {
            get { return _isExpand; }
            set 
            { 
                _isExpand = value;
                NotifyPropertyChanged("Icon");
            }
        }

        /// <summary>
        /// 获取或设置是否为文件夹类型节点
        /// </summary>
        public bool IsFolder 
        {
            get { return _isFolder; }
            protected set { _isFolder = value; }
        }

        /// <summary>
        /// 父级节点
        /// </summary>
        public TreeViewExNode ParentNode 
        { 
            get { return _preNode; }
            internal set 
            { 
                _preNode = value;
                if (ParentNodeChanged != null) 
                    ParentNodeChanged(this, null);
            }
        }

        /// <summary>
        /// 获取子节点列表
        /// </summary>
        public TreeViewExNodeCollection Nodes 
        { 
            get { return _nodes; }
            internal set 
            { 
                _nodes = value;
                _nodes.LocateNode = this;
            }
        }

        /// <summary>
        /// 能否拖动
        /// </summary>
        public bool AllowDrop
        {
            get { return _allowDrop; }
            protected set { _allowDrop = value; }
        }

        public bool AllowCopy 
        {
            get { return _allowCopy; }
            protected set { _allowCopy = value; }
        }

        public bool AllowCut 
        {
            get { return _allowCut; }
            protected set { _allowCut = value; }
        }

        public bool AllowDelete 
        {
            get { return _allowDelete; }
            protected set { _allowDelete = value; }
        }

        #endregion

        #region 方法

        internal bool DoubleClick() 
        {
            if (DoubleClickEvent != null)
            {
                DoubleClickEvent(this, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        public TreeViewExNode Clone() 
        {
            TreeViewExNode node = new TreeViewExNode();
            node._content = _content;
            node._expandIcon = _expandIcon;
            node._shutIcon = _shutIcon;
            node._allowDrop = _allowDrop;
            node._allowDelete = _allowDelete;
            node._allowCut = _allowCut;
            node._allowCopy = _allowCopy;
            node._isFolder = _isFolder;
            node._contextMenu = _contextMenu;
            for (int i = 0; i < _nodes.Count; i++)
                node._nodes.Add(_nodes[i].Clone());
            return node;
        }

        public int CompareTo(TreeViewExNode node) 
        {
            return _content.CompareTo(node._content);
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 转移该节点至目标节点
        /// </summary>
        /// <param name="targetNode"></param>
        internal static void Shift(TreeViewExNode dstNode, TreeViewExNode targetNode)
        {
            TreeViewExNodeEventArgs e = new TreeViewExNodeEventArgs(dstNode, targetNode);
            if (dstNode.BeforeShift != null) dstNode.BeforeShift(null, e);
            if (!e.Handle) {
                dstNode._preNode._nodes.Remove(dstNode);
                targetNode._nodes.Add(dstNode);
                //NotifyPropertyChanged("_preNode");
            }
            e.Dispose();
        }
        
        #endregion

        public TreeViewExNode() 
        {
            Nodes = new TreeViewExNodeCollection();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TreeViewExNodeCollection : ObservableCollection<TreeViewExNode>                            
    {
        public TreeViewExNode LocateNode;
        public event EventHandler BeforeAdded;
        public event EventHandler BeforeRemoved;
        public event EventHandler AfterAdded;
        public event EventHandler AfterRemoved;

        IDictionary<string, TreeViewExNode> _buffer;

        public TreeViewExNodeCollection() 
            : base()
        {
            _buffer = new Dictionary<string, TreeViewExNode>();
        }

        public new void Add(TreeViewExNode item) 
        {
            if (BeforeAdded != null) BeforeAdded(item, null);
            base.Add(item);
            item.ParentNode = LocateNode;
            //Sort();
            if (AfterAdded != null) AfterAdded(item, null);
        }

        public new void Remove(TreeViewExNode item) 
        {
            if (BeforeRemoved != null) BeforeRemoved(item, null);
            base.Remove(item);
            if (AfterRemoved != null) AfterRemoved(item, null);
        }

        public bool ContainsKey(string key) 
        {
            if (ComputeNodeByContent(key) != null)
                return true;
            else
                return false;
        }

        public TreeViewExNode this[string content] 
        {
            get {
                return ComputeNodeByContent(content);
            }

            set {
                this[content] = value;
            }
        }

        public void Sort()
        {
            Sort(Comparer<TreeViewExNode>.Default);
        }

        public void Cover(TreeViewExNodeCollection nc) 
        {
            for (int i = 0; i < this.Count; i++)
                nc.Add(this[i]);
        }

        public void Sort(IComparer<TreeViewExNode> comparer)
        {
            int i, j;
            TreeViewExNode index;
            for (i = 1; i < this.Count; i++)
            {
                index = Items[i];
                j = i; 
                while ((j > 0) && (comparer.Compare(Items[j - 1], index) == 1))
                {
                    Items[j] = Items[j - 1];
                    j = j - 1;
                }
                Items[j] = index;
            }
        }

        private TreeViewExNode ComputeNodeByContent(string content)
        {
            // 检查缓冲区
            if (_buffer.ContainsKey(content)) {
                return _buffer[content];
            }

            // 缓冲区不存在标记时
            for (int i = 0; i < this.Count; i++) {
                if (this[i].Content == content) {
                    _buffer.Add(content, this[i]);
                    return this[i];
                }
            }

            return null;
        }

    }
}