using MirrorVerse.Options;
using UnityEngine;

namespace MirrorVerse.UI.RendererFeatures
{
    [RequireComponent(typeof(Camera))]
    public class BlurBackgroundSource : MonoBehaviour
    {
        public BlurBackgroundRendererOptions options;

        private RenderTexture _blurredScreen;
        private Vector2Int _lastScreenDimension = Vector2Int.zero;

        // Blurred effect. For blur background image to use as content.
        public RenderTexture BlurredScreen
        {
            get { return _blurredScreen; }
        }

        private void Awake()
        {
            if (options == null || options.material == null || options.material.shader.name != "MirrorVerse/BlurBackgroundSource")
            {
                Debug.LogError("Invalide shader for blur background source. Disabled the source.");
                enabled = false;
            }
        }

        void Start()
        {
            PrepareBlurredScreen();
        }

        void OnDestroy()
        {
            if (_blurredScreen)
            {
                _blurredScreen.Release();
            }
        }

        public void PrepareBlurredScreen()
        {
            Camera attachedCamera = GetComponent<Camera>();
            Vector2Int screenDimension = Vector2Int.RoundToInt(attachedCamera.pixelRect.size);

            if (_blurredScreen != null && _blurredScreen.IsCreated() && screenDimension == _lastScreenDimension)
            {
                // Blurred screen already existed and same dimention.
                return;
            }

            // Create a new blurred screen.
            if (_blurredScreen)
            {
                _blurredScreen.Release();
            }

            _blurredScreen = new RenderTexture(Mathf.RoundToInt(screenDimension.x), Mathf.RoundToInt(screenDimension.y), 0);
            _blurredScreen.antiAliasing = 1;
            _blurredScreen.useMipMap = false;
            _blurredScreen.name = $"{gameObject.name} BlurBackground";
            _blurredScreen.filterMode = FilterMode.Bilinear;
            _blurredScreen.Create();

            _lastScreenDimension = screenDimension;
        }
    }
}
