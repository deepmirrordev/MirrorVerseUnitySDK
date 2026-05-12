using System;
using Unity.Collections;
namespace MirrorVerse
{
    public enum ImageFormat
    {
        // The default image format from Android/iOS camera.
        Yuv420 = 0,
        // Gray scale images from some supported sensors.
        GrayScale = 1,
        // Encoded format like Jpeg or PNG.
        Encoded = 2,
        // RGBA format images from Quest 3 etc.
        RGBAFlipped = 3
    };

    // Represents an image buffer.
    public class ImageData
    {
        private ImageFormat _imageFormat;
        private NativeArray<byte>? _nativeArrayData;
        private IntPtr? _nativeArrayPtr;
        private byte[] _data;
        private int _width;
        private int _height;
        // Size of the native array in bytes.
        private int _nativePtrArraySize;

        public byte[] Data
        {
            get
            {
                return _data;
            }
        }

        public bool HasNativeArrayData
        {
            get
            {
                return _nativeArrayData.HasValue;
            }
        }

        public NativeArray<byte> NativeArrayData
        {
            get
            {
                return _nativeArrayData.Value;
            }
        }

        public IntPtr NativeArrayPtr
        {
            get
            {
                return _nativeArrayPtr ?? IntPtr.Zero;
            }
            set
            {
                _nativeArrayPtr = value;
            }
        }
        
        public int NativePtrArraySize
        {
            get
            {
                return _nativePtrArraySize;
            }
        }

        public ImageFormat ImageFormat
        {
            get
            {
                return _imageFormat;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public void Dispose()
        {
            if (_nativeArrayData.HasValue)
            {
                _nativeArrayData.Value.Dispose();
            }
        }

        public static ImageData NewEncodedImage(byte[] encodedStr, int width, int height)
        {
            ImageData image = new ImageData();
            // Encoded image string, e.g. from cv::imencode.
            // This string should be able to be parsed using cv::imdecode.
            image._data = encodedStr;
            image._imageFormat = ImageFormat.Encoded;
            image._width = width;   // Encoded images may have width/height encoded to data bytes,
            image._height = height; // but if provided, set to the properties seperately.
            return image;
        }

        public static ImageData NewBitmapImage(byte[] data, ImageFormat imageFormat, int width, int height)
        {
            ImageData image = new ImageData();
            image._data = data;
            image._imageFormat = imageFormat; // Yuv420 or GrayScale.
            image._width = width;
            image._height = height;
            return image;
        }

        public static ImageData NewBitmapImage(NativeArray<byte> nativeArrayData, ImageFormat imageFormat, int width, int height)
        {
            ImageData image = new ImageData();
            image._nativeArrayData = nativeArrayData;
            image._imageFormat = imageFormat; // Yuv420 or GrayScale.
            image._width = width;
            image._height = height;
            return image;
        }

        public static ImageData NewImageFromNativePtr(IntPtr nativeArrayPtr, ImageFormat imageFormat, int size, int width, int height)
        {
            ImageData image = new ImageData
            {
                _nativeArrayPtr = nativeArrayPtr,
                _imageFormat = imageFormat,
                _nativePtrArraySize = size,
                _width = width,
                _height = height
            };
            return image;
        }
    }
}
