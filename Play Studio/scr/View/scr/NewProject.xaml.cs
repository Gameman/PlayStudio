using System.Linq;
using System.Windows.Controls;
using Play.Studio.Module.Templates;
using Play.Studio.View.Image;
using System.Windows;

namespace Play.Studio.View
{
    /// <summary>
    /// NewProject.xaml 的交互逻辑
    /// </summary>
    public partial class NewProject : UserControl
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

        private void FolderBrower_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
