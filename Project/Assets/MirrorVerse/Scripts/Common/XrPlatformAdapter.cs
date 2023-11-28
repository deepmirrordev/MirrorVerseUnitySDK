using UnityEngine;

namespace MirrorVerse
{
    // Interface to adapt configuration and data like XR poses, camera image
    // and other XR device/platform specific informations.
    public abstract class XrPlatformAdapter : MonoBehaviour
    {
        public abstract TrackingStatus GetTrackingStatus();

        public abstract GameObject GetCameraObject();

        public abstract bool GetCameraIntrinsics(out CameraIntrinsics cameraIntrinsics);

        public abstract bool GetCameraExtrinsics(out Pose cameraExtrinsics);

        public abstract bool GetCameraImagePose(out ImageData cameraImage, out Pose cameraPose, out long timestamp);

        public abstract bool GetDevicePose(out Pose devicePose, out long timestamp);

        public abstract bool SelectCameraConfiguration(out CameraConfiguration cameraConfiguration);

        public abstract void SetDetectedPlanePrefab(GameObject detectedPlanePrefab);

        public abstract void SetRaycastOnPlaneEnabled(bool enabled);

        public abstract void ToggleDetectedPlaneVisibility(bool visible);

        public abstract RaycastHitResult? TriggerRaycastOnPlane(Matrix4x4 transformFromLocalOrigin);
    }
}
