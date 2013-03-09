using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Play.Studio.Core.Command
{
    public class CommandEventListener
    {
        int m_executeCallCount;
		int m_undoCallCount;
		int m_redoCallCount;

		public int ExecuteCallCount
		{
			get { return m_executeCallCount; }
		}

		public int UndoCallCount
		{
			get { return m_undoCallCount; }
		}

		public int RedoCallCount
		{
			get { return m_redoCallCount; }
		}

		public CommandEventListener( ICommand action )
		{
			m_executeCallCount = 0;
			m_undoCallCount = 0;
			m_redoCallCount = 0;

			action.ExecuteEvent += new EventHandler< CommandEventArgs >( OnActionExecuteEvent );
			action.UndoEvent += new EventHandler< CommandEventArgs >( OnActionUndoEvent );
			action.RedoEvent += new EventHandler< CommandEventArgs >( OnActionRedoEvent );
		}

		public void OnActionExecuteEvent( object sender, CommandEventArgs args )
		{
			m_executeCallCount++;
		}

		public void OnActionUndoEvent( object sender, CommandEventArgs args )
		{
			m_undoCallCount++;
		}

		public void OnActionRedoEvent( object sender, CommandEventArgs args )
		{
			m_redoCallCount++;
		}
    }
}
