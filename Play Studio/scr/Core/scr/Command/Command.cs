using System;
using System.Threading;

namespace Play.Studio.Core.Command
{
    /// <summary>
    /// 指令接口
    /// </summary>
    public interface ICommand
    {
        /// Event triggered when the action has been executed.
        event EventHandler<CommandEventArgs> ExecuteEvent;


        /// Event triggered when the action has been undone.
        event EventHandler<CommandEventArgs> UndoEvent;

        /// Event triggered when the action has been redone.
        event EventHandler<CommandEventArgs> RedoEvent;

        /// Descriptive name of the action.
        String Name
        {
            get;
        }

        /// Effect of the action on the action history.
        CommandHistoryOperation HistoryOperation
        {
            get;
        }

        /// String containing a description of the latest error.
        String FailureReason
        {
            get;
        }


        bool Registered     { get; }

        bool Executed       { get; }

        /**
         * Execute this action.
         * 
         * In case of failure, the string FailureReason should indicate the reason.
         * 
         * It will call ExecuteEvent or RedoEvent, depending on the circumstances.
         * 
         * @return Result of the operation indicating if it was successful or not.
         */
        CommandResult Execute(object param);

        /**
         * Undo this action
         * 
         * In case of failure, the string FailureReason should indicate the reason.
         * 
         * If successful it will call UndoEvent.
         * 
         * @return Result of the operation indicating if it was successful or not.
         */
        CommandResult Undo();


        /**
         * Query history operation combination is active.
         * 
         * All the flags queried in a single call need to be active
         * for the function to return true.
         * 
         * @param historyOperationFlags Contains the flags to query.
         * 
         * @return True if action has all of the flags in historyOperationFlags
         *		active, false otherwise.
         */
        bool HasHistoryOperation(CommandHistoryOperation historyOperationFlag);
    }

    /// <summary>
    /// 指令
    /// </summary>
    public abstract class Command : ICommand    
    {
  	    String m_name;
		CommandHistoryOperation m_historyOperation;
		String m_failureReason;
		bool m_executedSuccessfully;
		bool m_executed;

		public event EventHandler< CommandEventArgs > ExecuteEvent;
		public event EventHandler< CommandEventArgs > UndoEvent;
		public event EventHandler< CommandEventArgs > RedoEvent;

		public String Name                                                                          
		{
			get { return m_name; }
		}

		public CommandHistoryOperation HistoryOperation                                             
		{
			get { return m_historyOperation; }
		}

		public String FailureReason                                                                 
		{
			get { return m_failureReason; }
			protected set { m_failureReason = value; }
		}

        /// <summary>
        /// 获取是否已注册
        /// </summary>
        public bool Registered
        {
            get;
            internal set;
        }

		public bool Executed                                                                        
		{
			get { return m_executed; }
		}

        public Command() 
            : this(Thread.CurrentContext.ContextID.ToString())                                      
        {
        }

		public Command( String name )                                                               
		{
			m_executed = false;
			m_name = name;

			m_historyOperation = CommandHistoryOperation.NONE;
		}

		protected void AddHistoryOperation( CommandHistoryOperation historyOperationFlags )         
		{
			m_historyOperation |= historyOperationFlags;
		}

		protected void RemoveHistoryOperation( CommandHistoryOperation historyOperationFlags )      
		{
			m_historyOperation &= ~historyOperationFlags;
		}

		public bool HasHistoryOperation( CommandHistoryOperation historyOperationFlags )            
		{
			return ( ( m_historyOperation & historyOperationFlags ) == historyOperationFlags );
		}

        public CommandResult Execute(object param)                                                              
		{
			if ( m_executedSuccessfully )
			{
				CommandResult result = OnRedo();

				if ( RedoEvent != null )
				{
					RedoEvent( this, new CommandEventArgs( this, result ) );
				}

				return result;
			}
			else
			{
				CommandResult result = OnExecute(param);
				
				m_executed = true;
				m_executedSuccessfully = ( result == CommandResult.SUCCESS );

				if ( ExecuteEvent != null )
				{
					ExecuteEvent( this, new CommandEventArgs( this, result ) );
				}

				return result;
			}
		}

        public CommandResult Undo()                                                                 
		{
			CommandResult result = OnUndo();

			if ( UndoEvent != null )
			{
				UndoEvent( this, new CommandEventArgs( this, result ) );
			}

			return result;
		}

        protected abstract CommandResult OnExecute(object param);

		virtual protected CommandResult OnUndo()                                                    
		{
			FailureReason = "Undo not supported for " + Name;
			return CommandResult.FAILURE;
		}

        protected virtual CommandResult OnRedo()                                                    
		{
			FailureReason = "Redo not supported for " + Name;
			return CommandResult.FAILURE;
		}
    }

    /// <summary>
    /// 委托指令
    /// </summary>
    public class FuncCommand : Command 
    {
        private Func<object, CommandResult> m_function;

        public FuncCommand(Func<object, CommandResult> function)
            : this(function.Target.ToString(), function)
        {
        }

        public FuncCommand(string name, Func<object, CommandResult> function)
            : base(name)
        {
            m_function = function;
        }

        protected override CommandResult OnExecute(object param)
        {
            return m_function(param);
        }
    }

}
