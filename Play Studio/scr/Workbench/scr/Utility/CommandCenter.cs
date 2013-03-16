using System;
using Play.Studio.Core.Command;
using Play.Studio.Module.Templates;

namespace Play.Studio.Workbench.Utility
{
    /// <summary>
    /// 指令中心
    /// </summary>
    public static class CommandCenter
    {
        public static readonly ICommand UpdateProjectTemplates;
        public static readonly ICommand UpdateProjectItemTemplates;
        public static readonly ICommand UpdateMenuItemTemplates;

        public static readonly ICommand ShowWindows;

        public static void Initialization() 
        {
        }

        static CommandCenter()
        {
            UpdateProjectTemplates = new FuncCommand("UpdateProjectTemplates", o =>
            {
                Template<ProjectTemplate>.UpdateTemplates();

                return true;
            });

            UpdateProjectItemTemplates = new FuncCommand("UpdateProjectItemTemplates", o =>
            {
                Template<ProjectItemTemplate>.UpdateTemplates();

                return true;
            });

            UpdateMenuItemTemplates = new FuncCommand("UpdateMenuItemTemplates", o =>
            {
                Template<MenuTemplate>.UpdateTemplates();

                return true;
            });

            ShowWindows = new FuncCommand("ShowWindows", o => 
            {
                var windowType = Type.GetType(o.ToString() + ", " + typeof(Play.Studio.View.NewProject).Assembly.FullName);
                Workbench.Current.ShowWindow(windowType);
                return true;
            });

            CommandManager.Register(UpdateProjectTemplates);
            CommandManager.Register(UpdateProjectItemTemplates);
            CommandManager.Register(UpdateMenuItemTemplates);
            CommandManager.Register(ShowWindows);
        }
    }
}
