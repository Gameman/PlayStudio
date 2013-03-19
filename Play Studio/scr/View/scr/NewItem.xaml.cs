using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Play.Studio.Core.Services;
using Play.Studio.Module.Templates;
using Play.Studio.Module.Views;

namespace Play.Studio.View
{
    /// <summary>
    /// NewItem.xaml 的交互逻辑
    /// </summary>
    public partial class NewItem : UserControl, IView
    {
        public string Result { get; private set; }

        public CategoryTree CategoryTree { get { return _category; } }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeComponent();
            this.FocusVisualStyle = null;

            OnLoaded();
            //categoryTreeView1.DataSource = ProjectItemTemplate.Templates;
            createButton.IsEnabled = false;

            this.nameBox.TextChanged += NameTextChanged;
        }

        private void OnLoaded()
        {
            // 创建分类头
            Category category = new Category();
            // 读取到所有项目数据模板
            foreach (var template in ProjectItemTemplate.Templates)
            {
                var categorys = template.Config["category"].Split('.');
                var currentCategory = category;

                // 查询到最后一个有效分类
                foreach (var c in categorys)
                {
                    var temp = currentCategory.SubCategorys.FirstOrDefault(X => X.Content.Equals(c));
                    if (temp == null)
                    {
                        currentCategory.SubCategorys.Add(temp = new Category() { Content = c });
                    }

                    currentCategory = temp;
                }

                currentCategory.Items.Add(new CategoryItem()
                {
                    Icon = ImageCenter.Get(template.Config["icon"]),
                    Description = template.Description,
                    Content = template.Config["name"],
                    Tag = template,
                });
            }

            _category.DataSource = category;
        }

        private void NameTextChanged(object sender, EventArgs e)
        {
            string error = CheckItemName(nameBox.Text);
            if (error != null)
            {
                nameBox.ToolTip = error;
                createButton.IsEnabled = false;
                return;
            }

            if (_category.SelectionItem != null)
                createButton.IsEnabled = true;
        }

        private static string CheckItemName(string name)
        {
            if (name.Length == 0 || !char.IsLetter(name[0]) && name[0] != '_')
            {
                return "Illegal item name: project name must start with letter.";
            }
            if (!FileService.IsValidDirectoryEntryName(name))
            {
                return "Illegal item name. Only letters, digits, space, '.' or '_' are allowed.";
            }
            if (name.EndsWith("."))
            {
                return "Illegal item name: project name may not end with '.'.";
            }
            return null;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Result = nameBox.Text;
            // 检查输出目录是否存在

            /*
            var item = (CommandServer.Execute("GetProjectExplorer") as ProjectExplorer).SelectedItem;
            if (!System.IO.Directory.Exists(item.FullName))
            {
                MessageBox.Show("Output directory does not exist.");
                return;
            }
             */

            // 解包
            //categoryTreeView1.SelectTemplate.Extract(item.FullName);
            // 重命名服务

            if (OnClosed != null)
                OnClosed(null, e);
        }

        private void CaneclButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnClosed != null)
                OnClosed(null, e);
        }

        public ViewShowType ShowType
        {
            get { return ViewShowType.Float; }
        }

        public ViewShowStrategy ShowStrategy
        {
            get { return ~ViewShowStrategy.Most; }
        }

        public string Title
        {
            get { return "New Item"; }
        }

        public event EventHandler OnClosed;


        public Size FloatSize
        {
            get { return new Size(380, 250); }
        }

        private void _category_OnSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }


        public void Refersh(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
