using System;
using System.Collections.Generic;
using System.Text;

namespace Play.Studio.Core.Command
{
	public class CommandEventArgs : EventArgs
	{
		/// Action that triggered the event.
		ICommand m_action;

		/// Result of the action that triggered the event.
		CommandResult m_result;

		public CommandEventArgs( ICommand action, CommandResult result )
		{
			m_action = action;
			m_result = result;
		}

		public ICommand Action
		{
			get { return m_action; }
		}

		public CommandResult Result
		{
			get { return m_result; }
		}
	}
}
