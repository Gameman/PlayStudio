using System.Windows.Controls;
using Play.Studio.Model.Templates;

namespace Play.Studio.View.Converters
{
    [TemplateConverter(typeof(MenuTemplate))]
    public class MenuItemConverter : TemplateConverter<MenuItem>
    {
        protected override MenuItem OnConvert<TInput>(TInput arg)
        {
            return OnConvert(arg as MenuTemplate);
        }

        protected MenuItem OnConvert(MenuTemplate arg)
        {
            MenuItem item = null;

            foreach (var unit in arg.Units) 
            {
                item = OnConvert(unit);
            }

            return item;
        }

        private static MenuItem OnConvert(MenuTemplateUnit unit) 
        {
            var item = new MenuItem();
            item.Header = unit.Header;
            foreach (var sub in unit.Subs)
            {
                if (sub.Header == "-")
                {
                    item.Items.Add(new Separator());
                }
                else
                {
                    item.Items.Add(OnConvert(sub));
                }
            }

            return item;
        }
    }
}
