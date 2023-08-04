using MirrorVerse.Options;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Render raycast cursor on detected plane or scene mesh.
    public class RaycastRenderer : MonoBehaviour
    {
        public RaycastRendererOptions options;
        public StaticMeshRenderer staticMeshRenderer;
        public NavMeshRenderer navMeshRenderer;

        private XrPlatformAdapter _xrPlatformAdapter;
        private Camera _camera;
        private GameObject _cursorObject;
        private RaycastHitResult? _hitResult;

        public Pose? GetCursorPose()
        {
            if (_hitResult.HasValue)
            {
                return _hitResult.Value.raycastHitPose;
            }
            return null;
        }

        public RaycastHitMode? GetCursorHitMode()
        {
            return _hitResult?.mode;
        }



        public void SetXrPlatformAdapter(XrPlatformAdapter xrPlatformAdapter)
        {
            _xrPlatformAdapter = xrPlatformAdapter;
            if (options.detectedPlanePrefab != null)
            {
                _xrPlatformAdapter.SetDetectedPlanePrefab(options.detectedPlanePrefab);
            }
            _camera = _xrPlatformAdapter.GetCameraObject().GetComponent<Camera>();
        }

        void Update()
        {
            if (options.cursorVisible)
            {
                if (_hitResult.HasValue)
                {
                    if (_cursorObject == null)
                    {
                        _cursorObject = Instantiate(options.cursorPrefab, gameObject.transform);
                    }
                    if (_cursorObject.activeSelf == false)
                    {
                        _cursorObject.SetActive(true);
                    }

                    Pose pose = _hitResult.Value.raycastHitPose;
                    if (_hitResult.Value.mode == RaycastHitMode.Plane)
                    {
                        // For plane cursor, rotate 90 on x-axis of the quad, same as set in cursor prefab.
                        pose.rotation = Quaternion.AngleAxis(90, Vector3.right);
                    }
                    _cursorObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
                }
                else
                {
                    if (_cursorObject != null && _cursorObject.activeSelf)
                    {
                        _cursorObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (_cursorObject != null && _cursorObject.activeSelf)
                {
                    _cursorObject.SetActive(false);
                }
            }

            ToggleDetectedPlaneVisibility();
        }
        
        private void ToggleDetectedPlaneVisibility()
        {
            _xrPlatformAdapter.SetRaycastOnPlaneEnabled(options.raycastOnPlaneEnabled);
            _xrPlatformAdapter.ToggleDetectedPlaneVisibility(options.detectedPlaneVisible);
        }

        public RaycastHitResult? RenderRaycastCursor(Matrix4x4 localToSceneTransform)
        {
            RaycastHitResult? raycastHitResult = null;

            // If raycast has turned on for mesh, check mesh existance first.
            if (options.raycastOnMeshEnabled)
            {
                // If mesh exists, do raycast on mesh.
                if (staticMeshRenderer != null && staticMeshRenderer.GetMeshObject() != null)
                {
                    // Only raycast on mesh is enabled.
                    raycastHitResult = TriggerRaycastOnStaticMesh();
                    _hitResult = raycastHitResult;
                    return raycastHitResult;
                }
            }

            // Fallback to raycast on plane if plane detection has turned on.
            if (options.raycastOnPlaneEnabled)
            {
                // Whether to do a local to fusion transformation, because raycast hit
                // from XR device adapter is always under local tracking coordinates.
                raycastHitResult = _xrPlatformAdapter.TriggerRaycastOnPlane(localToSceneTransform);
                _hitResult = raycastHitResult;
            }
            return raycastHitResult;
        }

        private RaycastHitResult? TriggerRaycastOnStaticMesh()
        {
            // Update the raycast cursor using raycast detection.
            RaycastHit? hit = null;
            float distance = 10;

            Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            MeshCollider[] allMeshColliders = new MeshCollider[0];
            if (staticMeshRenderer != null)
            {
                allMeshColliders = staticMeshRenderer.GetComponentsInChildren<MeshCollider>();
            }
            if (allMeshColliders.Length == 0 && navMeshRenderer != null)
            {
                allMeshColliders = navMeshRenderer.GetComponentsInChildren<MeshCollider>();
            }
            foreach (MeshCollider collider in allMeshColliders)
            {
                if (collider == null || !collider.Raycast(ray, out var currentHit, distance)
                                        || (currentHit.distance >= distance))
                {
                    continue;
                }
                distance = currentHit.distance;
                hit = currentHit;
            }
            if (hit.HasValue)
            {
                RaycastHitResult hitResult = new RaycastHitResult();
                hitResult.mode = RaycastHitMode.Mesh;
                hitResult.raycastHitPose = new Pose(hit.Value.point,
                    Quaternion.FromToRotation(Vector3.forward, hit.Value.normal));
                return hitResult;
            }
            return null;
        }


        private void OnDestroy()
        {
            if (_cursorObject != null)
            {
                Destroy(_cursorObject);
                _cursorObject = null;
            }
        }
    }
}
