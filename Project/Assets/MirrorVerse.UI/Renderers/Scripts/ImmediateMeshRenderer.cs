using MirrorVerse.Options;
using System;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Immediate mesh renderer for mesh rendering during scene streaming.
    public class ImmediateMeshRenderer : MonoBehaviour
    {
        public ImmediateMeshRendererOptions options;

        private GameObject _rootObject;
        private bool _meshHasCollision = false;

        private void Start()
        {
            // Initial with given layer. The minimap mesh renderer may have different layer.
            if (options.layer > 0)
            {
                gameObject.layer = options.layer;
            }
            _rootObject = gameObject;
        }

        private void Update()
        {
            ToggleVisibility(options.visible);
            ToggleCollision(options.collidable);
        }

        private void ToggleVisibility(bool visible)
        {
            if (_rootObject.activeSelf == visible)
            {
                // No change. Skip.
                return;
            }
            _rootObject.SetActive(visible);
        }

        private void ToggleCollision(bool enabled)
        {
            if (enabled == _meshHasCollision)
            {
                // No change. Skip.
                return;
            }
            _meshHasCollision = enabled;
            UpdateCollionMesh();
        }

        private void UpdateCollionMesh()
        {
            MeshCollider meshCollider = _rootObject.GetComponent<MeshCollider>();
            if (_meshHasCollision)
            {
                // Update collider from mesh if exists.
                MeshFilter meshFilter = _rootObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    if (meshCollider == null)
                    {
                        meshCollider = _rootObject.AddComponent<MeshCollider>();
                    }
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }
            }
            else
            {
                // Remove existing collider.
                if (meshCollider != null)
                {
                    meshCollider.sharedMesh = null;
                    Destroy(meshCollider);
                }
            }
        }

        public void RenderMeshObject(MeshRenderable meshRenderable)
        {
            GetMeshFilter().sharedMesh = meshRenderable.mesh;
            UpdateCollionMesh();
        }

        public void Clear()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null)
            {
                meshFilter.sharedMesh.Clear();
            }
        }

        private MeshFilter GetMeshFilter()
        {
            if (!_rootObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer = _rootObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = options.materials;
            }
            if (!_rootObject.TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter = _rootObject.AddComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }
}
