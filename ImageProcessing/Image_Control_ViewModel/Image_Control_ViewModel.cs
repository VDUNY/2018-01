using System;
using System.ComponentModel;    // IDataErrorInfo
using System.Threading;
using System.IO;    // Directory, File
using System.Collections.Generic; // List
using System.Collections.Concurrent;    // ConcurrentDictionary<T, T>
using System.Windows;   // Visibility; Application.Current.Dispatcher - reqs refs to PresentationFramework, System.Xaml and WindowsBase
using System.Windows.Media;       // PixelFormats, Color
using System.Windows.Media.Imaging;     // BitmapPalette; reqs ref to WindowsBase, PresentationCore
using System.Windows.Input; // for ICommand and CommandManager; requires ref to PresentationCore
using System.Reflection;    // Assembly
using ModuleMessages;

using Prism.Commands;
using Prism.Mvvm;
using Image_Interface;

namespace Image_Control_ViewModel
{
    /// <summary>
    ///  Every reqs mtg discussed 12-bit ccds. The code therefore does not make attempts to deal with either 8-bit or 
    ///  16-bit ccds.
    /// </summary>
    public partial class ImageControlViewModel: BindableBase, IDataErrorInfo, IDisposable
    {

        #region delegates, events

        public delegate void DelExceptionRaised(string msg);    // something went wrong
        public event DelExceptionRaised ExceptionRaised;

        #endregion delegates, events

        #region backing vars

        Object m_objLock = new object();
        string m_id = "";
        bool m_bContinuousAcquisition = false;  // needed to ContinuousAcq button to know whether to start or stop acq.
        const int MAX_PIXEL_VALUE = 4095;
        int m_imgDefaultWidth = 20; // pixels
        int m_imgDefaultHeight = 20;
        int m_maxPixelValue = MAX_PIXEL_VALUE;

        Thread m_thrdSingleImage = null;
        Thread m_thrdFilmstrip = null;

        dynamic m_camera = null;
        string m_phi = "";

        WriteableBitmap m_bmp = null;

        private bool m_bMapSetForNoScaling = false;   // shortcuts around resetting the 12-bit gray scale map.

        ConcurrentDictionary<string, string> m_appProperties = null;

        private short m_camBlackLevel = 0;
        private short m_camWhiteLevel = MAX_PIXEL_VALUE;
        private short[] map12BitGrayScale = new short[MAX_PIXEL_VALUE+1];   // holds the mapping values to convert 12-bit data to 8-bit representation for the monitor.

        bool m_setFiducial1Visbility = false;
        Visibility m_fiducial1Visibility = Visibility.Hidden;
        bool m_setFiducial2Visbility = false;
        Visibility m_fiducial2Visibility = Visibility.Hidden;
        bool m_setReticleVisibility = false;
        Visibility m_reticleVisibility = Visibility.Hidden;

        private ScalingChoice m_scaling = ScalingChoice.AUTO_SCALING;

        List<BitmapPalette> m_listPalettes = null;
        List<string> m_listPaletteNames = null;
        private String m_paletteSelected = "Default";

        private string m_controlCaption = " Image Display ";
        private bool m_filmstripCmdRunning = false;

        #endregion backing vars

        #region enums

        /// <summary>
        /// User gets a choice of how to scale display images.
        /// </summary>
        public enum ScalingChoice
        {
            AUTO_SCALING,
            EXPANDED_SCALING,
            NO_SCALING,
            STATIC_SCALING
        }

        #endregion enums

        #region ctors, dtor, dispose

        public ImageControlViewModel() { }

        public ImageControlViewModel(string id)
        {
            m_id = id;
            Phi = " 12.4 ";

            m_appProperties = new ConcurrentDictionary<string, string>();
            SingleImageCommand = new DelegateCommand<object>(param => this.ThreadSingleImage(), param => this.CanSingleImage);
            FilmstripCommand = new DelegateCommand<object>(param => this.ThreadFilmstrip(), param => true );
            LoadFractalCommand = new DelegateCommand<object>(param => this.LoadFractal(), param => true);
            LoadJupiterCommand = new DelegateCommand<object>(param => this.LoadJupiter(), param => true);
            LoadTargetCommand = new DelegateCommand<object>(param => this.LoadTarget(), param => true);
            /* we need a startup img of something else, the image control in the view has size (0,0). */
            WriteableBitmap bmp = new WriteableBitmap(500, 500, 96, 96, PixelFormats.Gray8, null);
            m_bmp = bmp;
            OnPropertyChanged("ImageSource");
        }   // public ImageControlViewModel(string id)

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
                m_bContinuousAcquisition = false;   // if continuous acq, stop it.
                try
                {
                    if (m_thrdSingleImage != null)    // maybe never acq'd an image
                        m_thrdSingleImage.Join();
                }
                catch (ThreadStateException) { };    // no big deal. Perhaps program shut down w/o any img acq.
                try
                {
                    if (m_thrdFilmstrip != null)  // maybe never acq'd an image
                        m_thrdFilmstrip.Join();
                }
                catch (ThreadStateException) { };    // no big deal. Perhaps program shut down w/o any img acq.
                if (m_camera != null)
                {
                    Thread.Sleep(2000); // just give the model some time to finish up whatever ... 
                    m_camera.Dispose();
                }
                m_appProperties = null;
            }
            // unmanaged resources here
            {
            }
        } // protected virtual void Dispose(bool disposing)

        #endregion ctors, dtor, dispose

        #region factory method
        #endregion factory method

        #region initialization

        /// <summary>
        /// Handle all network and hardware communication required for initialization
        /// config settings are need to init the camera. Must succeed.
        /// </summary>
        /// <param name="properties">application properties. we are going to add values from db configuration.</param>
        public void Initialize(object state)
        {
            ConcurrentDictionary<string, string> properties = (ConcurrentDictionary<string, string>)state;

            // generate an image model for each camera. 


            m_appProperties = properties;

            SetFiducial1Visibility = true;
            SetFiducial2Visibility = true;
            SetReticleVisibility = true;
            LoadPalettes();
            AdjustMapForNoScale(ref map12BitGrayScale);
            m_bMapSetForNoScaling = true;

            WriteableBitmap bmp = new WriteableBitmap(m_imgDefaultWidth, m_imgDefaultHeight, 96, 96, PixelFormats.Rgb24, m_listPalettes[0]);
            System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
            // msdn says width is stride in pixels, but that does not seem to be true.
            // Int32Rect, System.Array, stride in pixels, input buffer offset
            int pixel_stride = 3;
            Byte[] paletteImage = new byte[bmp.PixelHeight * bmp.PixelWidth];
            Byte[] palettizedImage = new byte[bmp.PixelHeight * bmp.PixelWidth * pixel_stride];
            Convert8BitGrayTo24BitRGB(paletteImage, m_imgDefaultWidth, m_imgDefaultHeight, m_listPalettes, 0, pixel_stride, ref palettizedImage);
            bmp.WritePixels(rect, palettizedImage, bmp.PixelWidth * pixel_stride, 0);
            bmp.Freeze();   // Freeze allows us to update the UI while not being on the UI thread.
            m_bmp = bmp;
            OnPropertyChanged("ImageSource");

        } // public void Initialize()

        #endregion initialization

        public short BlackLevel { get; set; }
        public short WhiteLevel { get; set; }

        public string CameraSerialNu { get; set; }
        public string CameraType { get; set; }

        #region IDataErrorInfo

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get 
            {
                string msg = "";
                switch (columnName)
                {
                    case "CamBlackLevel":
                        if (m_camBlackLevel < 0 || m_camBlackLevel > m_camWhiteLevel)
                        {
                            msg = "X Cam Black level must be between 0 and the white level.";
                        }
                        break;
                    case "CamWhiteLevel":
                        if (m_camWhiteLevel > m_maxPixelValue || m_camWhiteLevel < m_camBlackLevel)
                        {
                            msg = "XCam White level must be between the black level and " + m_maxPixelValue + ".";
                        }
                        break;
                }
                return msg;
            }
        }

        #endregion IDataErrorInfo

        #region MEF
        #endregion MEF

        #region windows events
        #endregion windows events

        #region properties
        #endregion properties

        #region bindable properties

        /// <summary>
        /// 
        /// </summary>
        public string ControlCaption
        {
            get { return m_controlCaption; }
            set { SetProperty<string>(ref m_controlCaption, value); OnPropertyChanged(() => ControlCaption); }
        }

        /// <summary>
        /// all the palettes
        /// </summary>
        public List<String> CamPalettes
        {
            get { return m_listPaletteNames; }
        }

        /// <summary>
        /// active palette
        /// </summary>
        public String PaletteSelected
        {
            get { return m_paletteSelected; }
            set { m_paletteSelected = value; }
        }

        public string Phi
        {
            get { return m_phi; }
            set
            {
                SetProperty<string>(ref m_phi, value); OnPropertyChanged(() => Phi);
            }
        }

        /// <summary>
        /// scaling mode selected by user
        /// </summary>
        public ScalingChoice Scaling
        {
            get { return m_scaling; }
            set { SetProperty<ScalingChoice>(ref m_scaling, value); OnPropertyChanged(() => Scaling); }
        }

        /// <summary>
        /// Value below which all pixels show as black
        /// </summary>
        public short CamBlackLevel
        {
            get { return m_camBlackLevel; }
            set { SetProperty<short>(ref m_camBlackLevel, value); OnPropertyChanged(() => CamBlackLevel); }
        }

        /// <summary>
        /// Value above which all pixels show as white
        /// </summary>
        public short CamWhiteLevel
        {
            get { return m_camWhiteLevel; }
            set { SetProperty<short>(ref m_camWhiteLevel, value); OnPropertyChanged(() => CamWhiteLevel); }
        }

        public bool FilmstripCmdRunning
        {
            get { return m_filmstripCmdRunning; }
            set { SetProperty<bool>(ref m_filmstripCmdRunning, value); OnPropertyChanged(() => FilmstripCmdRunning); }
        }

        /// <summary>
        /// Image from the camera
        /// </summary>
        public BitmapSource ImageSource
        {
            get { return m_bmp; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetFiducial1Visibility
        {
            get { return m_setFiducial1Visbility; }
            set
            {
                SetProperty<bool>(ref m_setFiducial1Visbility, value);
                if (m_setFiducial1Visbility) Fiducial1Visibility = Visibility.Visible;
                else Fiducial1Visibility = Visibility.Hidden;
                OnPropertyChanged(() => SetFiducial1Visibility);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Visibility Fiducial1Visibility
        {
            get { return m_fiducial1Visibility; }
            set { SetProperty<Visibility>(ref m_fiducial1Visibility, value); OnPropertyChanged(() => Fiducial1Visibility); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetFiducial2Visibility
        {
            get { return m_setFiducial2Visbility; }
            set
            {
                SetProperty<bool>(ref m_setFiducial2Visbility, value);
                if (m_setFiducial2Visbility) Fiducial2Visibility = Visibility.Visible;
                else Fiducial2Visibility = Visibility.Hidden;
                OnPropertyChanged(() => SetFiducial2Visibility);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Visibility Fiducial2Visibility
        {
            get { return m_fiducial2Visibility; }
            set { SetProperty<Visibility>(ref m_fiducial2Visibility, value); OnPropertyChanged(() => Fiducial2Visibility); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetReticleVisibility
        {
            get { return m_setReticleVisibility; }
            set
            {
                SetProperty<bool>(ref m_setReticleVisibility, value);
                if (m_setReticleVisibility) ReticleVisibility = Visibility.Visible;
                else ReticleVisibility = Visibility.Hidden;
                OnPropertyChanged(() => SetReticleVisibility);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Visibility ReticleVisibility
        {
            get { return m_reticleVisibility; }
            set { SetProperty<Visibility>(ref m_reticleVisibility, value); OnPropertyChanged(() => ReticleVisibility); }
        }
        #endregion bindable properties

        #region dependency properties
        #endregion dependency properties

        #region ICommands

        /// <summary>
        /// 
        /// </summary>
        public ICommand LoadFractalCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand LoadJupiterCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand LoadTargetCommand { get; private set; }

        /// <summary>
        /// Get one image data from the pleora box.
        /// </summary>
        public ICommand SingleImageCommand { get; private set; }

        /// <summary>
        /// Either enable or disable the UI button based on whether continuous acq is running
        /// button takes one image only. no need for start/stop functionality.
        /// </summary>
        bool CanSingleImage { get { return (!m_bContinuousAcquisition); } }

        /// <summary>
        /// Repeatedly get image data from the pleora box.
        /// </summary>
        public ICommand FilmstripCommand { get; private set; }

        #endregion ICommands

        #region algorithm code
        #endregion algorithm code

        #region hardware code

        private void LoadFractal()
        {
            if (m_camera != null) m_camera.Dispose();
            Assembly dynamicAssembly = Assembly.LoadFrom("Image_Fractal_Model.dll");
            Type type = dynamicAssembly.GetType("Image_Fractal_Model.ImageFractalModel");
            m_camera = Activator.CreateInstance(type);
            m_camera.ImgData.CameraType = "Imperx";
            m_camera.ImgData.CameraSerialNu = "12345";
            CameraSerialNu = "12345";
            ControlCaption = m_id + " " + " Digital";
            m_camera.ImgData.Id = m_id;
            ((ImageInterface)m_camera).ExceptionRaised += m_camera_ExceptionRaised;
            try
            {
                m_camera.Initialize(m_id);
            }
            catch (Exception ex)
            {
                HandleExceptions(new Exception(" Missing Img Acq Camera" + Environment.NewLine + ex.ToString()));
            }
            Thread thrdStartAcquiring = new Thread(new ThreadStart(AcquireOnProgramStartup));
            //            Thread.Sleep(3000);
            FilmstripCmdRunning = true;
            thrdStartAcquiring.Start();

        }

        private void LoadJupiter()
        {
            if (m_camera != null) m_camera.Dispose();
            Assembly dynamicAssembly = Assembly.LoadFrom("Image_Jupiter_Model.dll");
            Type type = dynamicAssembly.GetType("Image_Jupiter_Model.ImageJupiterModel");
            m_camera = Activator.CreateInstance(type);
            m_camera.ImgData.CameraType = "Imperx";
            m_camera.ImgData.CameraSerialNu = "12345";
            CameraSerialNu = "12345";
            ControlCaption = m_id + " " + " Digital";
            m_camera.ImgData.Id = m_id;
            ((ImageInterface)m_camera).ExceptionRaised += m_camera_ExceptionRaised;
            try
            {
                m_camera.Initialize(m_id);
            }
            catch (Exception ex)
            {
                HandleExceptions(new Exception(" Missing Img Acq Camera" + Environment.NewLine + ex.ToString()));
            }
            Thread thrdStartAcquiring = new Thread(new ThreadStart(AcquireOnProgramStartup));
            //            Thread.Sleep(3000);
            FilmstripCmdRunning = true;
            thrdStartAcquiring.Start();

        }

        private void LoadTarget()
        {
            if (m_camera != null) m_camera.Dispose();
            Assembly dynamicAssembly = Assembly.LoadFrom("Image_Virtual_Model.dll");
            Type type = dynamicAssembly.GetType("Image_Virtual_Model.ImageVirtualModel");
            m_camera = Activator.CreateInstance(type);
            m_camera.ImgData.CameraType = "Imperx";
            m_camera.ImgData.CameraSerialNu = "12345";
            CameraSerialNu = "12345";
            ControlCaption = m_id + " " + " Digital";
            m_camera.ImgData.Id = m_id;
            ((ImageInterface)m_camera).ExceptionRaised += m_camera_ExceptionRaised;
            try
            {
                m_camera.Initialize(m_id);
            }
            catch (Exception ex)
            {
                HandleExceptions(new Exception(" Missing Img Acq Camera" + Environment.NewLine + ex.ToString()));
            }
            Thread thrdStartAcquiring = new Thread(new ThreadStart(AcquireOnProgramStartup));
            //            Thread.Sleep(3000);
            FilmstripCmdRunning = true;
            thrdStartAcquiring.Start();

        }

        /// <summary>
        /// 
        /// </summary>
        private void AcquireOnProgramStartup()
        {
            Filmstrip(null);
        }

        /// <summary>
        /// allows user interface to proceed w/o blocking
        /// </summary>
        public void ThreadSingleImage()
        {
            m_thrdSingleImage = new Thread(new ParameterizedThreadStart(SingleImage));
            m_thrdSingleImage.Name = "m_thrdSingleImage";
            m_thrdSingleImage.Start();
        }   // ThreadSingleImage

        /// <summary>
        /// acquire one image only
        /// </summary>
        /// <param name="state"></param>
        public void SingleImage(object state)
        {
            lock (m_objLock)
            {
                CameraImageDataInterface img = null;
                if (m_camera != null)
                {
                    img = m_camera.AcquireTheImage();
                }
                if (img == null)
                {
                    System.Diagnostics.Debug.WriteLine(" Null img acquired. ");
                    WriteableBitmap bmp = new WriteableBitmap(m_imgDefaultWidth, m_imgDefaultHeight, 96.0, 96.0, PixelFormats.Gray8, null);
                    System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, m_imgDefaultWidth, m_imgDefaultHeight);
                    Array ary = Array.CreateInstance(typeof(byte), m_imgDefaultWidth * m_imgDefaultHeight);
                    bmp.WritePixels(rect, ary, m_imgDefaultWidth, 0);
                    bmp.Freeze();   // Freeze allows us to update the UI while not being on the UI thread.
                    m_bmp = bmp;
                    OnPropertyChanged("ImageSource");
                }
                else if (img.Image == null)
                {
                    WriteableBitmap bmp = new WriteableBitmap(m_imgDefaultWidth, m_imgDefaultHeight, 96.0, 96.0, PixelFormats.Gray8, null);
                    System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, m_imgDefaultWidth, m_imgDefaultHeight);
                    Array ary = Array.CreateInstance(typeof(byte), m_imgDefaultWidth * m_imgDefaultHeight);
                    bmp.WritePixels(rect, ary, m_imgDefaultWidth, 0);
                    bmp.Freeze();   // Freeze allows us to update the UI while not being on the UI thread.
                    m_bmp = bmp;
                }
                else if (img.Image.GetType() == typeof(ushort[]))
                {
                    m_bmp = AdjustImageForDisplay(img.Image, img.Width, img.Height, img.Width * sizeof(ushort));
                    OnPropertyChanged("ImageSource");
                }
                else if (img.Image.GetType() == typeof(byte[]))
                {
                    m_bmp = AdjustImageForDisplay(img.Image, img.Width, img.Height, img.Width * sizeof(byte));
                    OnPropertyChanged("ImageSource");
                }
            }   // lock (m_objLock)
        }   // SingleImage

        /// <summary>
        /// allow user interface to continue without blocking
        /// </summary>
        public void ThreadFilmstrip()
        {
            m_thrdFilmstrip = new Thread(new ParameterizedThreadStart(Filmstrip));
            m_thrdFilmstrip.Name = "thrdRunFilmstrip";
            FilmstripCmdRunning = true;
            m_thrdFilmstrip.Start();
        }   // ThreadRunFilmstrip

        /// <summary>
        /// When user clicks filmstrip, the camera (externally triggered) is repeatedly queried by pleora box to return an image.
        /// </summary>
        public void Filmstrip(object state)
        {
            m_bContinuousAcquisition = !m_bContinuousAcquisition; // must be outside of lock else can't change it.
            Application.Current.Dispatcher.BeginInvoke(
                (Action)( () =>
                    { ((DelegateCommand<object>)SingleImageCommand).RaiseCanExecuteChanged(); }
                )
                );

            while (m_bContinuousAcquisition)
            {
                SingleImage(null);
                //Thread.Sleep(1000);
            }
            Application.Current.Dispatcher.Invoke(
                (Action)(() =>
                { FilmstripCmdRunning = false; }
                )
                );
            
        }   // RunFilmstrip ( ... )

        #endregion hardware code


        #region utility methods

        /// <summary>
        /// Autoscaling recodes the pixel values based on the image data.
        /// Static scaling recodes the pixel values based on the white/black slider controls.
        /// Expanded scaling recodes the pixel values based on 0 = min pixel value and 4095 = max pixel value.
        /// </summary>
        /// <param name="image">Raw data from camera. This is copied to memory. The raw data is not modified. The copy is modified.</param>
        /// <returns></returns>
        private WriteableBitmap AdjustImageForDisplay(Array image, int width, int height, int stride)
        {
            lock (m_objLock)
            {
                short minVal = 0; short maxVal = 0;

                ushort[] workingImage = new ushort[width * height];    // will hold the bit-shifted pixel values
                if (image.GetType() == typeof(UInt16[]))
                {
                    Array.Copy(image, workingImage, width * height); // need to bitshift later to display on monitor
                }
                else if (image.GetType() == typeof(Int16[]))
                {
                    Array.Copy(image, workingImage, width * height); // need to bitshift later to display on monitor
                                                                     // negative numbers; background subtraction???
                    for (int i = 0; i < workingImage.Length; i++) { if (workingImage[i] < 0) workingImage[i] = 0; }
                }
                else if (image.GetType() == typeof(Byte[]))
                {
                    Array.Copy(image, workingImage, width * height); // need to bitshift later to display on monitor
                    for (int i = 0; i < width * height; i++)   // calculations are done; create the modified display
                    {
                        workingImage[i] = (ushort)(workingImage[i] << 4);  // byte data, we process all data as 12-bit
                    }
                }
                ushort[] adjustedImage = new ushort[width * height];    // will hold the bit-shifted pixel values
                Array.Copy(workingImage, adjustedImage, width * height);
                byte[] shifted8BitImage = new byte[width * height];
                if (!m_bMapSetForNoScaling && (Scaling == ScalingChoice.NO_SCALING))
                {
                    m_bMapSetForNoScaling = true;
                    AdjustMapForNoScale(ref map12BitGrayScale);
                }
                else if (Scaling == ScalingChoice.AUTO_SCALING)
                { // Create the grey scale based on the pixel values in the image.
                    FindMinMaxPixelVals(adjustedImage, width, height, ref minVal, ref maxVal);   // get the lowest and highest pixel value to determine scaling factor
                    m_bMapSetForNoScaling = false;
                    AdjustMapForAutoscale(ref map12BitGrayScale, ref minVal, ref maxVal);  // repopulate the scaling map based on the new min/max values
                    if (minVal != m_camBlackLevel) { CamBlackLevel = minVal; }
                    if (maxVal != m_camWhiteLevel) { CamWhiteLevel = maxVal; }
                }  // autoscale overrides blk/wht slider manipulation
                else if (Scaling == ScalingChoice.STATIC_SCALING) // Create the grey scale map based on the values of the black/white slider controls.
                {       // any pixel less than the chosen black level is black. Any pixel greater than the chosen white level is white.
                    m_bMapSetForNoScaling = false;
                    AdjustMapForStaticBlkWhtLevels(ref map12BitGrayScale, m_camBlackLevel, m_camWhiteLevel);
                }
                else if (Scaling == ScalingChoice.EXPANDED_SCALING)   // Create grey scale map such that min pixel value = 0 and max pixel value = 4095.
                {
                    m_bMapSetForNoScaling = false;
                    AdjustMapForAutoscale(ref map12BitGrayScale, ref m_camBlackLevel, ref m_camWhiteLevel);
                }
                CreateModifiedDisplay(adjustedImage, map12BitGrayScale, width, height, ref shifted8BitImage);    // apply scaling and return 8-bit pixels for display via pallette

                Byte[] palettizedImage = new byte[width * height * 3];
                WriteableBitmap bmp = null;
                int paletteIndex = m_listPaletteNames.FindIndex(FindPalette);  // get the current palette out of the palette array
                Convert8BitGrayTo24BitRGB(shifted8BitImage, width, height, m_listPalettes, paletteIndex, 3, ref palettizedImage);
                //  the width/stride is 1000, but in bytes it is width * pixel_stride bytes. 
                //  On 64-bit build, raises ArugmentException crossed a native/managed boundary and crashes.
                //  On 32-bit build, raises ArgumentException value does not fall in expected range.
                // whether the camera produces 8-bit, or 12-bit pixel format, we will put 16-bit into the bmp
                bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, m_listPalettes[paletteIndex]);
                System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
                // msdn says width is stride in pixels, but that does not seem to be true.
                // Int32Rect, System.Array, stride in pixels, input buffer offset
                bmp.WritePixels(rect, palettizedImage, width * 3, 0);
                bmp.Freeze();   // Freeze allows us to update the UI while not being on the UI thread.
                return bmp;
            }   //         private WriteableBitmap AdjustImageForDisplay(Array image, int width, int height, int stride)
        }

        /// <summary>
        /// When there is no scaling invovled, each pixel value is left unchanged thru the map.
        /// </summary>
        /// <param name="map12BitGrayScale"></param>
        private void AdjustMapForNoScale(ref short[] map12BitGrayScale)
        {
            for (int i = 0; i < m_maxPixelValue; i++) map12BitGrayScale[i] = (short)i;
        }   // AdjustMapForNoScale

        /// <summary>
        /// The color map varies as the min/max pixel values vary from img to img.
        /// Update the color map.
        /// </summary>
        /// <param name="map12BitGrayScale"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        private void AdjustMapForAutoscale(ref short[] map12BitGrayScale, ref short minVal, ref short maxVal)
        {
            if ((minVal == maxVal) && (minVal == 0))    // guard against perfectly black image
            {
                maxVal = 1;
            }
            else if ((minVal == maxVal) && (maxVal == m_maxPixelValue))    // guard against perfectly saturated image
            {
                minVal = (short)(m_maxPixelValue - 1);
            }
            else if (minVal == maxVal)
            {
                // must guard against div by 0. Something is wrong with camera data.
                maxVal++;
            }
            for (int i = 0; i < m_maxPixelValue + 1; i++)
            {       // these lines are wrong for autoscale
                if (i < minVal) { map12BitGrayScale[i] = 0; }
                else if (i > maxVal) { map12BitGrayScale[i] = (short)m_maxPixelValue; }
                else
                {
                    int numerator = (m_maxPixelValue * (i - minVal));
                    int denominator = maxVal - minVal;
                    short val = (short)(numerator / denominator);
                    map12BitGrayScale[i] = val;
                }

            }
        }   // AdjustMapForAutoscale

        /// <summary>
        /// returns a gray scale map where any pixel value under black level is black 
        /// and any pixel value greater than white level is white.
        /// E.g. black = 100, white = 4000
        /// map = { 0, 0, ... 0, 100, 101, 102, ... 3998, 3999, 4000, 4095, 4095, ... 4095 }
        /// </summary>
        /// <param name="map12BitGrayScale"></param>
        /// <param name="blackLevel"></param>
        /// <param name="whiteLevel"></param>
        private void AdjustMapForStaticBlkWhtLevels(ref short[] map12BitGrayScale, short blackLevel, short whiteLevel)
        {
            for (int i = 0; i < m_maxPixelValue + 1; i++)
            {
                if (i < blackLevel) { map12BitGrayScale[i] = 0; }
                else if (i > whiteLevel) { map12BitGrayScale[i] = (short)m_maxPixelValue; }
                else { map12BitGrayScale[i] = (short)i; }; // do nothing
            }
        }   // void AdjustMapForStaticBlkWhtLevels(ref short[] map12BitGrayScale, short blackLevel, short whiteLevel)

        /// <summary>
        /// Just prior to display, take the massaged from 12-bit to 8-bit pixel data and convert it to 24 bit rgb so we can use colorized pallettes.
        /// </summary>
        /// <param name="paletteImage">The pixels which will be transformed from grey-scale to a colorized pallette.</param>
        /// <param name="m_listPalettes">Array of all the pallettes.</param>
        /// <param name="paletteIndex">The complete list of pallette values that define the pallette. Each index is the definition for how to define a pixel value.</param>
        /// <param name="pixel_stride"> Offset from one pixel's data to the next pixel's data.</param>
        /// <param name="palettizedImage">Data manipulated to show in a colorized pallette.</param>
        private void Convert8BitGrayTo24BitRGB(byte[] paletteImage, int width, int height, List<BitmapPalette> m_listPalettes,
                    int paletteIndex, int pixel_stride, ref byte[] palettizedImage)
        {
            int index = 0;
            for (int i = 0; i < width * height ; i++)
            {       // We are going from 8-bit gray scale to 24-bit rgb. 'i' is 8-bit location. 'index' is the 24-bit location
                index = i * pixel_stride;
                int val = paletteImage[i];
                palettizedImage[index] = m_listPalettes[paletteIndex].Colors[val].R;
                palettizedImage[index + 1] = m_listPalettes[paletteIndex].Colors[val].G;
                palettizedImage[index + 2] = m_listPalettes[paletteIndex].Colors[val].B;
            }
        }   // Convert8BitGrayTo24BitRGB ( ... )

        /// <summary>
        /// The image data is 16-bit. The flatpanel monitor can only display 8-bit data. To minimize data loss, shift pixel data right
        /// by 4-bits. Then, e.g. 00001101 00111101 becomes 0000000 11010011 (3389 -> 211 ) (0x0D3D -> 0xD3).
        /// </summary>
        /// <param name="manipImage">12-bit image that needs display on a monitor</param>
        /// <param name="map12BitGrayScale">Adjusted for auto-scaling, static scaling or expanded scaling.</param>
        /// <param name="paletteImage">This bytes in this array will be used as indices into the color pallette.</param>
        private void CreateModifiedDisplay(ushort[] manipImage, short[] map12BitGrayScale, int width, int height, ref byte[] rightShiftedImage)
        {
            
            for (int i = 0; i < width * height; i++)   // calculations are done; create the modified display
            {
                ushort pixelVal = (ushort)(manipImage[i]);    // this pixel value needs to be scaled
                short scaledVal = map12BitGrayScale[pixelVal];  // compensating for auto-scaling or static scaling or expanded scaling
                rightShiftedImage[i] = (byte)(scaledVal >> 4);  // have now right shifted a 12-bit pixel format into 8-bits
            }
        }   // CreateModifiedDisplay ( ... )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        private void FindMinMaxPixelVals(ushort[] image, int horizontalSize, int verticalSize, ref short minVal, ref short maxVal)
        {
            minVal = (short)m_maxPixelValue; maxVal = 0; short pixelVal = 0;
            for (int i = 0; i < horizontalSize * verticalSize; i++)
            {
                pixelVal = (short)(image[i]);
                if (pixelVal < minVal) { minVal = pixelVal; }
                if (pixelVal > maxVal) { maxVal = pixelVal; }
            }
        }   // FindMinMaxPixelVals

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aName"></param>
        /// <returns></returns>
        private bool FindPalette(string paletteName) { if (paletteName.Equals(PaletteSelected)) return true; else return false; }

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
        /// Find the palettes in the file system and bring them into the app for use while displaying images.
        /// </summary>
        private void LoadPalettes()
        {
            m_listPalettes = new List<BitmapPalette>();
            m_listPaletteNames = new List<string>();
            string palettesPath = Environment.CurrentDirectory + "\\palettes";  // must be a sub-dir under the install dir
            try
            {
                string[] paletteNames = Directory.GetFiles(palettesPath, "*.csv");  // get all file that are comma separated values
                foreach (string name in paletteNames)   // one file at a time
                {
                    BitmapPalette pal = null;
                    string name2 = System.IO.Path.GetFileNameWithoutExtension(name);
                    m_listPaletteNames.Add(name2);  // for the dropdown box
                    var reader = new StreamReader(File.OpenRead(name));
                    List<Color> colors = new List<Color>();
                    reader.ReadLine();   // first line of each pallette file is the column headers.
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        Color color = new Color();
                        color.R = Convert.ToByte(values[0]);
                        color.B = Convert.ToByte(values[1]);
                        color.G = Convert.ToByte(values[2]);
                        colors.Add(color);
                    }
                    pal = new BitmapPalette(colors);
                    if (pal != null) m_listPalettes.Add(pal);   // add to the dropdown box
                }
            }
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
        }   // private void LoadPalettes()

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
        /// something unexpected occurred.
        /// </summary>
        /// <param name="msg"></param>
        private void m_camera_ExceptionRaised(string msg)
        {
            //            m_bContinuousAcquisition = false;   // stop if trying to continuously acquire, can't do this because algorithms expect new images
            StatusMessageEvent.Instance.Publish (new StatusMsg() { Msg = msg } );
            HandleExceptions(new Exception(msg));
        }   // void m_image_ExceptionRaised(string msg)

        #endregion event sinks

    }    // public class ImageControlViewModel: BindableBase, IDataErrorInfo, IDisposable

}   // namespace Image_Control_ViewModel
