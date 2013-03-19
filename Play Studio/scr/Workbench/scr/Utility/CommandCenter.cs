using System;
using Play.Studio.Core.Command;
using Play.Studio.Module.Templates;
using Play.Studio.Workbench.Solution;

namespace Play.Studio.Workbench.Utility
{
    /// <summary>
    /// 指令中心
    /// </summary>
    public static class CommandCenter
    {
        // templates
        public static ICommand UpdateProjectTemplates       { get; private set; }
        public static ICommand UpdateProjectItemTemplates   { get; private set; }
        public static ICommand UpdateMenuItemTemplates      { get; private set; }

        // project
        public static ICommand LoadProject                  { get; private set; }
        public static ICommand LoadProjectWithDialog        { get; private set; }

        // views
        public static ICommand ShowView                     { get; private set; }
        public static ICommand RefershView                  { get; private set; }

        public static void Initialization() 
        {
            InitViewCommands();
            InitProjectCommands();
            InitTemplateCommands();
        }

        private static void InitViewCommands()          
        {
            ShowView = new FuncCommand("ShowWindows", o =>
            {
                var windowType = Type.GetType(o.ToString() + ", " + typeof(Play.Studio.View.PluginExplorer).Assembly.FullName);
                Workbench.Current.ShowView(windowType);
                return true;
            });

            RefershView = new FuncCommand("RefershView", o => 
            {
                var windowType = Type.GetType(o.ToString() + ", " + typeof(Play.Studio.View.PluginExplorer).Assembly.FullName);
                Workbench.Current.RefershView(windowType, SolutionHost.CurrentProject);
                return true;
            });

            CommandManager.Register(ShowView);
            CommandManager.Register(RefershView);
        }
        private static void InitProjectCommands()       
        {

            LoadProject = new FuncCommand("LoadProject", o =>
            {
                return SolutionHost.Load(o.ToString()) != null;
            });

            LoadProjectWithDialog = new FuncCommand("LoadProjectWithDialog", o =>
            {
                Microsoft.Win32.OpenFileDialog ctrl = new Microsoft.Win32.OpenFileDialog();
                if (ctrl.ShowDialog().Value)
                {
                    return SolutionHost.Load(ctrl.FileName) != null;
                }

                return false;
            });


            CommandManager.Register(LoadProject);
            CommandManager.Register(LoadProjectWithDialog);
        }
        private static void InitTemplateCommands()      
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

            CommandManager.Register(UpdateProjectTemplates);
            CommandManager.Register(UpdateProjectItemTemplates);
            CommandManager.Register(UpdateMenuItemTemplates);
        }
    }
}
