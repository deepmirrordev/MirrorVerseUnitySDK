using MirrorVerse.Options;
using UnityEngine;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#endif

namespace MirrorVerse.UI.RendererFeatures
{
    // Holds the off-screen blurred snapshot the BlurBackgroundImage shaders sample
    // from. The backing storage type differs between Unity versions:
    //   * Unity 6 / URP 17: RTHandle, allocated via RTHandles.Alloc, fed into the
    //     RenderGraph-based BlurBackgroundRendererFeature.
    //   * Unity 2022.3 / URP 14: plain RenderTexture, written to by
    //     BlurBackgroundRenderPass via the legacy ScriptableRenderPass API.
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class BlurBackgroundSource : MonoBehaviour
    {
        public BlurBackgroundRendererOptions options;

        private Vector2Int _lastScreenDimension = Vector2Int.zero;

#if UNITY_6000_0_OR_NEWER
        private RTHandle _blurredScreen;

        public RTHandle BlurredScreen { get { return _blurredScreen; } }

        public Vector2Int ScreenDimension { get { return _lastScreenDimension; } }
#else
        private RenderTexture _blurredScreen;

        public RenderTexture BlurredScreen { get { return _blurredScreen; } }
#endif

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
#if UNITY_6000_0_OR_NEWER
            if (_blurredScreen != null)
            {
                _blurredScreen.Release();
                _blurredScreen = null;
            }
#else
            if (_blurredScreen)
            {
                _blurredScreen.Release();
            }
#endif
        }

        public void PrepareBlurredScreen()
        {
            Camera attachedCamera = GetComponent<Camera>();
            Vector2Int screenDimension =
                Vector2Int.RoundToInt(attachedCamera.pixelRect.size);

#if UNITY_6000_0_OR_NEWER
            // Skip if size hasn't changed.
            if (_blurredScreen != null &&
                screenDimension == _lastScreenDimension)
            {
                return;
            }

            // Release old handle.
            if (_blurredScreen != null)
            {
                _blurredScreen.Release();
                _blurredScreen = null;
            }

            // Allocate the RTHandle the RenderGraph blur chain will write into.
            _blurredScreen = RTHandles.Alloc(
                screenDimension.x,
                screenDimension.y,
                depthBufferBits: 0,
                colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
                filterMode: FilterMode.Bilinear,
                name: $"{gameObject.name}_BlurBackground"
            );

            _lastScreenDimension = screenDimension;
#else
            if (_blurredScreen != null && _blurredScreen.IsCreated() && screenDimension == _lastScreenDimension)
            {
                // Blurred screen already exists and same dimension.
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
#endif
        }
    }
}
