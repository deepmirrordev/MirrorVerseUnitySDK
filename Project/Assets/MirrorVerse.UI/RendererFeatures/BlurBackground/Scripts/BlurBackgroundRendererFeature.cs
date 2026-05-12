using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RendererFeatures
{
    // Outer ScriptableRendererFeature that resolves a BlurBackgroundSource for the
    // active camera and enqueues the BlurBackgroundRenderPass. The pass class itself
    // (in BlurBackgroundRenderPass.cs) has #if-guarded URP 17 / URP 14 implementations
    // sharing the same Setup(source, renderer) contract; this outer feature stays
    // platform-agnostic except for the URP 17-only configuration in Create().
    public class BlurBackgroundRendererFeature : ScriptableRendererFeature
    {
        private BlurBackgroundRenderPass _renderPass;

        private readonly Dictionary<Camera, BlurBackgroundSource> _sourceCache =
            new Dictionary<Camera, BlurBackgroundSource>();

        public override void Create()
        {
            _renderPass = new BlurBackgroundRenderPass();
            _sourceCache.Clear();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var source = GetSource(camera);
            if (source == null || !source.enabled || !Application.isPlaying)
            {
                return;
            }
            source.PrepareBlurredScreen();
            _renderPass.Setup(source, renderer);
#if UNITY_6000_0_OR_NEWER
            // RenderGraph-only configuration. Set inside AddRenderPasses (not Create)
            // so URP only allocates the intermediate texture when the feature is active
            // for this camera. Putting these in Create() makes URP's render-pass setup
            // fire even for m_Active = 0 / source-less features and emit "EndRenderPass:
            // Not inside a Renderpass" at startup. We don't sample depth, so request
            // Color only.
            _renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            _renderPass.requiresIntermediateTexture = true;
#endif
            renderer.EnqueuePass(_renderPass);
        }

        private BlurBackgroundSource GetSource(Camera camera)
        {
            if (!_sourceCache.ContainsKey(camera))
            {
                _sourceCache.Add(camera, camera.GetComponent<BlurBackgroundSource>());
            }
            return _sourceCache[camera];
        }
    }
}
