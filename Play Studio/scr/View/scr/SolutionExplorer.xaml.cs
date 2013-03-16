using System.Windows;
using System.Windows.Controls;
using Play.Studio.Module.Project;

namespace Play.Studio.View
{
    /// <summary>
    /// SolutionExplorer.xaml 的交互逻辑
    /// </summary>
    public partial class SolutionExplorer : UserControl         
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
    }

    public class SolutionExplorerNode : TreeViewExNode
    {
        public IProjectItem ProjectItem                         
        {
            get;
            private set;
        }

        public override TreeViewExNodeCollection Nodes          
        {
            get
            {
                return base.Nodes;
            }
            internal set
            {
                base.Nodes = value;
            }
        }

        public SolutionExplorerNode()                           
        {
            ProjectItem = this as IProjectItem;
        }

        public SolutionExplorerNode(IProjectItem projectItem)   
        {
            ProjectItem = projectItem;
        }
    }



}
