using UnityEngine;

namespace MirrorVerse
{
    // Interface to represent a frame in XR session.
    public interface IXrFrame
    {
        // Intrinsics of the camera.
        CameraIntrinsics? CameraIntrinsics { get; }

        // Raw image of the frame.
        ImageData CameraImage { get; }

        // Device pose of the frame.
        Pose? DevicePose { get; }

        // Timestamp of the frame in milliseconds since epoch.
        long Timestamp { get; }
    }
}
