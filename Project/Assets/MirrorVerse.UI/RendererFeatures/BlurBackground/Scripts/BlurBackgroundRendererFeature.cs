using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MirrorVerse.UI.RendererFeatures
{
    public class BlurBackgroundRendererFeature : ScriptableRendererFeature
    {
        private BlurBackgroundRenderPass _renderPass;

        private readonly Dictionary<Camera, BlurBackgroundSource> _sourceCache = new Dictionary<Camera, BlurBackgroundSource>();

        public void RegisterSource(BlurBackgroundSource source)
        {
            _sourceCache[source.GetComponent<Camera>()] = source;
        }

        public override void Create()
        {
            _renderPass = new BlurBackgroundRenderPass();
            _sourceCache.Clear();
        }

        void Setup(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            var source = GetSource(cameraData.camera);
            if (source == null)
            {
                return;
            }
            _renderPass.Setup(source, renderer);
        }

#if UNITY_2022_1_OR_NEWER
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            Setup(renderer, renderingData);
        }
#endif

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if !UNITY_2022_1_OR_NEWER
            Setup(renderer, renderingData);
#endif

            var cameraData = renderingData.cameraData;
            var camera = renderingData.cameraData.camera;
            var source = GetSource(camera);
            if (source == null || !source.enabled || !Application.isPlaying)
            {
                return;
            }
            var camPixelSize = cameraData.camera.pixelRect.size;
            source.PrepareBlurredScreen();
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
