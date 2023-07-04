namespace MirrorVerse
{
    public enum ImageFormat
    {
        // The default image format from Android/iOS camera.
        Yuv420,
        // Gray scale images from some supported sensors.
        GrayScale,
        // Encoded format like Jpeg or PNG.
        Encoded
    };

    // Represents an image buffer.
    public class ImageData
    {
        private ImageFormat _imageFormat;
        private byte[] _data;
        private int _width;
        private int _height;

        public byte[] Data
        {
            get  {
                return _data;
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
            image._imageFormat = imageFormat;  // Yuv420 or GrayScale.
            image._width = width;
            image._height = height;
            return image;
        }
    }
}
