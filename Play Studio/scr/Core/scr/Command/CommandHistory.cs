using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace Play.Studio.Core.Command
{
    /// <summary>
    /// 指令历史
    /// </summary>
    public class CommandHistory
    {
        /// <summary>
        /// Static command history that only avilable when user doesn't provide
        /// ISite object.
        /// </summary>
        static CommandHistory staticCommandHistory = null;

        /// <summary>
        /// Sub CommandQueue that uses for command recoording.
        /// </summary>
        CommandCollection recordingCommands = null;

        /// <summary>
        /// For keep tracking nested call of Begin/EndRecoord method.
        /// </summary>
        int recordingNestCount = 0;

 		/// List of actions that can be undone.
		private List< ICommand > m_undoableActions;

		/// List of actions that can be redone.
		private List< ICommand > m_redoableActions;

		/// Event called when the history changes.
		/// This means that actions were added or removed from the undoable
		/// and redoable action lists.
		public event EventHandler Changed;

		/// Event called when the history is cleared.
		public event EventHandler Cleared;

		/// Event called when an action is executed.
		public event EventHandler< CommandEventArgs > ActionExecuted;

		/// Event called when an action is undone.
		public event EventHandler< CommandEventArgs > ActionUndone;

		/// Event called when an action is redone.
		public event EventHandler< CommandEventArgs > ActionRedone;

		/// Check how many actions can be undone. It doesn't relate 1:1 to the number
		/// of undo steps, as some actions may cause more than one action to be undone.
		public int UndoableActionCount
		{
			get { return m_undoableActions.Count; }
		}

		/// Check how many actions can be redone. It doesn't relate 1:1 to the number
		/// of redo steps, as some actions may cause more than one action to be redone.
		public int RedoableActionCount
		{
			get { return m_redoableActions.Count; }
		}

        public CommandHistory()
		{
			m_undoableActions = new List< ICommand >();
			m_redoableActions = new List< ICommand >();
		}



		/**
		 * Clears the history of any undoable and redoable events.
		 * 
		 * Calls the Cleared and Changed events.
		 */
		public void Clear()
		{
			if ( m_undoableActions.Count == 0 && m_redoableActions.Count == 0 )
			{
				return;
			}

			m_undoableActions.Clear();
			m_redoableActions.Clear();

			OnClear();
			OnChange();
		}



		/**
		 * Execute an action on this History.
		 * 
		 * Normally the history will be updated so that the action can be undone
		 * or redone. The exact way how the action affects the history depends
		 * on its HistoryOperation.
		 * 
		 * If the action has been executed already, this operation will fail.
		 * 
		 * When an action is executed successfully, the redoable list of actions
		 * is cleared.
		 * 
		 * Events Cleared, Changed and ActionExecuted may result from this call.
		 * 
		 * @param action Action to execute in this History instance. If a null
		 *		value is passed, the function will return FAILURE.
		 * 
		 * @return Result from executing the action.
		 */ 
		public CommandResult ExecuteAction( ICommand action, object param )
		{
			if ( action == null )
			{
				return CommandResult.FAILURE;
			}

			if ( action.Executed )
			{
				return CommandResult.FAILURE;
			}

			CommandResult result = action.Execute(param);

			if ( result == CommandResult.SUCCESS )
			{
				m_redoableActions.Clear();

				if ( action.HasHistoryOperation( CommandHistoryOperation.CLEAR_ON_SUCCESS ) )
				{
					// We clear before than checking store on success, so that they
					// aren't mutually exclusive.
					Clear();
				}

				if ( action.HasHistoryOperation( CommandHistoryOperation.STORE_ON_SUCCESS ) )
				{
					m_undoableActions.Add( action );
					OnChange();
				}
			}

			OnActionExecuted( action, result );

			return result;
		}


		/**
		 * Undo the last action stored by this History.
		 * 
		 * If the action has the PASS_THROUGH flag set, the next
		 * action will try to be undone too ( recusively ).
		 * 
		 * Events Changed and ActionUndone may result from this operation.
		 * 
		 * @return Result from undoing the action.
		 */
		public CommandResult UndoAction()
		{
			if ( ! CanUndoAction() )
			{
				return CommandResult.FAILURE;
			}

			ICommand action = GetActionToUndo();

			CommandResult result = action.Undo();

			if ( result == CommandResult.SUCCESS )
			{
				RemoveLast( m_undoableActions );
				m_redoableActions.Add( action );
			}
			else
			{
				m_undoableActions.Clear();
			}

			OnChange();
			OnActionUndone( action, result );

			// We check PASS_THROUGH at the end because we want
			// events called individually for each action undone.
			if ( result == CommandResult.SUCCESS )
			{
				if ( action.HasHistoryOperation( CommandHistoryOperation.PASS_THROUGH ) )
				{
					if ( CanUndoAction() )
					{
						result = UndoAction();
					}
				}
			}

			return result;
		}



		/**
		 * Redo the last action undone in this history.
		 * 
		 * If the action has the PASS_THROUGH flag set, the next
		 * action will try to be redone too ( recursively ).
		 * 
		 * Events Changed, Cleared and ActionRedone may result from this operation.
		 * 
		 * @return Result from executing the action.
		 */
		public CommandResult RedoAction()
		{
			if ( ! CanRedoAction() )
			{
				return CommandResult.FAILURE;
			}

			ICommand action = GetActionToRedo();

			CommandResult result = action.Execute(null);

			if ( result == CommandResult.SUCCESS )
			{
				RemoveLast( m_redoableActions );

				if ( action.HasHistoryOperation( CommandHistoryOperation.STORE_ON_SUCCESS ) )
				{
					m_undoableActions.Add( action );
				}

				if ( action.HasHistoryOperation( CommandHistoryOperation.CLEAR_ON_SUCCESS ) )
				{
					Clear();
				}
			}
			else
			{
				m_redoableActions.Clear();
			}

			OnChange();
			OnActionRedone( action, result );

			// We check PASS_THROUGH at the end because we want
			// events called individually for each action redone.
			if ( result == CommandResult.SUCCESS )
			{
				if ( action.HasHistoryOperation( CommandHistoryOperation.PASS_THROUGH ) )
				{
					if ( CanRedoAction() )
					{
						result = RedoAction();
					}
				}
			}

			return result;
		}



		/**
		 * Checks if there are actions that can be undone.
		 * 
		 * @return True if there are actions to undo, false otherwise.
		 */ 
		public bool CanUndoAction()
		{
			return ( 0 < m_undoableActions.Count );
		}



		/**
		 * Checks if there are actions that can be redone.
		 * 
		 * @return True if there are actions to redo, false otherwise.
		 */
		public bool CanRedoAction()
		{
			return ( 0 < m_redoableActions.Count );
		}




		/**
		 * Obtain a list that contains the names of the actions to undo.
		 * 
		 * @return List with the names of the actions to undo.
		 */ 
		public List< String > GetUndoableActionNames()
		{
			return GetActionNames( m_undoableActions );
		}




		/**
		 * Obtain a list that contains the names of the actions to redo.
		 * 
		 * @return List with the names of the actions to redo.
		 */ 
		public List< String > GetRedoableActionNames()
		{
			return GetActionNames( m_redoableActions );
		}



		/**
		 * Obtain a list of names from an action list.
		 * 
		 * @return List with the names of the actions in the list.
		 */ 
		private List< String > GetActionNames( List< ICommand > actionList )
		{
			List< String > actionNames = new List< String >();

			foreach ( ICommand action in actionList )
			{
				actionNames.Add( action.Name );
			}

			return actionNames;
		}



		/**
		 * Obtain the next action that should be undone.
		 * 
		 * @return Action to be undone.
		 */ 
		private ICommand GetActionToUndo()
		{
			return GetLastAction( m_undoableActions );
		}



		/**
		 * Obtain the next action that should be redone.
		 * 
		 * @return Action to be redone.
		 */ 
		private ICommand GetActionToRedo()
		{
			return GetLastAction( m_redoableActions );
		}



		/**
		 * Get the last action on an action list.
		 * 
		 * The list must not be empty.
		 * 
		 * @param actionList List to get the action from.
		 * 
		 * @return Last Action in an action list.
		 */ 
		private ICommand GetLastAction( List< ICommand > actionList )
		{
			Debug.Assert( 0 < actionList.Count );

			return actionList[ actionList.Count - 1 ];
		}



		/**
		 * Remove the last action from an action list.
		 * 
		 * @param actionList List to remove the action from.
		 */ 
		private void RemoveLast( List< ICommand > actionList )
		{
			if ( actionList.Count == 0 )
			{
				return;
			}

			actionList.RemoveAt( actionList.Count - 1 );
		}

        /// <summary>
        /// Record multiple commands as one command.
        /// </summary>
        public void BeginRecordCommands()
        {
            if (recordingNestCount++ == 0)
                recordingCommands = new CommandCollection();
        }

        /// <summary>
        /// Stop recording commands and added as a command.
        /// </summary>
        public void EndRecordCommands()
        {
            if (--recordingNestCount != 0) return;

            // Added recorded commands to commands.
            // If recording commands recored one command, we just add that command to
            // main commands.
            if (recordingCommands.Count != 0)
            {
                if (recordingCommands.Count == 1)
                    m_undoableActions.Add(recordingCommands[0]);
                else
                    m_undoableActions.AddRange(recordingCommands);

                if (Changed != null) Changed(this, EventArgs.Empty);
            }

            recordingCommands = null;
        }

        /// <summary>
        /// Make sure CommandHistory added as a service to given site.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <remarks>You have to make sure added IServiceContainer to this
        /// serviceProvider.</remarks>
        public static CommandHistory EnsureHasService(IServiceProvider serviceProvider = null)
        {
            CommandHistory result = null;
            IServiceProvider sp = serviceProvider;
            if (sp != null)
            {
                // Add this CommandHistory service if given service
                // doesn't contains it.
                result = sp.GetService(typeof(CommandHistory)) as CommandHistory;
                if (result == null)
                {
                    // If there are no service, added new instance.
                    IServiceContainer s = sp.GetService(typeof(IServiceContainer))
                        as IServiceContainer;

                    if (s == null)
                    {
                        throw new InvalidOperationException("empty service container");
                    }

                    result = new CommandHistory();
                    s.AddService(typeof(CommandHistory), result);
                }
            }
            else
            {
                // If they don't have ISite, returns static instance.
                if (staticCommandHistory == null)
                    staticCommandHistory = new CommandHistory();

                result = staticCommandHistory;
            }

            return result;
        }

		#region event signaling


		private void OnChange()
		{
			if ( Changed != null )
			{
				Changed( this, new EventArgs() );
			}
		}

		private void OnClear()
		{
			if ( Cleared != null )
			{
				Cleared( this, new EventArgs() );
			}
		}

		private void OnActionExecuted( ICommand action, CommandResult result )
		{
			if ( ActionExecuted != null )
			{
				ActionExecuted( this, new CommandEventArgs( action, result ) );
			}
		}

		private void OnActionUndone( ICommand action, CommandResult result )
		{
			if ( ActionUndone != null )
			{
				ActionUndone( this, new CommandEventArgs( action, result ) );
			}
		}

		private void OnActionRedone( ICommand action, CommandResult result )
		{
			if ( ActionRedone != null )
			{
				ActionRedone( this, new CommandEventArgs( action, result ) );
			}
		}

		#endregion event signaling
	}
}

