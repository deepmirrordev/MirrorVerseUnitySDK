using MirrorVerse.Options;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    public class NavMeshRenderer : MonoBehaviour
    {
        public NavMeshRendererOptions options;
        private GameObject _rootObject;
        private bool _meshHasCollision = false;
        
        private MeshRenderable _meshRenderable;

        private void Update()
        {
            ToggleVisibility(options.visible);
            ToggleCollision(options.collidable);
        }

        public GameObject GetMeshObject()
        {
            return _rootObject;
        }

        private void ToggleVisibility(bool visible)
        {
            if (_rootObject == null)
            {
                return;
            }
            if (_rootObject.activeSelf == visible)
            {
                // No change. Skip.
                return;
            }
            _rootObject.SetActive(visible);
        }

        private void ToggleCollision(bool enabled)
        {
            if (_rootObject == null)
            {
                return;
            }
            if (enabled == _meshHasCollision)
            {
                // No change. Skip.
                return;
            }

            MeshCollider existingCollider = _rootObject.GetComponent<MeshCollider>();
            if (enabled && existingCollider == null)
            {
                // Add collider from mesh.
                // TODO: For now we simply use the same mesh for collider. Once we have Nav Mesh generated, we could use NavMesh for collider.
                MeshFilter meshFilter = _rootObject.GetComponent<MeshFilter>();
                MeshCollider meshCollider = _rootObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            }
            else if (!enabled && existingCollider != null)
            {
                // Remove collider.
                existingCollider.sharedMesh = null;
                Destroy(existingCollider);
            }
            _meshHasCollision = enabled;
        }

        public void RenderMeshObject(MeshRenderable meshRenderable)
        {
            if (_rootObject == null)
            {
                InitMeshObject();
            }
            Debug.Log($"Render nav mesh object: {_rootObject.name}.");

            if (meshRenderable != null)
            {
                _meshRenderable = meshRenderable;
                MeshFilter meshFilter = _rootObject.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = _meshRenderable.mesh;
            }
        }

        public void ResetRenderer()
        {
            ClearMeshObject();
        }

        private void InitMeshObject()
        {
            if (_rootObject != null)
            {
                ClearMeshObject();
            }
            long currentTimestamp = RendererUtils.NowMs();
            _rootObject = new GameObject("navmesh_" + currentTimestamp);
            Debug.Log($"Initialize a new navmesh object: {_rootObject.name}.");
            MeshRenderer meshRenderer = _rootObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = options.defaultMaterials;
            _rootObject.AddComponent<MeshFilter>();
            _rootObject.transform.SetParent(gameObject.transform);
        }

        private void ClearMeshObject()
        {
            if (_rootObject != null)
            {
                Destroy(_rootObject);
                _meshHasCollision = false;
                _rootObject = null;
            }
            _meshRenderable = null;
        }

        private void OnDestroy()
        {
            ClearMeshObject();
        }
    }
}
