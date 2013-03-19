using Play.Studio.Module.Project;
using System.Windows.Controls;

namespace Play.Studio.Module.Views
{
    public class SolutionExplorerNode : TreeViewExNode
    {
        public IProjectItem ProjectItem
        {
            get;
            private set;
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
