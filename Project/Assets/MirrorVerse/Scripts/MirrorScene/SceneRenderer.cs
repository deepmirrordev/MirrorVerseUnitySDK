using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse
{
    // Abstract class as interface that MirrorVerse core system will be calling to render the space or canvas.
    // Developer can extend it to customize the visualization when working with AR oprations provided by the system.
    public abstract class SceneRenderer : MonoBehaviour
    {
        // Core system sets the Xr Platform Adapter reference to the renderer.
        public abstract void SetXrPlatformAdapter(XrPlatformAdapter xrPlatformAdapter);

        // Core system calls when user starts capturing or stops.
        public abstract void SetCapturing(bool capturing, string currentClientId = null, string hostClientId = null);

        // Core system sends updated scene origin offset coordinate from localizar.
        public abstract void UpdateSceneOriginOffset(Pose sceneOriginOffset);

        // Core system sends connected status from client.
        public abstract void UpdateClientConnectionStatus(string clientId, bool isConnected);

        // Core system sends pose from client.
        public abstract void UpdateClientPose(string clientId, Pose pose);

        // Core system requests to render point clouds.
        public abstract void RenderPointCloudsBatch(IDictionary<string, Vector3[]> pointsBatch);

        // Core system requests to render point clouds.
        public abstract void RenderTrajectory(string clientId, Pose[] trajectory);

        // Core system requests to render immediate mesh.
        public abstract void RenderImmediateMesh(MeshRenderable meshRenderable);

        // Core system requests to render static mesh.
        public abstract void RenderStaticMesh(MeshRenderable meshRenderable);

        // Core system requests to render navigation mesh.
        public abstract void RenderNavigationMesh(MeshRenderable meshRenderable);

        // Core system requests to render air wall.
        public abstract void RenderAirWall();

        // Core system requests to render immediate detected objects.
        public abstract void RenderImmediateDetectedObjects(ObjectDetectionResult objectDetection);

        // Core system requests to render static detected objects.
        public abstract void RenderStaticDetectedObjects(ObjectDetectionResult objectDetection);

        // Core system requests to render raycast cursor.
        public abstract RaycastHitResult? RenderRaycastCursor(Matrix4x4 localToSceneTransform);

        // Core system requests to show QR code marker.
        public abstract void ShowMarker(MarkerRenderable markerRenderable);

        // Core system requests hide QR code marker.
        public abstract void HideMarker();

        // Core system requests to clean all immediate artifacts.
        public abstract void CleanImmediate();

        // Core system requests to clean all static artifacts.
        public abstract void CleanStatic();

        // Core system requests to clean everything.
        public abstract void CleanAll();

        // Returns current client's ID.
        public abstract string GetCurrentClientId();

        // Returns hosting client's ID.
        public abstract string GetHostClientId();

        // Returns whether core system is captuing.
        public abstract bool IsCapturing();

        // Whether the current device is the host.
        public abstract bool IsCurrentDeviceHost();

        // Returns the raycast cursor pose.
        public abstract Pose? GetRaycastCursorPose();

        // Returns the current precessed static mesh object.
        public abstract GameObject GetStaticMeshObject();

        // Returns the current precessed navigation mesh object.
        public abstract GameObject GetNavMeshObject();

        // Returns stream renderable object for custom events.
        public abstract SceneStreamRenderable GetStreamRenderable();
    }
}
