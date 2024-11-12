using MirrorVerse.Options;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
#if UNITY_2022_1_OR_NEWER
using RTH = UnityEngine.Rendering.RTHandle;
#else
using RTH = UnityEngine.Rendering.Universal.RenderTargetHandle;
#endif

namespace MirrorVerse.UI.RendererFeatures
{
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

        internal BlurBackgroundRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        internal void Setup(BlurBackgroundSource source, ScriptableRenderer renderer)
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
            desc.width = src.width >> downsampleFactor; //= width / 2^downsample
            desc.height = src.height >> downsampleFactor;
            cmd.GetTemporaryRT(nameId, desc, FilterMode.Bilinear);
        }

        private int SimplePingPong(int t, int max)
        {
            if (t > max)
            {
                return 2 * max - t;
            }
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

        // Relative blur size to maintain same look across multiple resolution
        private float ScaleWithResolution(float baseRadius, float width, float height)
        {
            float scaleFactor = Mathf.Min(width, height) / 1080f;
            scaleFactor = Mathf.Clamp(scaleFactor, .5f, 2f); //too much variation cause artifact
            return baseRadius * scaleFactor;
        }

        private RenderTargetIdentifier GetRenderTargetBackBuffer(ScriptableRenderer targetRenderer)
        {
            if (_getRenderTargetBackBufferFunc == null)
            {
#if UNITY_2022_1_OR_NEWER
                const string backBufferMethodName = "PeekBackBuffer";
#else
                const string backBufferMethodName = "GetBackBuffer";
#endif
                if (targetRenderer is UniversalRenderer ur)
                {
                    var cbs = ur.GetType()
                                .GetField("m_ColorBufferSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                                .GetValue(ur);
                    var gbb = cbs.GetType()
                                 .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                 .First(m => m.Name == backBufferMethodName && m.GetParameters().Length == 0);

                    _getRenderTargetBackBufferFunc = (Func<RTH>)gbb.CreateDelegate(typeof(Func<RTH>), cbs);
                }
            }

            Debug.Assert(_getRenderTargetBackBufferFunc != null, "Not URP.");

            var r = _getRenderTargetBackBufferFunc.Invoke();
#if UNITY_2022_1_OR_NEWER
            return r.nameID;
#else
            return r.Identifier();
#endif
        }
    }
}
