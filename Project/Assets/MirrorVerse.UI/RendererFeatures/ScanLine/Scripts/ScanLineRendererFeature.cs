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

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_options == null)
            {
                return;
            }
            _renderPass.Setup(_options);
            renderer.EnqueuePass(_renderPass);
        }

        public void Setup(ScanLineRendererOptions options)
        {
            _options = options;
        }
    }
}
