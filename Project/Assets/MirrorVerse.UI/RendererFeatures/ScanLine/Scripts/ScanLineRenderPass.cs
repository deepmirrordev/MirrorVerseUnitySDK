// ScanLineRenderPass has two implementations sharing the same Setup contract:
//   * Unity 6 / URP 17+ (#if UNITY_6000_0_OR_NEWER):
//       Overrides RecordRenderGraph with AddUnsafePass. Unsafe passes are exempt
//       from URP's render-pass merger so they don't get folded into the surrounding
//       post-process / final-swapchain pass chain. Inside the pass we get a regular
//       CommandBuffer and use the same Blitter.BlitCameraTexture calls the URP 14
//       legacy path used.
//   * Unity 2022.3 / URP 14 (#else):
//       Allocates an RTHandle in OnCameraSetup and uses CommandBuffer.Blit in
//       Execute() to apply the material then copy back. Mirrors the historical
//       working version.
using MirrorVerse.Options;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace MirrorVerse.UI.RendererFeatures
{
    public class ScanLineRenderPass : ScriptableRenderPass
    {
#if UNITY_6000_0_OR_NEWER
        private RTHandle _cameraColor;
        private RTHandle _tempTarget;
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

#if UNITY_6000_0_OR_NEWER
        private class PassData
        {
            public TextureHandle cameraColor;
            public TextureHandle tempTex;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_options == null || _options.material == null)
            {
                return;
            }

            var cameraData = frameData.Get<UniversalCameraData>();
            // Match legacy behavior: only run on the main camera so secondary cameras
            // (UI, etc.) don't double-apply the effect.
            if (cameraData.camera != Camera.main)
            {
                return;
            }

            var resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle cameraColor = resourceData.cameraColor;

            var desc = renderGraph.GetTextureDesc(cameraColor);
            desc.name = "_ScanLineTemp";
            desc.clearBuffer = false;
            desc.depthBufferBits = 0;
            TextureHandle tempTex = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddUnsafePass<PassData>(
                "ScanLine", out var passData))
            {
                builder.UseTexture(cameraColor, AccessFlags.Read);
                builder.UseTexture(tempTex, AccessFlags.Write);
                builder.AllowPassCulling(false);

                passData.cameraColor = cameraColor;
                passData.tempTex = tempTex;
                passData.material = _options.material;

                builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    // cmd.Blit (legacy) binds the source as _MainTex, which is what
                    // the scanline shader reads. Blitter.BlitCameraTexture would bind
                    // _BlitTexture instead, leaving the shader's _MainTex on its
                    // default "black" sampler.
                    cmd.Blit(data.cameraColor, data.tempTex, data.material, 0);
                });
            }

            // Swap: subsequent passes (and URP's final swapchain blit) read tempTex
            // as the new cameraColor. Avoids a write-back into cameraColor, which is
            // what URP's render-pass merger flags as "Trying to load color backbuffer
            // into a complex RenderPass setup" and what causes the portrait-only
            // dimension mismatch on devices where the swapchain orientation differs
            // from cameraColor's.
            resourceData.cameraColor = tempTex;
        }
#else
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _source = renderingData.cameraData.renderer.cameraColorTarget;
            _destination.Init("_ScanLineTemp");
            cmd.GetTemporaryRT(_destination.id, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera != Camera.main)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.Blit(_source, _destination.Identifier(), _options.material, 0);
            cmd.Blit(_destination.Identifier(), _source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_destination.id);
        }
#endif
    }
}
