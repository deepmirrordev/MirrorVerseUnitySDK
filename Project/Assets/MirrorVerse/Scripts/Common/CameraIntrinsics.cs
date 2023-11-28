using System;

namespace MirrorVerse
{
    public enum CameraModel
    {
        Pinhole,
        Fisheye
    }

    [Serializable]
    // Represends XR Camera intrinsics informations.
    public struct CameraIntrinsics
    {
        public float fx;
        public float fy;
        public float cx;
        public float cy;

        public float[] distortionCoefs;

        public int width;
        public int height;

        public CameraModel cameraModel;
    }
}
