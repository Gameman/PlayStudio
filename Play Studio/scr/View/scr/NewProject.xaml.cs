using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Play.Studio.Core.Command;
using Play.Studio.Core.Services;
using Play.Studio.Module.Templates;
using Play.Studio.Module.Views;
using UserControl = System.Windows.Controls.UserControl;

namespace Play.Studio.View
{
    /// <summary>
    /// NewProject.xaml 的交互逻辑
    /// </summary>
    public partial class NewProject : UserControl, IView
    {
        public NewProject()
        {
            InitializeComponent();

            OnLoaded();
        }

        private void OnLoaded() 
        {
            // 创建分类头
            Category category = new Category();
            // 读取到所有项目数据模板
            foreach (var template in ProjectTemplate.Templates)
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
            get { return "New Project"; }
        }

        public event System.EventHandler OnClosed;

        public Size FloatSize
        {
            get { return new Size(500, 350); }
        }

        private void FolderBrower_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ctrl = new FolderBrowserDialog();
            if (ctrl.ShowDialog() == DialogResult.OK) 
            {
                if (FolderText.Text != ctrl.SelectedPath)
                {
                    FolderText.Text = ctrl.SelectedPath;
                    ProjectName_TextChanged(null, null);
                }
            }
        }

        private void _category_OnSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            info.Content = _category.SelectionItem == null ? string.Empty : _category.SelectionItem.Description;
            ProjectName_TextChanged(null, null);
        }

        private static string CheckPathName(string name)
        {
            if (name.Length == 0 || !char.IsLetter(name[0]) && name[0] != '_')
            {
                return "Illegal item name: project name must start with letter.";
            }
            if (!FileService.IsValidPath(name))
            {
                return "not valid path.";
            }
            if (name.EndsWith("."))
            {
                return "Illegal item name: project name may not end with '.'.";
            }
            if (FileService.IsDirectory(name)) 
            {
                return "set up valid path";
            }

            return null;
        }

        private void create_Click(object sender, RoutedEventArgs e)
        {
            ProjectTemplate template = _category.SelectionItem.Tag as ProjectTemplate;

            // 创建项目工程文件
            var projectFilePath = Path.Combine(ProjectPath, ProjectName.Text + ".proj");
            template.WriteTo(projectFilePath, new Dictionary<string, string>() 
            {
                { "ProjectPath", ProjectPath },
                { "ProjectFilePath", projectFilePath },
                { "@{StandardNamespace}", "TestNamespace" },
            });

            CommandManager.Execute("LoadProject", projectFilePath);
        }

        private string ProjectPath
        {
            get { return createFolder.IsChecked.Value ? Path.Combine(FolderText.Text, ProjectName.Text) : FolderText.Text; }
        }

        private void ProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var iii = CheckPathName(ProjectPath);
            if (string.IsNullOrEmpty(iii))
            {
                create.IsEnabled = true;
                if (_category.SelectionItem == null || _category.SelectionItem.Tag == null) 
                {
                    create.IsEnabled = false;
                    info.Content = "需要选择一个有效的项目模板.";
                }
                if (string.IsNullOrEmpty(FolderText.Text)) 
                {
                    create.IsEnabled = false;
                    info.Content = "需要指定一个有效目录.";
                }
            }
            else 
            {
                create.IsEnabled = false;
                info.Content = iii;
            }
        }


        public void Refersh(params object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}
