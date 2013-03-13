using System.Linq;
using System.Windows.Controls;
using Play.Studio.Module.Templates;
using Play.Studio.View.Converters;

namespace Play.Studio.View
{
    /// <summary>
    /// TopMenu.xaml 的交互逻辑
    /// </summary>
    public partial class TopMenu : UserControl
    {
        public TopMenu()
        {
            InitializeComponent();
        }

        public MenuItem[] DataSource
        {
            get 
            {
                return BaseUserInterfaceTemplate<MenuTemplate>.GetTemplate(ExpandControlType.topmenu).Select(X => TemplateConverter<MenuItem>.Convert(X)).ToArray();
            }
        }
    }
}
