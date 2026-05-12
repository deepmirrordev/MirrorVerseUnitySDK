using UnityEngine;

namespace MirrorVerse
{
    public struct TrackedImageResult
    {
        public long timestamp;

        // Unique name to identify the tracked image.
        public string referenceImageName;

        // Pose under local AR world origin (first device pose when AR camera is started).
        public Pose localPose;

        // Offset of this tracked image to the origin image.
        // If this tracked image is the origin image, then offset is identity.
        public Pose offsetPose;

        // Ecef pose of the tracked image in physical reality.
        public EcefPose? ecefPose;

        // Geodetic pose of the tracked image in physical reality. It's the same position of Ecef but in WGS84 format.
        public GeodeticPose? geodeticPose;

        // Size of the tracked image. This could be different from reference image data
        // because it could be printed on different size of poster.
        public Vector2 trackedSize;
    }
}
