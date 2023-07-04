using UnityEngine;
using UnityEngine.UI;

namespace MirrorVerse.UI.Renderers
{
    // Marker renderer calculates and renders a QRCode marker on screen canvas.
    public class MarkerRenderer : MonoBehaviour
    {
        public Image qrCodeImage;
        public MarkerTag panel;

        public bool RenderQrCodeImage(MarkerRenderable markerRenderable)
        {
            // The size will be overwritten by actual image from bytes.
            // Disable mipChain so that the marker texture is not affected by global Quality settings.

            Texture2D qrCodeTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (qrCodeTexture.LoadImage(markerRenderable.imageBytes))
            {
                gameObject.SetActive(true);

                qrCodeTexture.filterMode = FilterMode.Point;
                qrCodeImage.sprite = Sprite.Create(qrCodeTexture,
                    new Rect(0, 0, qrCodeTexture.width, qrCodeTexture.height),
                    new Vector2(qrCodeTexture.width / 2, qrCodeTexture.height / 2));
                RectTransform qrCodeImageRect = qrCodeImage.gameObject.GetComponent<RectTransform>();
                qrCodeImageRect.sizeDelta = markerRenderable.screenSize;
                return true;
            }
            // Invalid image bytes.
            Debug.Log($"Cannot load QR code image bytes.");
            HideQrCodeImage();
            return false;
        }

        public void HideQrCodeImage()
        {
            gameObject.SetActive(false);
        }
    }
}
