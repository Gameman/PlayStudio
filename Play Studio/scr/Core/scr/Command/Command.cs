using System;
using System.Threading;

namespace Play.Studio.Core.Command
{
    /// <summary>
    /// 指令接口
    /// </summary>
    public interface ICommand : System.Windows.Input.ICommand
    {
        event EventHandler<CommandEventArgs> ExecuteEvent;
        event EventHandler<CommandEventArgs> UndoEvent;
        event EventHandler<CommandEventArgs> RedoEvent;

        string                  Name                { get; }

        CommandHistoryOperation HistoryOperation    { get; }

        string                  FailureReason       { get; }
        bool                    Registered          { get; }
        bool                    Executed            { get; }

        bool                    ExecuteWithResult(object param);
        bool                    Undo();

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

        public event EventHandler                   CanExecuteChanged;
		public event EventHandler<CommandEventArgs> ExecuteEvent;
		public event EventHandler<CommandEventArgs> UndoEvent;
		public event EventHandler<CommandEventArgs> RedoEvent;

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

        public void Execute(object param)                                                              
		{
            ExecuteWithResult(param);
		}

        public bool ExecuteWithResult(object param) 
        {
            if (m_executedSuccessfully)
            {
                bool result = OnRedo();

                if (RedoEvent != null)
                {
                    RedoEvent(this, new CommandEventArgs(this, result));
                }

                return result;
            }
            else
            {
                bool result = OnExecute(param);

                m_executed = true;
                m_executedSuccessfully = (result == true);

                if (ExecuteEvent != null)
                {
                    ExecuteEvent(this, new CommandEventArgs(this, result));
                }

                return result;
            }
        }

        public bool Undo()                                                                 
		{
			bool result = OnUndo();

			if ( UndoEvent != null )
			{
				UndoEvent( this, new CommandEventArgs( this, result ) );
			}

			return result;
		}

        protected abstract bool OnExecute(object param);

		virtual protected bool OnUndo()                                                    
		{
			FailureReason = "Undo not supported for " + Name;
            return false;
		}

        protected virtual bool OnRedo()                                                    
		{
			FailureReason = "Redo not supported for " + Name;
            return false;
		}

        public virtual bool CanExecute(object parameter)
        {
            return false;
        }
    }

    /// <summary>
    /// 委托指令
    /// </summary>
    public class FuncCommand : Command 
    {
        private Func<object, bool> m_function;

        public FuncCommand(Func<object, bool> function)
            : this(function.Target.ToString(), function)
        {
        }

        public FuncCommand(string name, Func<object, bool> function)
            : base(name)
        {
            m_function = function;
        }

        protected override bool OnExecute(object param)
        {
            return m_function(param);
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }

}
