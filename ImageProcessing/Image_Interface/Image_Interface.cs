using System;

namespace Image_Interface
{

    public delegate void DelExceptionRaised(string msg);    // notice of unexpected issue
    
    /// <summary>
    /// methods to support images 
    /// </summary>
    public interface ImageInterface
    {
        // events are declared in the implementing class
        event DelExceptionRaised ExceptionRaised;

        void Dispose();

        bool Initialize(string id);

        CameraImageDataInterface ImgData { get; set; }

        CameraImageDataInterface AcquireTheImage();
        
    }   // public interface ImageInterface

    /// <summary>
    /// structure to pass img data
    /// </summary>
    public interface CameraImageDataInterface
    {
        Array Image { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        int HorizontalBinning { get; set; }
        int VerticalBinning { get; set; }
        int BitsPerPixel { get; set; }
        int Stride { get; set; }
        float Gain { get; set; }

        /// <summary>
        /// discriminator to allow for multiple instances of this class
        /// </summary>
        string Id { get; set; }
        string CameraType { get; set; }
        string CameraSerialNu { get; set; }
        string Timestamp { get; set; }
        string Phi { get; set; }
    }   // public interface CameraImageDataInterface

}   // namespace Image_Interface
