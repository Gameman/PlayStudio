using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Play.Studio.Module.Views
{
    public class TreeViewExNodeEventArgs : EventArgs, IDisposable                                               
    {
        #region IDisposable Members

        public bool IsDisposed;

        public void Dispose()
        {
            Dispose(true);
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
        public virtual string Content
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
        public BitmapSource ExpandIcon
        {
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
        public virtual bool IsFolder
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
        public virtual TreeViewExNodeCollection Nodes
        {
            get { return _nodes; }
            internal set
            {
                _nodes = value;
                _nodes.Host = this;
            }
        }

        public virtual bool AllowDrop   { get; protected set; }
        public virtual bool AllowCopy   { get; protected set; }
        public virtual bool AllowCut    { get; protected set; }
        public virtual bool AllowDelete { get; protected set; }

        #endregion

        #region 方法

        public bool DoubleClick()
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
            node.AllowDrop = AllowDrop;
            node.AllowDelete = AllowDelete;
            node.AllowCut = AllowCut;
            node.AllowCopy = AllowCopy;
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
        public static void Shift(TreeViewExNode dstNode, TreeViewExNode targetNode)
        {
            TreeViewExNodeEventArgs e = new TreeViewExNodeEventArgs(dstNode, targetNode);
            if (dstNode.BeforeShift != null) dstNode.BeforeShift(null, e);
            if (!e.Handle)
            {
                dstNode.ParentNode.Nodes.Remove(dstNode);
                targetNode.Nodes.Add(dstNode);
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TreeViewExNodeCollection : ObservableCollection<TreeViewExNode>, IInList<TreeViewExNode>, INotifyCollectionChanged
    {
        public TreeViewExNode Host { get; internal set; }

        public TreeViewExNodeCollection()
            : base()
        {
        }

        public TreeViewExNodeCollection(IEnumerable<TreeViewExNode> elements)
            : base(elements)
        {
        }

        public new void Add(TreeViewExNode item)            
        {
            if (BeforeAdded != null) BeforeAdded(item, null);
            base.Add(item);
            item.ParentNode = Host;
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
            return this.FirstOrDefault(X => X.Content.Equals(key)) != null;
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

        public bool IsReadOnly
        {
            get { return false; }
        }

        public event EventHandler BeforeAdded;
        public event EventHandler BeforeRemoved;
        public event EventHandler AfterAdded;
        public event EventHandler AfterRemoved;
    }
}
