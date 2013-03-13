using System;

namespace Play.Studio.Core.Command
{
    public class CommandHistoryEventListener
    {
        	int m_executedCallCount;
		int m_undoneCallCount;
		int m_redoneCallCount;
		int m_changedCallCount;
		int m_clearedCallCount;

		public int ExecutedCallCount
		{
			get { return m_executedCallCount; }
		}

		public int UndoneCallCount
		{
			get { return m_undoneCallCount; }
		}

		public int RedoneCallCount
		{
			get { return m_redoneCallCount; }
		}

		public int ChangedCallCount
		{
			get { return m_changedCallCount; }
		}

		public int ClearedCallCount
		{
			get { return m_clearedCallCount; }
		}

		public CommandHistoryEventListener( CommandHistory history )
		{
			m_executedCallCount = 0;
			m_undoneCallCount = 0;
			m_redoneCallCount = 0;
			m_changedCallCount = 0;
			m_clearedCallCount = 0;

			history.ActionExecuted += new EventHandler<CommandEventArgs>( history_ActionExecuted );
			history.ActionUndone += new EventHandler<CommandEventArgs>( history_ActionUndone );
			history.ActionRedone += new EventHandler<CommandEventArgs>( history_ActionRedone );
			history.Changed += new EventHandler( history_Changed );
			history.Cleared += new EventHandler( history_Cleared );
		}

		void history_ActionExecuted( object sender, CommandEventArgs e )
		{
			m_executedCallCount++;
		}

		void history_ActionUndone( object sender, CommandEventArgs e )
		{
			m_undoneCallCount++;
		}

		void history_ActionRedone( object sender, CommandEventArgs e )
		{
			m_redoneCallCount++;
		}

		void history_Changed( object sender, EventArgs e )
		{
			m_changedCallCount++;
		}

		void history_Cleared( object sender, EventArgs e )
		{
			m_clearedCallCount++;
		}
    }
}
