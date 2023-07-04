namespace MirrorVerse
{
    // Represents information from QR Code marker detection.
    public struct MarkerInfo
    {
        // Associated scene ID in the QrCode.
        public string sceneId;

        // Extra information to display.
        public string displayInfo;

        // Extra raw bytes to be contained in the QrCode.
        public byte[] extraData;
    }
}
