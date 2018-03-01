using System;

using System.Diagnostics;   // DebuggerStepThru
using System.Windows.Input; // for ICommand and CommandManager; requires ref to PresentationCore

namespace ViewModelMVVM
{
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    /// <remarks>
    /// *** Sample usage in a viewmodel class; not all inclusive re binding and datacontext
    /// using System.Windows.Input; // for ICommand and CommandManager; requires ref to PresentationCore
    /// using ViewModelMVVM
    /// RelayCommand m_helpAboutCommand = null;
    /// m_helpAboutCommand = new RelayCommand(param => this.HelpAbout(), param => this.CanHelpAbout);
    /// public ICommand HelpAboutCommand { get { return m_helpAboutCommand; } }
    /// public bool CanHelpAboutComnand { get { return true; } }
    /// private void HelpAbout() { string msg = "Help and About";  }
    /// </remarks>
    public class RelayCommand : ICommand
    {

        #region delegates, events, threads and locking vars

        public delegate void CancelCommandEventHandler(object sender, CancelCommandEventArgs args);

        public event EventHandler Executed; // notify subscribers when the requested action is complete
        public event EventHandler AllowExecuteChanged; // notify subscribers that the command status changed
        public event CancelCommandEventHandler Executing;

        #endregion delegates, events, threads and locking vars
        
        #region backing vars

        readonly Action<object> _execute;   // delegate to represent a method with a single param and does not return a value

            // delegate that represents a method that defines a set of criteria and whether the object meets the criteria
            // object is contravariant, that is it accepts either the type specified or a less derived type. E.g., specify 'dog', accepts 'animal'
        readonly Predicate<object> _canExecute;

        // provides a binding mechanism from a bool control, not bound to cmd object,
        // to change an ICommand object's execute status
        private bool _bAllowExecute = false;
        
        #endregion backing vars

        #region enums

        #endregion enums

        #region  ctors, dtors, dispose

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute; // Action<object> single param and doesn't return a value
            _canExecute = canExecute;   // Predicate<object>
        }

        #endregion  ctors, dtors, dispose

#region Properties

        /// <summary>
        /// Mechanism for a UI control, not bound to command object,
        /// to bind and update status of ICommand object, e.g. Checkbox.IsChecked
        /// </summary>
        public bool AllowExecute
        {
            get
            {
                return _bAllowExecute;
            }
            set
            {
                _bAllowExecute = value;
                if (AllowExecuteChanged != null)
                {
                    AllowExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

#endregion Properties

        #region ICommand Members

        // instructs debugger to step thru ran than into the code
        // necessary because this predicate delegate is invoked whenever the focus
        // is changed on the UI which is/can be quite often.
        [DebuggerStepThrough] 
        public bool CanExecute(object parameter) // should the user interface allow the cmd to be executed?
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary>
        /// Causes a refresh to update wpf bindings
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// ensures that the appropriate command object executes
        /// </summary>
        /// <param name="parameter">ICommand object</param>
        public void Execute(object parameter)
        {
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = parameter, Cancel = false };
            OnInvokeExecuting(args);
            if (args.Cancel) return;
            _execute(parameter);
            OnInvokeExecuted(new CommandEventArgs() { Parameter = parameter });
        }

        #endregion ICommand Members

        #region event sinks

        #endregion event sinks

        #region Utility methods

        /// <summary>
        /// Event that signals that the relay command has executed
        /// </summary>
        /// <param name="args"></param>
        protected void OnInvokeExecuted(CommandEventArgs args)
        {
            if (Executed != null)
            {
                Executed(this, args);
            }
        }

        /// <summary>
        /// Event that signals that the relay command is executing
        /// </summary>
        /// <param name="args"></param>
        protected void OnInvokeExecuting(CancelCommandEventArgs args)
        {
            if (Executing != null)
            {
                Executing(this, args);
            }
        }

        #endregion Utility methods

    }   // RelayCommand : ICommand

    /// <summary>
    /// CommandEventArgs - simply holds the command parameter.
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        public object Parameter { get; set; }
    }

    /// <summary>
    /// CancelCommandEventArgs - just like above but allows the event to 
    /// be cancelled.
    /// </summary>
    public class CancelCommandEventArgs : CommandEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CancelCommandEventArgs"/> command should be cancelled.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool Cancel { get; set; }
    }		// CommandEventArgs

}   // ViewModelMVVM

