using MirrorVerse.Options;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Immediate mesh renderer for mesh rendering during scene streaming.
    public class ImmediateMeshRenderer : MonoBehaviour
    {
        public ImmediateMeshRendererOptions options;

        private GameObject _wrapperObject;

        private void Start()
        {
            // Initial with given layer. The minimap mesh renderer may have different layer.
            if (options.layer > 0)
            {
                gameObject.layer = options.layer;
            }
        }

        private void Update()
        {
            ToggleVisibility(options.visible);
        }

        private void ToggleVisibility(bool visible)
        {
            if (_wrapperObject == null)
            {
                return;
            }
            if (_wrapperObject.activeSelf == visible)
            {
                // No change. Skip.
                return;
            }
            _wrapperObject.SetActive(visible);
        }

        public void RenderMeshObject(MeshRenderable meshRenderable)
        {
            // Note: Immediate mesh is always draco compressed.
            GetMeshFilter().sharedMesh = meshRenderable.mesh;
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
            GameObject rootObject = _wrapperObject == null ? gameObject : _wrapperObject;

            if (!rootObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer = rootObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = options.materials;
            }
            if (!rootObject.TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter = rootObject.AddComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }
}
