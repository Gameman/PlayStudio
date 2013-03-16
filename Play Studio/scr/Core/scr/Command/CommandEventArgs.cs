using System;
using System.Collections.Generic;
using System.Text;

namespace Play.Studio.Core.Command
{
	public class CommandEventArgs : EventArgs
	{
		ICommand m_action;
		bool m_result;

		public CommandEventArgs( ICommand action, bool result )
		{
			m_action = action;
			m_result = result;
		}

		public ICommand Action
		{
			get { return m_action; }
		}

		public bool Result
		{
			get { return m_result; }
		}
	}
}
