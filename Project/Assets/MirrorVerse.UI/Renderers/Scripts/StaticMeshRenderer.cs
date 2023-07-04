using MirrorVerse.Options;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Mesh renderer for static mesh after scene has processed. Useful for collision, occlusion and shadows.
    public class StaticMeshRenderer : MonoBehaviour
    {
        public StaticMeshRendererOptions options;

        private GameObject _rootObject;
        private bool _meshHasCollision = false;
        private bool _meshHasOcclusion = false;
        private bool _meshHasShadowCaster = false;
        private bool _meshHasShadowReceiver = false;

        private MeshRenderable _meshRenderable;

        private void Update()
        {
            ToggleVisibility(options.visible);
            ToggleCollision(options.collidable);
            ToggleMeshShaders(options.withOcclusion, options.castsShadow, options.receivesShadow);
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

        private void ToggleMeshShaders(bool occlusion, bool castShadow, bool receiveShadow)
        {
            // Resolves AR transparent shaders the AR session processed mesh. There are multiple bits and be turned on or off.
            // If any of the bits is ON, the mesh will appear as transparent mesh. If all bits are OFF, the mesh will appear with original material from model loader.
            // Note that if shadow caster is ON, occlusion is also ON.
            if (_rootObject == null)
            {
                return;
            }
            if (occlusion == _meshHasOcclusion &&
                castShadow == _meshHasShadowCaster &&
                receiveShadow == _meshHasShadowReceiver)
            {
                // No change. Skip.
                return;
            }

            Renderer[] meshRenderers = _rootObject.GetComponentsInChildren<UnityEngine.MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                // Note occlusion shader and occlusion shadow caster shader are mutually exclusive.
                // And if shadow caster is enabled, occlusion has to be enabled as well.
                bool useOcclusionShader = occlusion && !castShadow;
                bool useOcclusionShadowCasterShader = occlusion && castShadow;
                bool useShadowReceiver = receiveShadow;
                List<Material> transparentMaterials = new();
                if (useOcclusionShader)
                {
                    transparentMaterials.Add(options.occlusionMaterial);
                }
                if (useOcclusionShadowCasterShader)
                {
                    transparentMaterials.Add(options.occlusionShadowCasterMaterial);
                }
                if (useShadowReceiver)
                {
                    transparentMaterials.Add(options.shadowReceiverMaterial);
                }
                if (transparentMaterials.Count > 0)
                {
                    meshRenderer.sharedMaterials = transparentMaterials.ToArray();
                }
                else
                {
                    // Restore to original mesh material from model loader.
                    if (_meshRenderable != null && _meshRenderable.meshType == SceneMeshType.TexturedObjZip && options.textureMaterial != null)
                    {
                        meshRenderer.sharedMaterials = new Material[] { options.textureMaterial };
                    }
                    else if (options.defaultMaterials != null && options.defaultMaterials.Length > 0)
                    {
                        meshRenderer.sharedMaterials = options.defaultMaterials;
                    }
                }
            }
            _meshHasOcclusion = occlusion;
            _meshHasShadowCaster = castShadow;
            _meshHasShadowReceiver = receiveShadow;
        }

        public void RenderMeshObject(MeshRenderable meshRenderable)
        {
            if (_rootObject == null)
            {
                InitMeshObject();
            }
            Debug.Log($"Render mesh object: {_rootObject.name}.");

            if (meshRenderable != null)
            {
                _meshRenderable = meshRenderable;
                if (_meshRenderable.meshType == SceneMeshType.TexturedObjZip)
                {
                    MeshRenderer meshRenderer = _rootObject.GetComponent<MeshRenderer>();
                    options.textureMaterial.mainTexture = _meshRenderable.texture;
                    meshRenderer.sharedMaterials = new Material[] { options.textureMaterial };
                }
                MeshFilter meshFilter = _rootObject.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = _meshRenderable.mesh;

                // Immediately toggle shaders for newly rendered mesh.
                ToggleMeshShaders(options.withOcclusion, options.castsShadow, options.receivesShadow);
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
            _rootObject = new GameObject("mesh_" + currentTimestamp);
            Debug.Log($"Initialize a new mesh object: {_rootObject.name}.");
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
                if (options.textureMaterial != null)
                {
                    options.textureMaterial.mainTexture = null;
                }
            }
            _meshRenderable = null;
            _meshHasOcclusion = false;
            _meshHasShadowCaster = false;
            _meshHasShadowReceiver = false;
        }

        private void OnDestroy()
        {
            ClearMeshObject();
        }
    }
}
