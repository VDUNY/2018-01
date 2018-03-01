using System;
using System.Collections;   // DictionaryEntry
using System.Collections.Concurrent;    // ConcurrentDictionary<T, T>
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;   // MouseButtonEventArgs, etc
using System.Windows.Data;  // BindingExpression
using System.ComponentModel.Composition;    // MEF; reqs ref to System.ComponentModel.Composition
using System.Runtime.CompilerServices;      // [CallerFilePath], etc
using System.Threading;
using System.Windows.Threading; // DispatcherTimer
using LLE.Util; // logging
using System.Windows.Media; // PixelFormats
using System.Windows.Media.Imaging; // BitmapSource

using ModuleMessages;
using ViewImages;
using Image_Control_ViewModel;
using System.Diagnostics;

namespace Image_Control_View
{
    /// <summary>
    /// Interaction logic for ImageControlView.xaml
    /// </summary>
    public partial class ImageControlView : UserControl, IDisposable
    {

        #region delegates, events, threads and locking vars

        public delegate void DelExceptionRaised(string msg);    // async notification that something went wrong
        public event DelExceptionRaised ExceptionRaised;

        #endregion delegates, events, threads and locking vars

        #region backing vars

        private ConcurrentDictionary<string, string> m_appProperties = null;    // hold Application-wide properties
        private ImageControlViewModel m_imageControlViewModel = null;

        private DispatcherTimer m_tmrStartUp = null;
        private MovingMarker m_markerBeingMoved = MovingMarker.NONE;

            // ratio of reticle width to bmp width; keeps reticle proportional to image resizing
        private Single m_ratioSizeImageChange = 1.0f;
            // User to calculate percent change in size of border for image on SizeChanged().
            // If init'd to (0.0, 0.0), leads to percent change = NaN because of DivBy0.
        private Size m_imgSizePreResize = new Size(0, 0);   

        // The Loaded and Unloaded events fire each time the control is rendered, e.g. in a wpf tab control.
        // So we need to guard that the initialization of the viewmodel only happens once
        // and that Dispose() is not prematurely called.
        bool m_bUserControlLoaded = false;
        //
        bool m_bFirstTimeSizeChanged = true;

        VideoStreamView videoStream = null;

        #endregion backing vars

        #region enums

        enum MovingMarker
        {
            NONE,
            FIDUCIAL1,
            FIDUCIAL2,
            RETICLE
        };

        #endregion enums

        #region ctors, dtor, dispose

        /// <summary>
        /// Will generally be called by .net MEF runtime.
        /// Called when the MEF GetExportedValues() method is run.
        /// </summary>
        public ImageControlView()    // must be public for xaml designer to find. But not used at runtime, as we need the id.
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            m_imageControlViewModel = new ImageControlViewModel();
        }

        public ImageControlView(string id)
        {
            InitializeComponent();  // results UserControl_Initialized being raised
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            Id = id;
            m_imageControlViewModel = new ImageControlViewModel(Id);
            m_appProperties = new ConcurrentDictionary<string, string>();
            foreach (DictionaryEntry de in Application.Current.Properties)
            {       // cmd line params and values read in from app config file
                m_appProperties.AddOrUpdate(de.Key.ToString(), de.Value.ToString(), (k, v) => v);
            }
            // in viewmodel ctor. Need to fire a tmrStartUp event and exit ctor so that the
            // ui controls are available to display an error msg.

            rdoAutoScale.GroupName = id + " Scaling";
            rdoNoScale.GroupName = id + " Scaling";
            rdoStaticScale.GroupName = id + " Scaling";
            rdoExpandScale.GroupName = id + " Scaling";

            m_tmrStartUp = new DispatcherTimer();  // can't handle hardware exception and provide warning
            m_tmrStartUp.Interval = TimeSpan.FromMilliseconds(100);
            m_tmrStartUp.Tick += new EventHandler(m_tmrStartUp_Tick);
            m_tmrStartUp.Start();
        }   // public ImageControlView(string id)

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
                if (videoStream != null)
                {
                    videoStream.Dispose();
                    videoStream.Close();
                }
                m_imageControlViewModel.Dispose();
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
        private void ImageControlView_Initialized(object sender, EventArgs e)
        {
        }       

        #endregion initialization

        #region MEF
        #endregion MEF

        #region Windows events

        /// <summary>
        /// handles the x, y, z and theta position of the target
        /// </summary>
        /// <param name="sender"></param>1
        /// <param name="e"></param>
        /// <remarks> called each time the window is shown </remarks>
        private void ImageControlView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!m_bUserControlLoaded)
            {
                m_bUserControlLoaded = true;
                m_imageControlViewModel.ExceptionRaised += m_imageControlViewModel_ExceptionRaised;

                m_imageControlViewModel.Initialize(m_appProperties);   //  can start w/o network connection
                this.DataContext = m_imageControlViewModel; 
                videoStream = new VideoStreamView(m_imageControlViewModel);
                videoStream.Show();

            }
        }   // private void ImageControlView_Loaded(object sender, RoutedEventArgs e)

        /// <summary>
        /// If any hardware  initialization errors occur during the viewmodel ctor, there is no ui control yet
        /// to display the error msg. So the Window_Loaded event must complete and exit for the ui to be
        /// available. Therefore hardware initialization must follow Window_Loaded. The way to do this
        /// is to fire off a tmrStartUp in Window_Loaded and sink its Tick event which will happen after the UI exists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_tmrStartUp_Tick(object sender, EventArgs e)
        {
            m_tmrStartUp.Stop();     // first tick event occurs after main window loaded
            // in wpf we move ellipses around, not by redrawing them at the new coordinates, but
            fid1Transform.X = 0;    // by applying a new location transform to the design time coordinates.
            fid1Transform.Y = 0;    // e.g. if we center an ellipse at 500, 500 on a 1000 x 1000 window, 
            fid2Transform.X = 0;    // then we apply a transform of (-10, -10) to draw the ellipse at (490, 490).
            fid2Transform.Y = 0;    // we do NOT draw the ellipse at (490, 490).
            Grid.SetZIndex(fid1Camera, (int)80);
            Grid.SetZIndex(fid2Camera, (int)80);

                // in cryoview, the img control has an actual size at this point. Here, the img render size is still (0,0).
            m_ratioSizeImageChange = 1.0f;

        } //  void timer_StartUp(object sender, EventArgs e)


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stckPnlImageControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }   // private void stckPnlImageControls_SizeChanged(object sender, SizeChangedEventArgs e)
        
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Called the first time the user control is displayed, and then when the window size changes.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_bFirstTimeSizeChanged)    // initial call to display the user control, perhaps a tab change?
            {       // regardless, this is the first opportunity to get the rendered size of the image control to store for later use.
                    // Needed to effect transformation of the fid markes and resizing of the reticle
                m_bFirstTimeSizeChanged = false;
                m_imgSizePreResize = imgsrcCamera.RenderSize;
                // new reticle size = percent size of reticle relative to actual image size; 
                // keeps reticle proportional to image size
                reticleCamera.Width = m_ratioSizeImageChange * imgsrcCamera.ActualWidth;
                reticleCamera.Height = m_ratioSizeImageChange * imgsrcCamera.ActualHeight;
            }
            else
            {   
                if ((imgsrcCamera.ActualHeight == 0.0) || (imgsrcCamera.ActualWidth == 0.0))
                {
//                    Debug.WriteLine("ImgPreResize: " + m_imgSizePreResize.Height + " / " + m_imgSizePreResize.Width);
//                    Debug.WriteLine("Height/Width: " + imgsrcCamera.RenderSize + " == " + 
//                        imgsrcCamera.ActualHeight + " / " + imgsrcCamera.ActualWidth);
                }
                else
                {
//                    Debug.WriteLine("Height/Width: " + imgsrcCamera.RenderSize + " == " +
//                        imgsrcCamera.ActualHeight + " / " + imgsrcCamera.ActualWidth);
                    // new reticle size = percent size of reticle relative to actual image size; 
                    // keeps reticle proportional to image size
                    reticleCamera.Width = m_ratioSizeImageChange * imgsrcCamera.ActualWidth;
                    reticleCamera.Height = m_ratioSizeImageChange * imgsrcCamera.ActualHeight;

                    // temp var to keep current size and not overwrite the previous control size
                    Size sizeImage = new Size(imgsrcCamera.ActualWidth, imgsrcCamera.ActualHeight);
                    // used to get the new fiducial marker transforms
                    double imgHSizePercentChange = imgsrcCamera.ActualHeight / m_imgSizePreResize.Height;
                    double imgWSizePercentChange = imgsrcCamera.ActualWidth / m_imgSizePreResize.Width;
                    // now we overwrite the previous control size with the new control size for use on the next SizeChanged() event.
                    m_imgSizePreResize = sizeImage; // save for next change in user control size
//                    Debug.WriteLine("ImgPreResize: " + m_imgSizePreResize.Height + " / " + m_imgSizePreResize.Width);
                    // relocate the fiducial markers
                    fid1Transform.X *= imgHSizePercentChange;   // adjust transform by percent change in size of user control.
                    fid1Transform.Y *= imgWSizePercentChange;   // e.g. if user control is 10% bigger and Transform = (10, 10),
                    fid2Transform.X *= imgHSizePercentChange;   // then new value of Transform is (11, 11).
                    fid2Transform.Y *= imgWSizePercentChange;
                    
                    // these are edge cases that can occur if the image goes way to small, beyond the limits of being visible.
                    if (fid1Transform.X.Equals(Single.NaN)) fid1Transform.X = 1;
                    if (fid1Transform.Y.Equals(Single.NaN)) fid1Transform.Y = 1;
                    if (fid2Transform.X.Equals(Single.NaN)) fid2Transform.X = 1;
                    if (fid2Transform.Y.Equals(Single.NaN)) fid2Transform.Y = 1;
                    if (Double.IsInfinity(fid1Transform.X)) fid1Transform.X = 1;
                    if (Double.IsInfinity(fid1Transform.Y)) fid1Transform.Y = 1;
                    if (Double.IsInfinity(fid2Transform.X)) fid2Transform.X = 1;
                    if (Double.IsInfinity(fid2Transform.Y)) fid2Transform.Y = 1;
                    
                }
            }
        }   //private void imgBorder_SizeChanged(object sender, SizeChangedEventArgs e)

        /// <summary>
        /// User is moving mouse across the x-axis image. If the mouse button is pressed, resize the reticle or move a fiducial marker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgsrcCamera_MouseMove(object sender, MouseEventArgs e)
        {
            if ((m_markerBeingMoved == MovingMarker.RETICLE) && (e.LeftButton == MouseButtonState.Pressed))
            {
                Point mouseDownP = e.GetPosition(imgsrcCamera);
                Point centerP = new Point(imgsrcCamera.ActualWidth / 2, imgsrcCamera.ActualHeight / 2);
                Point newP = new Point(0.0f, 0.0f);
                double height = 0.0f;
                bool bHeightChange = true;
                if (Math.Abs(mouseDownP.X - centerP.X) > Math.Abs(mouseDownP.Y - centerP.Y))
                {
                    bHeightChange = false;
                }
                if (bHeightChange)  // regardless of height or width change, we are going to keep the reticle as a square, not a rectangle
                {
                    height = Math.Abs(centerP.Y - mouseDownP.Y) * 2 + 1;
                }
                else  // it's a width change
                {
                    height = Math.Abs(centerP.X - mouseDownP.X) * 2 + 1;
                }
                reticleCamera.Height = height;  // keep it square
                reticleCamera.Width = height;
                    // record the proportionate size of the reticle relative to the image control
                m_ratioSizeImageChange = (Single)(reticleCamera.ActualWidth / imgsrcCamera.ActualWidth);
            }
            else if ((m_markerBeingMoved == MovingMarker.FIDUCIAL1) && (e.LeftButton == MouseButtonState.Pressed))
            {
                Point mouseDownP = e.GetPosition(imgsrcCamera); // where's the mouse relative to upper left corner of image control
                Point centerP = new Point(imgsrcCamera.ActualWidth / 2, imgsrcCamera.ActualHeight / 2);   // design time was image center
                int XDirection = 1;
                int YDirection = 1;
                if (centerP.X > mouseDownP.X) XDirection = -1;  // moving fiducial upwards
                if (centerP.Y > mouseDownP.Y) YDirection = -1;  // moving fiducial left
                fid1Transform.X = XDirection * (Math.Abs(mouseDownP.X - centerP.X));    // in wpf we apply a transform to paint an ellipse
                fid1Transform.Y = YDirection * (Math.Abs(mouseDownP.Y - centerP.Y));    // in a new location. we do NOT redraw it.
                e.Handled = true;
            }
            else if ((m_markerBeingMoved == MovingMarker.FIDUCIAL2) && (e.LeftButton == MouseButtonState.Pressed))
            {
                Point mouseDownP = e.GetPosition(imgsrcCamera);
                Point centerP = new Point(imgsrcCamera.ActualWidth / 2, imgsrcCamera.ActualHeight / 2);
                int XDirection = 1;
                int YDirection = 1;
                if (centerP.X > mouseDownP.X) XDirection = -1;  // moving fiducial upwards
                if (centerP.Y > mouseDownP.Y) YDirection = -1;  // moving fiducial left
                fid2Transform.X = XDirection * (Math.Abs(mouseDownP.X - centerP.X));    // in wpf we apply a transform to paint an ellipse
                fid2Transform.Y = YDirection * (Math.Abs(mouseDownP.Y - centerP.Y));    // in a new location. we do NOT redraw it.
                e.Handled = true;
            }
//            Debug.WriteLine("fid1transform x / y : " + fid1Transform.X + " / " + fid1Transform.Y);
        }   //         private void imgsrcCamera_MouseMove(object sender, MouseEventArgs e)

        /// <summary>
        /// User is starting to move a fiducial marker to a new location.
        /// Or user is starting to resize the reticle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void markerCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
//            Debug.WriteLine("MouseDown x / y : " + e.GetPosition(imgsrcCamera));
            if (sender == fid1Camera)
            {
                m_markerBeingMoved = MovingMarker.FIDUCIAL1;
                Grid.SetZIndex(fid1Camera, Grid.GetZIndex(fid2Camera) + 1);
            }
            else if (sender == fid2Camera)
            {
                m_markerBeingMoved = MovingMarker.FIDUCIAL2;
                Grid.SetZIndex(fid2Camera, Grid.GetZIndex(fid1Camera) + 1);
            }
            else if (sender == reticleCamera)
            {
                m_markerBeingMoved = MovingMarker.RETICLE;
            }
        }   // private void markerCamera_MouseDown(object sender, MouseButtonEventArgs e)

        /// <summary>
        /// User finished moving a fiducial marker to a new position
        /// Or user finished resizing a reticle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void markerCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
//            Debug.WriteLine("MouseUp x / y : " + e.GetPosition(imgsrcCamera));
            if (sender == fid1Camera)
            {
                m_markerBeingMoved = MovingMarker.NONE;
                Grid.SetZIndex(fid1Camera, Grid.GetZIndex(fid2Camera) - 1);
            }
            else if (sender == fid2Camera)
            {
                m_markerBeingMoved = MovingMarker.NONE;
                Grid.SetZIndex(fid2Camera, Grid.GetZIndex(fid1Camera) - 1);
            }
            else if (sender == reticleCamera)
            {
                m_markerBeingMoved = MovingMarker.NONE;
            }
        }   // private void markerCamera_MouseUp(object sender, MouseButtonEventArgs e)

        /// <summary>
        /// event first fires on startup when the slider value is set to the max allowed.
        /// the control is disabled in xaml, but apparently is temporarily enabled by wpf
        /// on startup to allow setting of the value, and then disabled again. 
        /// on display, the control would still be disabled without the enabling action in the timer_Tick event.
        /// Verified that the timer_Tick event does fire after the slider_ValueChanged event, and not before.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sldrLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider control = (Slider)sender;
            if ((control == sldrCamWhiteLevel) && (control.Value <= sldrCamBlackLevel.Value))
            {
                control.Value = sldrCamBlackLevel.Value + 1;
            }
            else if ((control == sldrCamBlackLevel) && (control.Value >= sldrCamWhiteLevel.Value))
            {
                control.Value = sldrCamWhiteLevel.Value - 1;
            }
        }   // private void sldrLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)

        #endregion Windows event

        #region properties

        public string Id { get; set; }

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

        /// <summary>
        /// Strongly notify user of an unexpected occurrence
        /// </summary>
        /// <param name="msg"></param>
        private void m_imageControlViewModel_ExceptionRaised(string msg)
        {
            // viewModel will only throw catastrophic exceptions. It has the MsgFmViewModel dependency property for status and warning
                        MessageBoxResult result = MessageBoxResult.OK;
                        this.Dispatcher.BeginInvoke((Action)  // issues now sent to the app status bar
                            (
                                () =>
                                {
                                    result = MessageBox.Show( msg, "ViewModel Warning: ", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
                                    if (result == MessageBoxResult.Cancel)
                                    {
                                    }
                                }   // anonymous method lambda expression
                            ) // new Action ()
                            );  // BeginInvoke
        }// m_imgViewModel_ExceptionRaised (string)

        #endregion event sinks

    }   //     public partial class ImageControlView : UserControl, ICryoviewWindow, IDisposable

}   // namespace Image_Control_View