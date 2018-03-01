using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;    // MEF; reqs ref to System.ComponentModel.Composition
using System.Collections; // DictionaryEntry
using System.Collections.Concurrent;        // ConcurrentDictionary<T, T>
using System.Windows.Threading; // DispatcherTimer
using IMainview;
using System.Runtime.CompilerServices;      // [CallerFilePath], etc
using LLE.Util; // logging

using Image_ViewModel;

namespace Image_Control_View
{
    /// <summary>
    /// Interaction logic for Image_Control_View.xaml
    /// </summary>
    public partial class ImageView : UserControl, IMainWindow, IDisposable
    {

        #region delegates, events
        #endregion delegates, events

        #region backing vars

        ImageViewModel m_imageViewModel = null;

        private ConcurrentDictionary<string, string> m_appProperties = null;

        // The Loaded and Unloaded events fire each time the control gets/loses focus. 
        // So we need to guard that the initialization of the viewmodel only happens once
        // and that Dispose() is not prematurely called.
        bool m_bUserControlLoaded = false;
        DispatcherTimer tmrWindowRendered = null;

        #endregion backing vars

        #region enums
        #endregion enums

        #region ctors, dtor, dispose

        /// <summary>
        /// 
        /// </summary>
        public ImageView()
        {
            // The _Initialized event is called by the system within the InitializeComponent() call.
            // Therefore we can't use _Initialized event to do post-construction processing.
            InitializeComponent();

            m_imageViewModel = new ImageViewModel();
        }   // public ImageView()

        /// <summary>
        /// Called by user-code. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }   // public void Dispose()

        /// <summary>
        /// Called by either user-code or the runtime. If runtime, disposing = false;
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {       // managed resources here
                m_imageViewModel.Dispose();
            }
            // unmanaged resources here
            {
            }
        }   // protected virtual void Dispose(bool disposing)

        #endregion ctors, dtor, dispose

        #region initialization

        /// <summary>
        /// Called when the MEF GetExportedValues() method is run. Called after view ctor() completes.
        /// We do not call this method; it's a windows event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageView_Initialized(object sender, EventArgs e)
        {
        }   // private void ImageView_Initialized(object sender, EventArgs e)

        #endregion initialization

        #region MEF

        /// <summary>
        /// allows MEF to make this WPF control/MVVM view available to other apps by discovery at runtime
        /// </summary>
        [Export(typeof(IMainWindow))]
        public IMainWindow Window
        {
            get { return this; }
        }

        /// <summary>
        /// close this control when called by user code or .net
        /// </summary>
        [Export]
        public void Close()
        {
            Dispose();
        }

        [Export]
        public String ServiceName { get { return "Image View"; } }

        #endregion MEF

        #region Windows events

        /// <summary>
        /// This event fires when the focus is moved back to the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!m_bUserControlLoaded)
            {
                m_imageViewModel.Initialize();
                m_bUserControlLoaded = true;
                tmrWindowRendered = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Send, tmrWindowRendered_Tick, this.Dispatcher);
            }
        }   // private void ImageView_Loaded(object sender, RoutedEventArgs e)

        /// <summary>
        /// This event fires when the focus is moved off the control.
        /// So Dispose() is not appropriate here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageView_Unloaded(object sender, RoutedEventArgs e)
        {
        }   // private void ImageView_Unloaded(object sender, RoutedEventArgs e)

        /// <summary>
        /// After rendering, the controls have actual height/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrWindowRendered_Tick(object sender, EventArgs e)
        {
            tmrWindowRendered.Stop();
            tmrWindowRendered = null;
            this.DataContext = m_imageViewModel;
        }   // private void tmrWindowRendered_Tick(object sender, EventArgs e)

        #endregion Windows events

        #region properties
        #endregion properties

        #region ICommands
        #endregion ICommands

        #region algorithm code
        #endregion algorithm code

        #region hardware code
        #endregion hardware code

        #region utility methods
        #endregion utility methods

        #region event sinks
        #endregion event sinks

    }   // public partial class ImageView : UserControl, ICryoviewWindow, IDisposable

}   // namespace Image_Control_View