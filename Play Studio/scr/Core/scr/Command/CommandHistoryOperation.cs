using System;

namespace Play.Studio.Core.Command
{
	[Flags]
	public enum CommandHistoryOperation
	{
		/// The action isn't saved in the action history when executed.
		NONE = 0,
		
		/// The action is stored in the action history when executed successfully.
		STORE_ON_SUCCESS = 1,

		/// The action clears the previous action history when executed successfully.
		CLEAR_ON_SUCCESS = 2,

		/// When undone or redone, the next action on the action history will be undone or redone too.
		PASS_THROUGH = 4
	}
}