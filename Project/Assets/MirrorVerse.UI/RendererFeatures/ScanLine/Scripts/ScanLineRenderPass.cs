using MirrorVerse.Options;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RendererFeatures
{
    public class ScanLineRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier _source;
        private RenderTargetHandle _destination;
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
            _source = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(_destination.id, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera currentCamera = renderingData.cameraData.camera;
            if (currentCamera == Camera.main)
            {
                // Only show effect on main camera.
                CommandBuffer cmd = CommandBufferPool.Get();

                cmd.Blit(_source, _destination.Identifier(), _options.material, 0);
                cmd.Blit(_destination.Identifier(), _source);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_destination.id);
        }
    }
}
