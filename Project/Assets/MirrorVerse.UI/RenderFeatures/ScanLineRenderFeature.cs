using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RenderFeatures
{
    public class ScanLineRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class CustomRenderPassSettings
        {
            public Material material;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public Material GetSharedMaterial()
        {
            if (settings != null)
            {
                return settings.material;
            }
            return null;
        }

        public CustomRenderPassSettings settings = new CustomRenderPassSettings();
        private ScanLineRenderPass _scriptablePass;

        public override void Create()
        {
            _scriptablePass = new ScanLineRenderPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_scriptablePass);
        }
    }

    public class ScanLineRenderPass : ScriptableRenderPass
    {
        RenderTargetIdentifier source;
        RenderTargetHandle destination;

        ScanLineRenderFeature.CustomRenderPassSettings settings;

        public ScanLineRenderPass(ScanLineRenderFeature.CustomRenderPassSettings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(destination.id, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera currentCamera = renderingData.cameraData.camera;
            if (currentCamera == Camera.main)
            {
                // Only show effect on main camera.
                CommandBuffer cmd = CommandBufferPool.Get();

                cmd.Blit(source, destination.Identifier(), settings.material, 0);
                cmd.Blit(destination.Identifier(), source);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(destination.id);
        }
    }
}
