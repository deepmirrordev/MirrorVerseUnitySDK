using UnityEngine;
using UnityEngine.UI;

namespace MirrorVerse.UI.RendererFeatures
{
    public class BlurBackgroundImage : Image
    {
        private static readonly int BLUR_TEX_PROP_ID = Shader.PropertyToID("_BlurTex");

        private BlurBackgroundSource _source;

        protected override void Start()
        {
            base.Start();

            AutoAcquireSource();

            if (_source == null || !_source.enabled)
            {
                return;
            }

            if (material && material.shader.name == "MirrorVerse/BlurBackgroundImage")
            {
                material.SetTexture(BLUR_TEX_PROP_ID, _source.BlurredScreen);
            }
            else
            {
                Debug.LogWarning("Blur Background Image Source or material not found.");
            }

            if (canvas)
            {
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            }
        }

        bool sourceAcquiredOnStart = false;

        private void AutoAcquireSource()
        {
            if (sourceAcquiredOnStart)
            {
                return;
            }

            _source = _source ? _source : FindFirstObjectByType<BlurBackgroundSource>();
            sourceAcquiredOnStart = true;
        }

        private bool Validate()
        {
            if (!IsActive() || !material)
            {
                return false;
            }

            if (!_source || !_source.enabled)
            {
                return false;
            }

            if (!_source.BlurredScreen)
            {
                return false;
            }

            return true;
        }

        private void LateUpdate()
        {
            if (!Validate())
            {
                return;
            }
            materialForRendering.SetTexture(BLUR_TEX_PROP_ID, _source.BlurredScreen);
        }
    }
}
