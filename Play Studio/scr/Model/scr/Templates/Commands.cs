using Play.Studio.Core.Command;

namespace Play.Studio.Module.Templates
{
    
    public class Commands
    {
        public static readonly ICommand UpdateProjectTemplates;
        public static readonly ICommand UpdateProjectItemTemplates;
        public static readonly ICommand UpdateMenuItemTemplates;

        static Commands() 
        {
            UpdateProjectTemplates = new FuncCommand("UpdateProjectTemplates", o => 
            {
                Template<ProjectTemplate>.UpdateTemplates();
                return CommandResult.SUCCESS;
            });

            UpdateProjectItemTemplates = new FuncCommand("UpdateProjectItemTemplates", o => 
            {
                Template<ProjectItemTemplate>.UpdateTemplates();
                return CommandResult.SUCCESS;
            });

            UpdateMenuItemTemplates = new FuncCommand("UpdateMenuItemTemplates", o => 
            {
                Template<MenuTemplate>.UpdateTemplates();
                return CommandResult.SUCCESS;
            });

            CommandManager.Register(UpdateProjectTemplates);
            CommandManager.Register(UpdateProjectItemTemplates);
            CommandManager.Register(UpdateMenuItemTemplates);
        }
    }
}
