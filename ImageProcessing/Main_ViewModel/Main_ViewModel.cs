using System;
using System.Collections.Generic;       // KeyValuePair
using System.Collections.Concurrent;    // ConcurrentDictionary<T,T>; threadsafe access to collection
using System.Collections.ObjectModel;   // ObservableCollection<T>
using System.Windows.Input; // for ICommand; requires ref to PresentationCore
using System.IO;                // Hdf file existence, dir creation
using System.Threading;
using System.Linq;  // ToDictionary() extension method

using System.Diagnostics;   // EventLog
using System.Runtime.CompilerServices;  // [CallerFileName], etc
using LLE.Util;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using ModuleMessages;

namespace Main_ViewModel
{
    /// <summary>
    /// Maintain the logic of keeping the user interface updated as to controls enabled, items checked, etc
    /// </summary>
    public class MainViewModel : BindableBase, IDisposable 
    {

        #region delegates, events, threads and locking vars

        // Use this for catastrophic info to user. 
        // Use dependency property MsgFmViewModel for status and warnings.
        public delegate void DelExceptionRaised(string msg);
        public event DelExceptionRaised ExceptionRaised;

        // signals the view to show the help-about window
        public delegate void DelHelpAboutRaised(string msg);
        public event DelHelpAboutRaised HelpAboutRaised;

        #endregion delegates, events, threads and locking vars

        #region backing vars

        object objLock = new object();
        string m_statusMsg = "";    // DP StatusMsg

        #endregion backing vars

        #region enums

        #endregion enums

        #region ctors, dtor, dispose

        /// <summary>
        /// For creation of vars, etc.
        /// Any network or hardware communication is handled in the Initialization() method.
        /// </summary>
        public MainViewModel()
        {
            HelpAboutCommand = new DelegateCommand<object>(param => { string msg = "Help and About"; OnHelpAboutRaised(msg); });
            ClearStatusMsgCommand = new DelegateCommand<object>(param => { StatusMsg = ""; });
            StatusMessageEvent.Instance.Subscribe(StatusMessageReceived);
        }   // MainViewModel()

        /// <summary>
        /// Got here by user code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } // protected void Dispose()

        /// <summary>
        /// Got here either by user code or by garbage collector. If param false, then gc.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {       // managed resources here
            }
            // unmanaged resources here
            {
            }
        } // protected virtual void Dispose(bool disposing)

        #endregion ctors, dtor, dispose

        #region initialization

        /// <summary>
        /// Handle all network and hardware communication required for initialization
        /// </summary>
        /// <param name="properties">application properties. we are going to add values from db configuration.</param>
        public bool Initialize()
        {
            bool bRetVal = true;
            StatusMsg = " Started ... ";
            return bRetVal;
        } // public void Initialize()

        #endregion initialization

        #region properties
        #endregion properties

        #region bindable properties

        /// <summary>
        /// Things the user should know about.
        /// </summary>
        public string StatusMsg
        {
            get { return m_statusMsg; }
            set { SetProperty<string>(ref m_statusMsg, value); OnPropertyChanged(() => StatusMsg); }
        }

        #endregion bindable properties

        #region dependency properties
        #endregion dependency properties

        #region ICommands

        /// <summary>
        /// 
        /// </summary>
        public ICommand ClearStatusMsgCommand { get; private set;}

        /// <summary>
        /// user wants info about the program 
        /// </summary>
        public ICommand HelpAboutCommand { get; private set; }

        #endregion ICommands

        #region algorithm code

        #endregion algorithm code

        #region hardware code
        #endregion hardware code

        #region utility methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sourceFile"></param>
        /// <param name="lineNumber"></param>
        /// <param name="memberName"></param>
        private void HandleExceptions(Exception ex,
                [CallerFilePath] string sourceFile = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string trace = sourceFile.Substring(sourceFile.LastIndexOf('\\') + 1) + "(" + lineNumber + "): " + memberName + " --> ";
            if (ex.GetType() == typeof(DirectoryNotFoundException))
            {
                OnExceptionRaised(trace + Environment.NewLine + ex.ToString());
            }
            else if (ex.GetType() == typeof (IOException))
            {
                OnExceptionRaised(trace +  Environment.NewLine + ex.ToString());
            }
            else if (ex.GetType() == typeof (UnauthorizedAccessException))
            {
                OnExceptionRaised(trace + Environment.NewLine + ex.ToString());
            }
            else if (ex.ToString().Contains("Cmd Line Values Missing"))
            {
                OnExceptionRaised(trace + Environment.NewLine + " Cmd Line Values Missing ... " + Environment.NewLine + ex.ToString());
            }
            else
            {
                string msg = ex.ToString().Substring(0, ex.ToString().IndexOf('-'));
                OnExceptionRaised(msg + " " + trace + " " + ex.Message);
            }
        }   // private void HandleExceptions(Exception ex,

        /// <summary>
        ///  Try to get problem notifications back to the user via the Status Dependency Property, but something may go 
        ///  seriously wrong before the initialization is complete and the Status DP is bound to the view.
        /// </summary>
        /// <param name="msg">Description of the problem</param>
        private void OnExceptionRaised(string msg)
        {
            // test for null before performing a member access
            ExceptionRaised?.Invoke(msg);   // pass it on (to the view)
        }       // private void OnExceptionRaised(string msg)

        /// <summary>
        /// user requested info about the program
        /// </summary>
        /// <param name="msg"></param>
        private void OnHelpAboutRaised(string msg)
        {
            HelpAboutRaised?.Invoke(msg);
        }   // private void OnHelpAboutRaised(string msg)

        #endregion utility methods

        #region event sinks

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void StatusMessageReceived(StatusMsg msg)
        {
            StatusMsg = msg.Msg;
        }   // private void StatusMsgReceived(StatusMsg msg)

        #endregion event sinks

    }       // MainViewModel

}       // Main_ViewModel


