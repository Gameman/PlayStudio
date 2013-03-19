using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Play.Studio.Core;
using Play.Studio.Module.Views;
using System.Collections.Generic;
using Play.Studio.Module.Project;
using Play.Studio.Module.Resource;
using Play.Studio.Module.Language;

namespace Play.Studio.View
{
    /// <summary>
    /// SolutionExplorer.xaml 的交互逻辑
    /// </summary>
    public partial class SolutionExplorer : UserControl, IView  
    {
        public SolutionExplorer()
        {
            InitializeComponent();

            entity.BeforeNodeOperationEvent += entity_BeforeNodeOperationEvent;
            entity.BeforeDropEvent += entity_BeforeDropEvent;
            entity.TreeViewExItemMouseDoubleClick += (s, e) =>
            {
                //CommandServer.Execute(DefaultCommand.Develop_OnProjectResourceOpen, entity.HitNode);

            };
            entity.TreeViewExMouseRightButtonDown += (s, e) =>
            {
                PopContextMenu(s as SolutionExplorerNode);
            };    

            HeadNode = new SolutionExplorerNode();

            Property.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == "Solution") 
                {
                    HeadNode.Nodes.Clear();
                    var orSolution = Property.Get<SolutionExplorerNode>("Solution", default(SolutionExplorerNode));
                    if (orSolution != null)
                        HeadNode.Nodes.Add(orSolution);
                }
            };

            var initSolution = Property.Get<SolutionExplorerNode>("Solution", default(SolutionExplorerNode));
            if (initSolution != null)
                HeadNode.Nodes.Add(initSolution);
        }

        public SolutionExplorerNode HeadNode
        {
            get { return GetValue(HeadNodeDependency) as SolutionExplorerNode; }
            set { SetValue(HeadNodeDependency, value); }
        }

        public readonly static DependencyProperty HeadNodeDependency =
            DependencyProperty.Register(
                "HeadNode",
                typeof(SolutionExplorerNode),
                typeof(SolutionExplorer),
                new PropertyMetadata(new PropertyChangedCallback(headNodePropertyChangedCallback))
                );

        static void headNodePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)  
        {
            var pe = sender as SolutionExplorer;
            if (pe != null)
                pe.entity.HeadNode = e.NewValue as SolutionExplorerNode;

        }

        #region 方法

        private void entity_BeforeDropEvent(object sender, TreeViewExNodeEventArgs args)                            
        {
            //CommandServer.Execute("OnProjectExplorerDrop", sender, args.TargetNode);
        }

        private void entity_BeforeNodeOperationEvent(object sender, TreeViewExNodeOperationEventArgs args)          
        {
            bool overWrite = false;

            // 目标节点不是一个文件夹
            if (!args.TargetNode.IsFolder)
            {
                args.Handle = true;
                return;
            }

            // 是否已存在相同节点
            if (args.SourceNode.ParentNode == args.TargetNode)
            {
                args.Handle = true;
                return;
            }

            // 检测内容是否冲突
            if (args.TargetNode.Nodes.ContainsKey(args.SourceNode.Content))
            {
                // 如果冲突
                args.Handle = true;
                return;
            }

            // 层级错误
            if (args.SourceNode.Nodes.Contains(args.TargetNode))
            {
                args.Handle = true;
                return;
            }
        }

        private void TreeView_ContentConflictEvent(object sender, TreeViewExNodeEventArgs args)                     
        {
        }

        #endregion

        public ViewShowType ShowType
        {
            get { return ViewShowType.Fixed; }
        }

        public ViewShowStrategy ShowStrategy
        {
            get { return ViewShowStrategy.Right; }
        }

        public string Title
        {
            get { return "Solution Explorer"; }
        }

        public Size FloatSize
        {
            get { return new Size(300, 650); }
        }

        public event System.EventHandler OnClosed;


        #region 右键菜单

        public void PopContextMenu(SolutionExplorerNode hitContextMenuNode)
        {
            if (hitContextMenuNode.ContextMenu == null)
            {
                hitContextMenuNode.ContextMenu = new System.Windows.Controls.ContextMenu();

                var k = hitContextMenuNode.ProjectItem;

                if (ProjectItemType.Folders.Contains(k.ItemType))
                {
                    if (k.ItemType.Equals(ProjectItemType.ReferneceFolder))
                    {
                        hitContextMenuNode.ContextMenu.Items.Add(GetAddReferenceMenu());
                    }
                    else
                    {
                        AddAddOperation(hitContextMenuNode.ContextMenu);
                        if (k.ItemType.Equals(ProjectItemType.Folder))
                        {
                            hitContextMenuNode.ContextMenu.Items.Add(new Separator());
                            AddFileOperation(hitContextMenuNode.ContextMenu, true);
                        }
                    }
                }
                else if (ProjectItemType.References.Contains(k.ItemType))
                {
                    hitContextMenuNode.ContextMenu.Items.Add(GetDeleteMenu());
                }
                else if (ProjectItemType.Files.Contains(k.ItemType))
                {
                    AddFileOperation(hitContextMenuNode.ContextMenu, false);
                }

                if (hitContextMenuNode.ContextMenu.HasItems)
                    hitContextMenuNode.ContextMenu.Items.Add(new Separator());

                hitContextMenuNode.ContextMenu.Items.Add(GetPropertyMenu());
            }
        }

        #region additem


        private MenuItem GetDeleteMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Delete}".Tran();
            return item;
        }

        private MenuItem GetCopyMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Copy}".Tran();
            return item;
        }

        private MenuItem GetCutMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Cut}".Tran();
            return item;
        }

        private MenuItem GetPasteMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Paste}".Tran();
            return item;
        }

        private MenuItem GetRenameMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Rename}".Tran();
            return item;
        }

        private MenuItem GetPropertyMenu() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{Property}".Tran();
            return item;
        }

        private MenuItem GetNewItem() 
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{NewItem}".Tran();
            return item;
        }

        private MenuItem GetNewFolder()
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{NewFolder}".Tran();
            return item;
        }

        private MenuItem GetExistMenu()
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{ExistItem}".Tran();
            return item;
        }

        private MenuItem GetAddReferenceMenu()
        {
            MenuItem item = new MenuItem();
            item.Click += (s, e) => { };
            item.Header = "@{AddReference}".Tran();
            return item;
        }

        private void AddFileOperation(ContextMenu c, bool addPaste) 
        {
            c.Items.Add(GetCutMenu());
            c.Items.Add(GetCopyMenu());
            if (addPaste) 
            {
                c.Items.Add(GetPasteMenu());
            }
            c.Items.Add(GetDeleteMenu());
            c.Items.Add(GetRenameMenu());
        }

        private void AddAddOperation(ContextMenu c) 
        {
            c.Items.Add(GetNewItem());
            c.Items.Add(GetExistMenu());
            c.Items.Add(GetNewFolder());
            //c.Items.Add(new Separator());
        }

        #endregion

        #endregion
    }
}
