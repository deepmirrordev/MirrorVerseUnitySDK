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
        
        public bool Equals(CameraIntrinsics other)
        {
            if (distortionCoefs.Length != other.distortionCoefs.Length)
            {
                return false;
            }
            
            for (int i = 0; i < distortionCoefs.Length; i++)
            {
                if (!distortionCoefs[i].Equals(other.distortionCoefs[i]))
                {
                    return false;
                }
            }
            
            return fx.Equals(other.fx) && fy.Equals(other.fy) && cx.Equals(other.cx) && cy.Equals(other.cy) 
                   && width == other.width && height == other.height && cameraModel == other.cameraModel;
        }
    }
}
