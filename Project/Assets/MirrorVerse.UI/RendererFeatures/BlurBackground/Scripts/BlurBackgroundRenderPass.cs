// BlurBackgroundRenderPass has two implementations sharing the same class name and
// the same Setup contract, picked at compile time:
//   * Unity 6 / URP 17+ (#if UNITY_6000_0_OR_NEWER):
//       Overrides RecordRenderGraph and uses RasterRenderPass / TextureHandle /
//       UniversalResourceData. The blurred RT is an RTHandle owned by
//       BlurBackgroundSource.
//   * Unity 2022.3 / URP 14 (#else):
//       Overrides Execute and drives the legacy CommandBuffer + ScriptableRenderer
//       APIs (intermediate RTs, BlitProcedural, PeekBackBuffer reflection). The
//       blurred RT is a RenderTexture owned by BlurBackgroundSource.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
#else
using MirrorVerse.Options;
using System.Linq;
using System.Reflection;
using RTH = UnityEngine.Rendering.RTHandle;
#endif

namespace MirrorVerse.UI.RendererFeatures
{
#if UNITY_6000_0_OR_NEWER
    // RenderGraph-based pass for URP 17+. Records a chain of downscale/blur passes
    // into the RenderGraph then composites into the source's RTHandle.
    public class BlurBackgroundRenderPass : ScriptableRenderPass
    {
        const int BLUR_PASS = 0;
        const int CROP_BLUR_PASS = 1;

        static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        static readonly int _Radius  = Shader.PropertyToID("_Radius");

        BlurBackgroundSource _source;

        public BlurBackgroundRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void Setup(BlurBackgroundSource source, ScriptableRenderer renderer)
        {
            // renderer is unused on URP 17 (RenderGraph fetches its own resources via
            // ContextContainer/UniversalResourceData), but kept in the signature so
            // callers don't need a separate Setup overload per platform.
            _source = source;
        }

        private class PassData
        {
            public TextureHandle src;
            public TextureHandle dst;
            public Material material;
            public int passIndex;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_source == null || !_source.enabled || !Application.isPlaying)
                return;

            var options = _source.options;
            var material = options.material;

            material.EnableKeyword("PROCEDURAL_QUAD");

            float radius = ScaleWithResolution(
                options.radius,
                _source.ScreenDimension.x,
                _source.ScreenDimension.y);

            material.SetFloat(_Radius, radius);

            var resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle cameraColor = resourceData.cameraColor;

            TextureHandle blurred = RecordBlurChain(renderGraph, cameraColor, _source, material);
            RecordFinalCopy(renderGraph, blurred, _source.BlurredScreen, material);
        }

        static TextureHandle RecordBlurChain(
            RenderGraph renderGraph,
            TextureHandle cameraColor,
            BlurBackgroundSource source,
            Material material)
        {
            var options = source.options;

            int iteration = Mathf.Max(options.iteration, 1);
            int stepCount = Mathf.Max(iteration * 2 - 1, 1);

            TextureHandle last = cameraColor;

            for (int depth = 0; depth < stepCount; depth++)
            {
                int sizeLevel;
                if (depth == 0)
                {
                    sizeLevel = options.iteration > 0 ? 1 : 0;
                }
                else
                {
                    sizeLevel = SimplePingPong(depth, iteration - 1) + 1;
                    sizeLevel = Mathf.Min(sizeLevel, options.maxDepth);
                }

                int width  = source.ScreenDimension.x  >> sizeLevel;
                int height = source.ScreenDimension.y >> sizeLevel;

                var desc = new TextureDesc(width, height)
                {
                    name = $"BlurStep_{depth}",
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    filterMode = FilterMode.Bilinear,
                    clearBuffer = false
                };

                TextureHandle current = renderGraph.CreateTexture(desc);

                using var builder = renderGraph.AddRasterRenderPass<PassData>(
                    $"Blur Background Step {depth}", out var passData);
                builder.AllowPassCulling(false);
                builder.UseTexture(last, AccessFlags.Read);
                builder.SetRenderAttachment(current, 0, AccessFlags.WriteAll);
                passData.src = last;
                passData.dst = current;
                passData.material = material;
                passData.passIndex = depth == 0 ? CROP_BLUR_PASS : BLUR_PASS;
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    var mpb = new MaterialPropertyBlock();
                    mpb.SetTexture(_MainTex, data.src);
                    ctx.cmd.DrawProcedural(
                        Matrix4x4.identity,
                        data.material,
                        data.passIndex,
                        MeshTopology.Quads,
                        4,
                        1, mpb);
                });

                last = current;
            }

            return last;
        }

        static void RecordFinalCopy(
            RenderGraph renderGraph,
            TextureHandle src,
            RTHandle target,
            Material material)
        {
            TextureHandle targetHandle = renderGraph.ImportTexture(target);

            using var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Blur Background Final Copy", out var passData);

            builder.UseTexture(src, AccessFlags.Read);
            builder.SetRenderAttachment(targetHandle, 0, AccessFlags.WriteAll);
            builder.AllowPassCulling(false);

            passData.src = src;
            passData.dst = targetHandle;
            passData.material = material;
            passData.passIndex = BLUR_PASS;

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                data.material.SetTexture("_MainTex", data.src);
                ctx.cmd.DrawProcedural(
                    Matrix4x4.identity,
                    data.material,
                    data.passIndex,
                    MeshTopology.Quads,
                    4,
                    1);
            });
        }

        static int SimplePingPong(int t, int max)
        {
            if (t > max) return 2 * max - t;
            return t;
        }

        // Relative blur size to maintain same look across multiple resolutions.
        static float ScaleWithResolution(float baseRadius, float width, float height)
        {
            float scaleFactor = Mathf.Min(width, height) / 1080f;
            scaleFactor = Mathf.Clamp(scaleFactor, .5f, 2f);
            return baseRadius * scaleFactor;
        }
    }
#else
    // Legacy ScriptableRenderPass for URP 14. Drives the blur via CommandBuffer +
    // intermediate temp RTs; uses reflection to dig out the back buffer RTHandle
    // from URP's internal ColorBufferSystem.
    public class BlurBackgroundRenderPass : ScriptableRenderPass
    {
        private static readonly int MAIN_TEX_PROP_ID = Shader.PropertyToID("_MainTex");
        private static readonly int RADIUS_PROP_ID = Shader.PropertyToID("_Radius");
        private const int BLUR_PASS = 0;
        private const int CROP_BLUR_PASS = 1;
        private const int MAX_STACK_DEPTH = 32;
        private const string PROFILER_TAG = "Blur Background Image Source";

        private BlurBackgroundSource _source;
        private BlurBackgroundRendererOptions _options;
        private ScriptableRenderer _targetRenderer;

        private int[] _intermediateRT;
        private Func<RTH> _getRenderTargetBackBufferFunc;

        public BlurBackgroundRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void Setup(BlurBackgroundSource source, ScriptableRenderer renderer)
        {
            _targetRenderer = renderer;
            _source = source;
            _options = source.options;
            _options.material.EnableKeyword("PROCEDURAL_QUAD");
            InitIntermediateRT();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(PROFILER_TAG);
            RenderTargetIdentifier renderTarget = GetRenderTargetBackBuffer(_targetRenderer);
            Blur(cmd, renderTarget);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void InitIntermediateRT()
        {
            _intermediateRT = new int[MAX_STACK_DEPTH * 2 - 1];
            for (var i = 0; i < _intermediateRT.Length; i++)
            {
                _intermediateRT[i] = Shader.PropertyToID($"TI_intermediate_rt_{i}");
            }
        }

        private void Blur(CommandBuffer cmd, RenderTargetIdentifier src)
        {
            RenderTexture target = _source.BlurredScreen;
            float radius = ScaleWithResolution(_options.radius, target.width, target.height);
            _options.material.SetFloat(RADIUS_PROP_ID, radius);

            int firstDownsampleFactor = _options.iteration > 0 ? 1 : 0;
            int stepCount = Mathf.Max(_options.iteration * 2 - 1, 1);

            int firstIRT = _intermediateRT[0];
            CreateTempRenderTextureFrom(cmd, firstIRT, target, firstDownsampleFactor);
            BlitProcedural(cmd, src, firstIRT, CROP_BLUR_PASS);

            for (var i = 1; i < stepCount; i++)
            {
                BlurAtDepth(cmd, i, target);
            }

            BlitProcedural(cmd, _intermediateRT[stepCount - 1], target, BLUR_PASS);
            CleanupIntermediateRT(cmd, stepCount);
        }

        private void CreateTempRenderTextureFrom(CommandBuffer cmd, int nameId, RenderTexture src, int downsampleFactor)
        {
            var desc = src.descriptor;
            desc.width = src.width >> downsampleFactor;
            desc.height = src.height >> downsampleFactor;
            cmd.GetTemporaryRT(nameId, desc, FilterMode.Bilinear);
        }

        private static int SimplePingPong(int t, int max)
        {
            if (t > max) return 2 * max - t;
            return t;
        }

        private void BlurAtDepth(CommandBuffer cmd, int depth, RenderTexture baseTexture)
        {
            int sizeLevel = SimplePingPong(depth, _options.iteration - 1) + 1;
            sizeLevel = Mathf.Min(sizeLevel, _options.maxDepth);
            CreateTempRenderTextureFrom(cmd, _intermediateRT[depth], baseTexture, sizeLevel);
            BlitProcedural(cmd, _intermediateRT[depth - 1], _intermediateRT[depth], 0);
        }

        private void BlitProcedural(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier destination, int passIndex)
        {
            cmd.SetGlobalTexture(MAIN_TEX_PROP_ID, src);
            cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1),
                                RenderBufferLoadAction.DontCare,
                                RenderBufferStoreAction.Store,
                                RenderBufferLoadAction.DontCare,
                                RenderBufferStoreAction.DontCare);
            cmd.DrawProcedural(Matrix4x4.identity, _options.material, passIndex, MeshTopology.Quads, 4, 1, null);
        }

        private void CleanupIntermediateRT(CommandBuffer cmd, int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                cmd.ReleaseTemporaryRT(_intermediateRT[i]);
            }
        }

        // Relative blur size to maintain same look across multiple resolutions.
        private static float ScaleWithResolution(float baseRadius, float width, float height)
        {
            float scaleFactor = Mathf.Min(width, height) / 1080f;
            scaleFactor = Mathf.Clamp(scaleFactor, .5f, 2f);
            return baseRadius * scaleFactor;
        }

        private RenderTargetIdentifier GetRenderTargetBackBuffer(ScriptableRenderer targetRenderer)
        {
            if (_getRenderTargetBackBufferFunc == null)
            {
                if (targetRenderer is UniversalRenderer ur)
                {
                    var cbs = ur.GetType()
                                .GetField("m_ColorBufferSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                                .GetValue(ur);
                    var gbb = cbs.GetType()
                                 .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                 .First(m => m.Name == "PeekBackBuffer" && m.GetParameters().Length == 0);

                    _getRenderTargetBackBufferFunc = (Func<RTH>)gbb.CreateDelegate(typeof(Func<RTH>), cbs);
                }
            }

            Debug.Assert(_getRenderTargetBackBufferFunc != null, "Not URP.");

            return _getRenderTargetBackBufferFunc.Invoke().nameID;
        }
    }
#endif
}
