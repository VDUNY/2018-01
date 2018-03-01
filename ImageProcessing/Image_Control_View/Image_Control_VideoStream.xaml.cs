using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;   // TouchEventArgs, MouseEventArgs, etc
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media; // PixelFormats
using System.Windows.Media.Imaging; // BitmapSource

using ModuleMessages;
using Image_Control_ViewModel;

namespace ViewImages
{
    /// <summary>
    /// Interaction logic for VideoStream_View.xaml
    /// </summary>
    public partial class VideoStreamView : Window, IDisposable
    {

        #region delegates, events, threads and locking vars
        #endregion delegates, events, threads and locking vars

        #region backing vars

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint MF_ENABLED = 0x00000000;

        const uint SC_CLOSE = 0xF060;

        const int WM_SHOWWINDOW = 0x00000018;
        const int WM_CLOSE = 0x10;

        private MovingMarker m_markerBeingMoved = MovingMarker.NONE;
        private Single m_ratioSizeViewportChange = 0.0f;
        // User to calculate percent change in size of border for image on SizeChanged().
        // If init'd to (0.0, 0.0), leads to percent change = NaN because of DivBy0.
        private Size m_viewportSizePreResize = new Size(0, 0);   


        bool m_bFirstTimeSizeChanged = true;

        ImageControlViewModel m_imageViewModel = null;

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

        public VideoStreamView(ImageControlViewModel imageViewModel)
        {
            InitializeComponent();
            this.Title = imageViewModel.ControlCaption;
            m_imageViewModel = imageViewModel;
            this.DataContext = m_imageViewModel;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {       // managed resources here
                // the thread uses Dispatcher.Invoke() which causes a deadlock. 
                // I believe that the Invoke(NewColor) hangs because the window is closing and the Join()
                // below hangs because the thread can't complete. We are just going
                // to let the thread exit.
                //                m_videothrd43KWarning.Join();  
                // viewModel passed in via ctor; do not dispose here.
                this.Close();
            }
            // unmanaged resources here
            {
            }
        }

        #endregion ctors, dtor, dispose

        #region initialization
        #endregion initialization

        #region MEF
        #endregion MEF

        #region Windows events

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoStream_Loaded(object sender, RoutedEventArgs e)
        {       
            fid1Transform.X = 0;    // by applying a new location transform to the design time coordinates.
            fid1Transform.Y = 0;    // e.g. if we center an ellipse at 500, 500 on a 1000 x 1000 window, 
            fid2Transform.X = 0;    // then we apply a transform of (-10, -10) to draw the ellipse at (490, 490).
            fid2Transform.Y = 0;    // we do NOT draw the ellipse at (490, 490).
            Grid.SetZIndex(fid1Camera, (int)80);
            Grid.SetZIndex(fid2Camera, (int)80);

            // in cryoview, the img control has an actual size at this point. Here, the img render size is still (0,0).
            m_ratioSizeViewportChange = 1.0f;


        }   // private void VideoStream_Loaded(object sender, RoutedEventArgs e)

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }   // protected override void OnSourceInitialized(EventArgs e)

        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHOWWINDOW)
            {
                IntPtr hMenu = GetSystemMenu(hwnd, false);
                if (hMenu != IntPtr.Zero)
                {
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }
            }
            else if (msg == WM_CLOSE)
            {
//                handled = true;
            }
            return IntPtr.Zero;
        }   // IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn();
        }   // private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut();
        }   // private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)

        private void cmdZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }   // private void cmdZoomOut_Click(object sender, RoutedEventArgs e)

        private void cmdZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();

        }   // private void cmdZoomIn_Click(object sender, RoutedEventArgs e)   

        /// <summary>
        /// Zoom the viewport out by a small increment.
        /// </summary>
        private void ZoomOut()
        {
            controlZoomPan.ContentScale -= 0.1;
        }   // private void ZoomOut()

        /// <summary>
        /// Zoom the viewport in by a small increment.
        /// </summary>
        private void ZoomIn()
        {
            controlZoomPan.ContentScale += 0.1;
        }   // private void ZoomIn()

        private void cmdZoomIn_TouchDown(object sender, TouchEventArgs e)
        {
            ZoomIn();
        }   // private void cmdZoomIn_TouchDown(object sender, TouchEventArgs e)

        private void cmdZoomOut_TouchDown(object sender, TouchEventArgs e)
        {
            ZoomOut();
        }   // private void cmdZoomOut_TouchDown(object sender, TouchEventArgs e)

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Called the first time the user control is displayed, and then when the window size changes.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrlvwr_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_bFirstTimeSizeChanged)    // initial call to display the user control, perhaps a tab change?
            {       // regardless, this is the first opportunity to get the rendered size of the image control to store for later use.
                // Needed to effect transformation of the fid markes and resizing of the reticle
                m_bFirstTimeSizeChanged = false;
                m_viewportSizePreResize = controlZoomPan.RenderSize;
                // new reticle size = percent size of reticle relative to actual image size; 
                // keeps reticle proportional to image size
                reticleCamera.Width = m_ratioSizeViewportChange * controlZoomPan.ViewportWidth;
                reticleCamera.Height = m_ratioSizeViewportChange * controlZoomPan.ViewportHeight;
            }
            else
            {
                if ((controlZoomPan.ViewportWidth == 0.0) || (m_viewportSizePreResize.Width == 0.0))
                {
                }
                else
                {
                    // new reticle size = percent size of reticle relative to actual image size; 
                    // keeps reticle proportional to image size
                    reticleCamera.Width = m_ratioSizeViewportChange * controlZoomPan.ViewportWidth;
                    reticleCamera.Height = m_ratioSizeViewportChange * controlZoomPan.ViewportHeight;

                    // temp var to keep current size and not overwrite the previous control size
                    Size sizeViewport = new Size(controlZoomPan.ViewportWidth, controlZoomPan.ViewportHeight);
                    // used to get the new fiducial marker transforms
                    double imgHSizePercentChange = controlZoomPan.ViewportHeight / m_viewportSizePreResize.Height;
                    double imgWSizePercentChange = controlZoomPan.ViewportWidth / m_viewportSizePreResize.Width;
                    // now we overwrite the previous control size with the new control size for use on the next SizeChanged() event.
                    m_viewportSizePreResize = sizeViewport; // save for next change in user control size
//                    Debug.WriteLine(m_viewportSizePreResize.Height + " / " + m_viewportSizePreResize.Width);
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
        }   // private void scrlvwr_SizeChanged(object sender, SizeChangedEventArgs e)

        /// <summary>
        /// User is starting to move a fiducial marker to a new location.
        /// Or user is starting to resize the reticle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void markerCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
//            Debug.WriteLine("MouseDown x / y : " + e.GetPosition(imgLiveVideo));
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
//            Debug.WriteLine("MouseUp x / y : " + e.GetPosition(imgLiveVideo));
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
        /// User is moving mouse across the x-axis image. If the mouse button is pressed, resize the reticle or move a fiducial marker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgLiveVideo_MouseMove(object sender, MouseEventArgs e)
        {
            if ((m_markerBeingMoved == MovingMarker.RETICLE) && (e.LeftButton == MouseButtonState.Pressed))
            {
                Point mouseDownP = e.GetPosition(imgLiveVideo);
                Point centerP = new Point(dckpnlImg.ActualWidth / 2, dckpnlImg.ActualHeight / 2);
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
                m_ratioSizeViewportChange = (Single)(reticleCamera.ActualWidth / imgLiveVideo.ActualWidth);
            }
            else if ((m_markerBeingMoved == MovingMarker.FIDUCIAL1) && (e.LeftButton == MouseButtonState.Pressed))
            {
                Point mouseDownP = e.GetPosition(imgLiveVideo); // where's the mouse relative to upper left corner of image control
                Point centerP = new Point(dckpnlImg.ActualWidth / 2, dckpnlImg.ActualHeight / 2);   // design time was image center
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
                Point mouseDownP = e.GetPosition(controlZoomPan);

                Point centerP = new Point(controlZoomPan.ViewportHeight/ 2, controlZoomPan.ViewportHeight/ 2);
                fid2Transform.X = mouseDownP.X - centerP.X;    // in wpf we apply a transform to paint an ellipse
                fid2Transform.Y = mouseDownP.Y - centerP.Y;    // in a new location. we do NOT redraw it.
                e.Handled = true;
            }
            Debug.WriteLine("fid1transform x / y : " + fid1Transform.X + " / " + fid1Transform.Y);
        }   // private void imgLiveVideo_MouseMove(object sender, MouseEventArgs e)

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

        #endregion utility methods

    }   // public partial class VideoStreamView : Window, IDisposable
}   // namespace ViewImages
