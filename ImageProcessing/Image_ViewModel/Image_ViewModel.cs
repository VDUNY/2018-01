using System;
using System.Collections.Generic;        // KeyValuePair
using System.Collections.Concurrent;    // ConcurrentDictionary<T, T>
using System.Collections.ObjectModel;
using System.Data.Odbc;     // read palettes in from file; reqs ref to System.Data
using System.IO;            // Directory class
using System.Windows;   // Visibility
using System.Windows.Media;       // PixelFormats, Color
using System.Windows.Media.Imaging;     // BitmapPalette; reqs ref to WindowsBase, PresentationCore
using System.Windows.Input; // for ICommand; requires ref to PresentationCore
using System.Runtime.CompilerServices;    // [CallerFilePath], etc
using System.Threading;
using Prism.Mvvm;

using Image_Control_View;
using LLE.Util;     // logging

namespace Image_ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageViewModel : BindableBase, IDisposable
    {

        #region delegates, events

        public delegate void DelExceptionRaised(string msg);    // notification that something went wrong
        public event DelExceptionRaised ExceptionRaised;

        #endregion delegates, events

        #region backing vars

        private ObservableCollection<ImageControlView> m_imageViews = new ObservableCollection<ImageControlView>();

        Object objLock = new object();

        ConcurrentDictionary<string, string> m_appProperties = null;

        #endregion backing vars

        #region enums
        #endregion enums

        #region ctors, dtor, dispose

        /// <summary>
        /// For creation of vars, etc.
        /// Any network or hardware communication is handled in the Initialization() method.
        /// </summary>
        public ImageViewModel()
        {
            m_appProperties = new ConcurrentDictionary<string, string>();


        }   // public ImageViewModel()

        /// <summary>
        /// Got here by user code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } // public void Dispose()

        /// <summary>
        /// Got here either by user code or by garbage collector. If param false, then gc.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {       // managed resources here
                foreach (ImageControlView uc in m_imageViews)
                {
                    uc.Dispose();
                }
                m_appProperties = null;
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
        public void Initialize()
        {
                // need two image views
                m_imageViews.Add(new ImageControlView( "X"));
                m_imageViews.Add(new ImageControlView("Y"));
        } // public void Initialize()

        #endregion initialization

        #region properties

        /// <summary>
        /// holds one or more image controls, depending on whether station or cart
        /// </summary>
        public ObservableCollection<ImageControlView> ImageControlsSettings
        {
            get { return m_imageViews; }
            set { SetProperty<ObservableCollection<ImageControlView>>(ref m_imageViews, value); OnPropertyChanged( () => ImageControlsSettings); }
        }

        #endregion properties

        #region ICommands
        #endregion ICommands

        #region algorithm code
        #endregion algorithm code

        #region hardware code
        #endregion hardware code

        #region utility methods

        /// <summary>
        /// Aspect-oriented programming
        /// Break down program logic into distinct parts, aka concerns, one of which is handling exceptions.
        /// </summary>
        /// <param name="ex"></param>
        private void HandleExceptions(Exception ex)
        {
            OnExceptionRaised(ex.ToString()); // pass it on to any subscribers
        }   // private void HandleExceptions(Exception ex)

        /// <summary>
        /// wraps the test for any subscribers to the event before raising it
        /// </summary>
        /// <param name="msg"></param>
        private void OnExceptionRaised(string msg)
        {
            if (ExceptionRaised != null)
            {
                ExceptionRaised(msg);
            }
        }   // private void OnExceptionRaised(string msg)

        #endregion utility methods
 
        #region event sinks
        

        #endregion event sinks

    }   // public class ImageViewModel

}   // namespace Image_ViewModel
