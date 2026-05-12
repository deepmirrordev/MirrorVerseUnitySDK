using MirrorVerse.Options;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RendererFeatures
{
    public class ScanLineRendererFeature : ScriptableRendererFeature
    {
        private ScanLineRenderPass _renderPass;
        private ScanLineRendererOptions _options;

        public override void Create()
        {
            _renderPass = new ScanLineRenderPass();
        }

        void ConfigPass(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (_options == null)
            {
                return;
            }
            _renderPass.Setup(_options);
        }

#if UNITY_2022_1_OR_NEWER
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            ConfigPass(renderer, renderingData);
        }
#endif

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if !UNITY_2022_1_OR_NEWER
            ConfigPass(renderer, renderingData);
#endif
            var cameraData = renderingData.cameraData;
            var camera = renderingData.cameraData.camera;
            var camPixelSize = cameraData.camera.pixelRect.size;
            renderer.EnqueuePass(_renderPass);
        }

        public void Setup(ScanLineRendererOptions options)
        {
            _options = options;
        }
    }
}
