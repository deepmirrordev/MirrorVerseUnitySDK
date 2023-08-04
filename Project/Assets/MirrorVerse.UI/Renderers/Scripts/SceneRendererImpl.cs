using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    public class SceneRendererImpl : SceneRenderer
    {
        public XrPlatformAdapter xrPlatformAdapter;
        public GameObject immediateRoot;
        public PointCloudRenderer pointCloudRenderer;
        public TrajectoryRenderer trajectoryRenderer;
        public ImmediateMeshRenderer immediateMeshRenderer;
        public ImmediateMeshRenderer minimapMeshRenderer;
        public DetectedBoxRenderer immediateBoxRenderer;
        public StaticMeshRenderer staticMeshRenderer;
        public NavMeshRenderer navMeshRenderer;
        public DetectedBoxRenderer staticBoxRenderer;
        public AirWallRenderer airWallRenderer;
        public RaycastRenderer raycastRenderer;

        public MarkerRenderer markerRenderer;
        public MinimapRenderer minimapRenderer;

        private GameObject _cameraObject;
        private string _currentClientId;
        private string _hostClientId;
        private bool _capturing;
        private Dictionary<string, ClientStreamRenderable> _allClientStreams = new();

        private void Start()
        {
            // Note: there are several sub render objects as children of this visual artifact renderer.
            // The root transform of visual artifacts renderer will change when session has a fused pose.
            // Then all children rednerers will follow the root transform.

            // Here constructs the sub renderers with given renderer options customized by applications
            /* - CoreRenderer (Component)
                  - ImmediateRoot  // This root's transform in Unity scene is controlled by server during streaming.
                      - PointClouds
                      - Trajectories
                      - ImmediateBox
                      - ImmediateMesh
                      - MinimapImmediateMesh (different layer mask)
                      - ...
                  - StaticMesh     // Reset of the object's transforms should be always set to identity.
                  - NavMesh
                  - StaticBox
                  - AirWall
                  - RaycastCursor
                  - CanvasRoot
                      - MarkerCanvas
                      - MinimapCanvas
            */

            // The client ID is generated when a new capture is started.
            _currentClientId = null;
            _hostClientId = null;
            _capturing = false;

            if (xrPlatformAdapter == null)
            {
                Debug.LogWarning(
                    "Warning: XR Platform Adapter instance is not set. All following rendering logic are aborted." +
                    $"Please check if 'XR Platform Adapter' property is correctly assigned.");
                return;
            }

            if (xrPlatformAdapter != null)
            {
                _cameraObject = xrPlatformAdapter.GetCameraObject();
            }

            if (raycastRenderer != null)
            {
                raycastRenderer.SetXrPlatformAdapter(xrPlatformAdapter);
            }

            if (pointCloudRenderer != null && _cameraObject != null)
            {
                pointCloudRenderer.SetCameraObject(_cameraObject);
            }

            if (minimapRenderer != null && _cameraObject != null)
            {
                minimapRenderer.SetArCameraObject(_cameraObject);
            }
        }

        public override void SetCapturing(bool capturing, string currentClientId = null, string hostClientId = null)
        {
            _capturing = capturing;
            if (capturing)
            {
                _currentClientId = currentClientId;

                if (!string.IsNullOrWhiteSpace(hostClientId))
                {
                    _hostClientId = hostClientId;
                }
                else
                {
                    // If given host client ID is null, current device is host.
                    _hostClientId = _currentClientId;
                }

                Debug.Log($"Capturing stream is started. Current cleint ID: {_currentClientId}, host client ID: {_hostClientId}");

                // Show minimap
                if (minimapRenderer != null && minimapRenderer.options.enabled)
                {
                    minimapRenderer.UpdateCurrentClient(_currentClientId);
                    minimapRenderer.ToggleVisibility(true);
                }
            }
            else
            {
                // Hide minimap
                if (minimapRenderer != null)
                {
                    minimapRenderer.ToggleVisibility(false);
                    minimapRenderer.ClearAll();
                }
            }
        }

        public override bool IsCapturing()
        {
            return _capturing;
        }

        public override void UpdateSceneOriginOffset(Pose sceneOriginOffset)
        {
            // Set the immediate root to scene origin offset.
            immediateRoot.transform.SetPositionAndRotation(sceneOriginOffset.position, sceneOriginOffset.rotation);
        }

        public override void UpdateClientConnectionStatus(string clientId, bool isConnected)
        {
            GetOrCreateClientStream(clientId).connected = isConnected;
            GetOrCreateClientStream(clientId).isHost = (clientId == GetHostClientId());
        }

        public override void UpdateClientPose(string clientId, Pose pose)
        {
            GetOrCreateClientStream(clientId).clientPose = pose;

            minimapRenderer.UpdateClientPose(clientId, pose);
        }

        public override void RenderPointCloudsBatch(IDictionary<string, Vector3[]> pointsBatch)
        {
            if (pointCloudRenderer != null)
            {
                pointCloudRenderer.SetNewBatch();
                foreach (var (clientId, points) in pointsBatch)
                {
                    if (points != null && points.Length > 0)
                    {
                        pointCloudRenderer.RenderPoints(clientId, points, _currentClientId == clientId);
                    }

                    GetOrCreateClientStream(clientId).points = points;
                }
            }
        }

        public override void RenderTrajectory(string clientId, Pose[] trajectory)
        {
            if (trajectoryRenderer != null)
            {
                if (trajectory != null && trajectory.Length > 0)
                {
                    trajectoryRenderer.RenderTrajectory(clientId, trajectory, _currentClientId == clientId);
                }

                GetOrCreateClientStream(clientId).trajectory = trajectory;
            }
        }

        public override void RenderImmediateMesh(MeshRenderable meshRenderable)
        {
            if (immediateMeshRenderer != null)
            {
                immediateMeshRenderer.RenderMeshObject(meshRenderable);
            }
            if (minimapMeshRenderer != null)
            {
                minimapMeshRenderer.RenderMeshObject(meshRenderable);
            }
        }

        public override void RenderStaticMesh(MeshRenderable meshRenderable)
        {
            if (staticMeshRenderer != null)
            {
                staticMeshRenderer.RenderMeshObject(meshRenderable);
            }
        }

        public override void RenderNavigationMesh(MeshRenderable meshRenderable)
        {
            if (navMeshRenderer != null)
            {
                navMeshRenderer.RenderMeshObject(meshRenderable);
            }
        }

        public override void RenderAirWall()
        {
            if (airWallRenderer != null)
            {
                airWallRenderer.RenderAirWall();
            }
        }

        public override void RenderImmediateDetectedObjects(ObjectDetectionResult objectDetection)
        {
            if (immediateBoxRenderer != null)
            {
                immediateBoxRenderer.RenderDetectedBoxes(objectDetection.boxDetections);
            }
        }

        public override void RenderStaticDetectedObjects(ObjectDetectionResult objectDetection)
        {
            if (staticBoxRenderer != null)
            {
                staticBoxRenderer.RenderDetectedBoxes(objectDetection.boxDetections);
            }
        }

        public override RaycastHitResult? RenderRaycastCursor(Matrix4x4 localToSceneTransform)
        {
            return raycastRenderer.RenderRaycastCursor(localToSceneTransform);
        }

        public override void ShowMarker(MarkerRenderable markerRenderable)
        {
            if (markerRenderer != null)
            {
                markerRenderer.RenderQrCodeImage(markerRenderable);
            }
        }

        public override void HideMarker()
        {
            if (markerRenderer != null)
            {
                markerRenderer.HideQrCodeImage();
            }
        }

        public override void CleanImmediate()
        {
            if (_allClientStreams != null)
            {
                _allClientStreams.Clear();
            }
            if (pointCloudRenderer != null)
            {
                pointCloudRenderer.ClearBuffers();
            }
            if (trajectoryRenderer != null)
            {
                trajectoryRenderer.ClearAll();
            }
            if (immediateMeshRenderer != null)
            {
                immediateMeshRenderer.Clear();
            }
            if (minimapMeshRenderer != null)
            {
                minimapMeshRenderer.Clear();
            }
            if (immediateBoxRenderer != null)
            {
                immediateBoxRenderer.ClearAllBoxes();
            }
            if (minimapRenderer != null)
            {
                minimapRenderer.ClearAll();
            }
            _currentClientId = null;
            _hostClientId = null;
        }

        public override void CleanStatic()
        {
            if (staticMeshRenderer != null)
            {
                staticMeshRenderer.ResetRenderer();
            }
            if (navMeshRenderer != null)
            {
                navMeshRenderer.ResetRenderer();
            }
            if (airWallRenderer != null)
            {
                airWallRenderer.ResetRenderer();
            }
            if (staticBoxRenderer != null)
            {
                staticBoxRenderer.ClearAllBoxes();
            }
        }

        public override void CleanAll()
        {
            CleanImmediate();
            CleanStatic();
        }

        public override string GetCurrentClientId()
        {
            return _currentClientId;
        }

        public override string GetHostClientId()
        {
            return _hostClientId;
        }

        public override bool IsCurrentDeviceHost()
        {
            return _hostClientId != null && _currentClientId == _hostClientId;
        }

        public override Pose? GetRaycastCursorPose()
        {
            if (raycastRenderer != null)
            {
                return raycastRenderer.GetCursorPose();
            }
            return null;
        }

        public override GameObject GetStaticMeshObject()
        {
            if (staticMeshRenderer != null)
            {
                return staticMeshRenderer.GetMeshObject();
            }
            return null;
        }
        
        public override GameObject GetNavMeshObject()
        {
            if (navMeshRenderer != null)
            {
                return navMeshRenderer.GetMeshObject();
            }
            return null;
        }

        public override SceneStreamRenderable GetStreamRenderable()
        {
            SceneStreamRenderable streamRenderable = new SceneStreamRenderable();
            streamRenderable.sharedOriginOffset = new Pose(immediateRoot.transform.position, immediateRoot.transform.rotation);
            streamRenderable.currentClientId = GetCurrentClientId();
            streamRenderable.clientStreams = _allClientStreams;
            streamRenderable.immediateMesh = GetImmediateMesh();
            return streamRenderable;
        }

        private ClientStreamRenderable GetOrCreateClientStream(string clientId)
        {
            if (!_allClientStreams.ContainsKey(clientId))
            {
                ClientStreamRenderable client = new();
                client.clientId = clientId;
                client.connected = true;
                client.isHost = false;
                client.clientPose = null;
                _allClientStreams[clientId] = client;
            }
            return _allClientStreams[clientId];
        }

        private Mesh GetImmediateMesh()
        {
            if (immediateMeshRenderer != null)
            {
                MeshFilter meshFilter = immediateMeshRenderer.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    return meshFilter.mesh;
                }
            }
            return null;
        }
    }
}
