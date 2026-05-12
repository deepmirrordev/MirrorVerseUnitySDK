using MirrorVerse.Options;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RendererFeatures
{
    public class ScanLineRenderPass : ScriptableRenderPass
    {
#if UNITY_2022_3_OR_NEWER
        private RTHandle _source;
        private RTHandle _destination;
#else
        private RenderTargetIdentifier _source;
        private RenderTargetHandle _destination;
#endif

        private ScanLineRendererOptions _options;

        public ScanLineRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public void Setup(ScanLineRendererOptions options)
        {
            _options = options;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
#if UNITY_2022_1_OR_NEWER
            _source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            int width = Screen.width;
            int height = Screen.height;

            var camDesc = renderingData.cameraData.cameraTargetDescriptor;
            if (camDesc.width > 0 && camDesc.height > 0)
            {
                width = camDesc.width;
                height = camDesc.height;
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogError("Invalid size!");
                return;
            }
            
            // Manually initialization. Camera setup may fail.
            var safeDesc = new RenderTextureDescriptor(width, height)
            {
                colorFormat = RenderTextureFormat.ARGB32,
                depthBufferBits = (int)DepthBits.None,
                msaaSamples = 1,
                useMipMap = false,
                autoGenerateMips = false,
                volumeDepth = 1,
                dimension = TextureDimension.Tex2D
            };

            _destination?.Release();

            try
            {
                _destination = RTHandles.Alloc(
                        width: safeDesc.width,
                        height: safeDesc.height,
                        slices: 1,
                        filterMode: FilterMode.Bilinear,
                        wrapMode: TextureWrapMode.Clamp,
                        colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
                        name: "_ScanLineTemp"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"RTHandles.Alloc failed: {e}");
                return;
            }
#else
            _source = renderingData.cameraData.renderer.cameraColorTarget;
            _destination.Init("_ScanLineTemp");
            cmd.GetTemporaryRT(_destination.id, renderingData.cameraData.cameraTargetDescriptor);
#endif
        }

#if UNITY_2022_3_OR_NEWER
        private static Material _blitMaterial;

        private static Material GetBlitMaterial()
        {
            if (_blitMaterial != null) return _blitMaterial;

            string[] shaderNames = {
                "Hidden/Universal Render Pipeline/Blit", // Likely this one.
                "Hidden/BlitToScreen",
                "Hidden/CopyTexture",
                "Hidden/Internal-Colored"
            };

            foreach (var name in shaderNames)
            {
                var shader = Shader.Find(name);
                if (shader != null)
                {
                    _blitMaterial = CoreUtils.CreateEngineMaterial(shader);
                    _blitMaterial.hideFlags = HideFlags.HideAndDontSave;
                    Debug.Log($"[MVBlitUtils] Using shader: {name}");
                    return _blitMaterial;
                }
            }

            Debug.LogError("Failed to find blit shader. Creating fallback.");
            _blitMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            _blitMaterial.hideFlags = HideFlags.HideAndDontSave;
            return _blitMaterial;
        }
#endif

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera != Camera.main)
            {
                return;
            }

#if UNITY_2022_3_OR_NEWER
            if (_source == null || _destination == null)
            {
                Debug.LogError("Resources not ready!");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            var copyMat = GetBlitMaterial();
            if (copyMat == null)
            {
                Debug.LogError("Copy material is NULL!");
                CommandBufferPool.Release(cmd);
                return;
            }

            cmd.Blit(_source, _destination, _options.material, 0);
            cmd.SetGlobalTexture("_ScanLineTemp", _destination);
            cmd.Blit(_destination, _source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
#else

            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.Blit(_source, _destination.Identifier(), _options.material, 0);
            cmd.Blit(_destination.Identifier(), _source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
#endif
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
#if UNITY_2022_3_OR_NEWER
            _destination?.Release();
#else
            cmd.ReleaseTemporaryRT(_destination.id);
#endif
        }

        public void Dispose()
        {
#if UNITY_2022_3_OR_NEWER
            _destination?.Release();
#endif
        }
    }
}