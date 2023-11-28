using UnityEngine;

namespace MirrorVerse
{
    // Interface to represent a frame in XR session.
    public interface IXrFrame
    {
        // Intrinsics of the camera.
        CameraIntrinsics? CameraIntrinsics { get; }

        // Raw image of the frame.
        ImageData Image { get; }

        // Timestamp of the frame in nanoseconds.
        long Timestamp { get; }

        // Camera pose of the frame. This is the physical camera pose in Unity world.
        // Calculate camera pose from device pose:  CameraPose = DevicePose * Extrinsics
        Pose? CameraPose { get; }

        // Device pose of the frame. For headwearing devices, this is also usually the IMU, center-eye or head in Unity world.
        // Convert device pose from camera pose: DevicePose = CameraPose * InverseExtrinsics
        Pose? DevicePose { get; }
    }
}
