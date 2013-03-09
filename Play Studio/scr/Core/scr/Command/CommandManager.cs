using System.Collections.Generic;
using Play.Studio.Core.Services;

namespace Play.Studio.Core.Command
{
    /// <summary>
    /// 指令管理器
    /// </summary>
    public static class CommandManager
    {
        private static CommandHistory s_generalCommandHistory = new CommandHistory();
        private static Dictionary<string, ICommand> s_registerCommands = new Dictionary<string, ICommand>();

        /// <summary>
        /// 注册公共命令
        /// </summary>
        public static void Register(ICommand command) 
        {
            if (!command.Registered)
                s_registerCommands.Add(command.Name, command);
        }

        /// <summary>
        /// 注册注册命令
        /// </summary>
        public static void Unregister(ICommand command) 
        {
            if (command.Registered)
                s_registerCommands.Remove(command.Name);
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        public static CommandResult Execute(string commandStr, object param) 
        {
            if (s_registerCommands.ContainsKey(commandStr))
                return Execute(s_registerCommands[commandStr], param);
            else
                return CommandResult.FAILURE;
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        public static CommandResult Execute(ICommand command, object param) 
        {
            return s_generalCommandHistory.ExecuteAction(command, param);
        }

    }
}
