using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;  // CallerFilePath, etc
using System.Runtime.InteropServices;
using Image_Interface;
using LLE.HDF4;
using LLE.Util;

namespace Image_Fractal_Model
{
    public class ImageFractalModel : ImageInterface, IDisposable
    {
        #region delegates, events

        public event DelExceptionRaised ExceptionRaised;

        #endregion delegates, events

        #region backing vars

        // simulation support
        List<CameraImageData> lstImages = new List<CameraImageData>();
        int lstPtr = 0;

        #endregion backing vars

        #region enums

        #endregion enums

        #region ctors, dtor, dispose

        /// <summary>
        /// 
		/// </summary>
		public ImageFractalModel()
        {
            ImgData = new CameraImageData();
        }   // private ImageVirtualModel()

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Got here either by user code or by garbage collector. If param false, then gc.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)  // managed resources here 
            {
                ImgData = null;
                lstImages.Clear();
                lstImages = null;
            }
            { } // unmanaged resources here
        }    // protected virtual void Dispose(bool disposing)

        #endregion ctors, dtor, dispose

        #region initialization

        public bool Initialize(string id)
        {
            bool bRetVal = true;
            // simulation only; to load simulation data
            try
            {
                Bitmap bmp = (Bitmap)Image.FromFile("Fractal.bmp", false);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat );

                byte[] bytes = new byte[bitmapData.Stride * bmp.Height];
                Marshal.Copy(bitmapData.Scan0, bytes, 0, bitmapData.Stride * bmp.Height );
                bmp.UnlockBits(bitmapData);
                ushort[] uPixels = new ushort[bitmapData.Stride * bmp.Height];
                for (int i = 0; i < bitmapData.Stride * bmp.Height; i++) { uPixels[i] = (ushort)(bytes[i] << 4); }

                CameraImageData data = new CameraImageData();
                data.Width = bitmapData.Stride;
                data.Height = bmp.Height;
                data.HorizontalBinning = 1;
                data.VerticalBinning = 1;
                data.Gain = 1.0f;
                data.BitsPerPixel = 12;
                data.Stride = bmp.Width * 2;
                data.Image = uPixels;
                lstImages.Add(data);
            }   // try
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
            // end simulation only; to load simulation data
            return bRetVal;
        }

        #endregion initialization

        #region properties

        #endregion properties

        public CameraImageDataInterface ImgData { get; set; }

        #region algorithm code

        #endregion algorithm code

        #region hardware code

        /// <summary>
        /// Have the pleora box write out the pixels from the box to memory
        /// </summary>
        public CameraImageDataInterface AcquireTheImage()
        {
            // msdn says width is stride in pixels, but that does not seem to be true.
            // Int32Rect, System.Array, stride in pixels, input buffer offset
            // simulates the camera 
            CameraImageDataInterface tempData = null;
            try
            {
                System.Threading.Thread.Sleep(100); // simulate .1Hz trigger
                lstPtr++;
                if (lstPtr > lstImages.Count - 1) lstPtr = 0;

                tempData = new CameraImageData()
                {
                    Image = lstImages[lstPtr].Image,
                    Height = lstImages[lstPtr].Height,
                    Width = lstImages[lstPtr].Width,
                    HorizontalBinning = 1,
                    VerticalBinning = 1,
                    Gain = 1.0f,
                    BitsPerPixel = lstImages[lstPtr].BitsPerPixel,
                    Stride = lstImages[lstPtr].Stride,
                    CameraType = ImgData.CameraType,
                    CameraSerialNu = ImgData.CameraSerialNu,
                    Id = ImgData.Id,
                    Timestamp = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.ff tt")
                };
                ImgData = tempData;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
            finally
            { }
            return tempData;
        }   // AcquireTheImage()

        #endregion hardware code

        #region utility methods

        /// <summary>
        /// Aspect-oriented programming
        /// Break down program logic into distinct parts, aka concerns, one of which is handling exceptions.
        /// </summary>
        /// <param name="ex"></param>
        private void HandleExceptions(Exception ex,
                        [CallerFilePath] string sourceFile = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string trace = sourceFile.Substring(sourceFile.LastIndexOf('\\') + 1) + "(" + lineNumber + "): " + memberName + " --> ";
            if (ex.GetType() == typeof(InvalidCastException))
            {
                OnExceptionRaised(trace + " " + ex.ToString());
            }
            else if (ex.GetType() == typeof(IndexOutOfRangeException))
            {   // thrown if reader["..."] attempts to get a value not retrieved by the sql statement. Could be catastrophic.
                // Require that code or db be fixed before continuing.
                // catching something like reader["xxx"] or reader[27] does not exist. In a data-correct table, this should not happen. 
                OnExceptionRaised(ex.ToString());
            }
            else if (ex.GetType() == typeof(FormatException))
            {
                OnExceptionRaised(ex.ToString());
            }
            else
            {
                OnExceptionRaised(trace + ex.ToString());
            }
        }   // HandleExceptions

        /// <summary>
        /// Let subscribers know that something completely unexpected happened.
        /// Wraps the test for any subscribers to the event before raising it.
        /// </summary>
        /// <param name="msg"></param>
        private void OnExceptionRaised(string msg)
        {
            if (ExceptionRaised != null)
            {
                ExceptionRaised(msg);
            }
        }

        #endregion utility methods

        #region event sinks

        #endregion event sinks

    }   // public class ImageFracta;Model: ImageInterface, IDisposable

    public class CameraImageData : CameraImageDataInterface
    {
        public Array Image { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int HorizontalBinning { get; set; }
        public int VerticalBinning { get; set; }
        public int BitsPerPixel { get; set; }
        public int Stride { get; set; }
        public float Gain { get; set; }
        public string CameraType { get; set; }
        public string CameraSerialNu { get; set; }
        public string Phi { get; set; }
        public string Timestamp { get; set; }

        public string Id { get; set; }
    }   // CameraImageData


}

