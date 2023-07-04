using System;
using UnityEngine;

namespace MirrorVerse
{
    // Tag component that is used to track whether QR code marker panel is visble.
    public class MarkerTag : MonoBehaviour
    {
        public static Action<bool, GameObject> onMakerVisibilityChange;

        public void OnEnable()
        {
            onMakerVisibilityChange?.Invoke(true, gameObject);
        }

        void OnDisable()
        {
            onMakerVisibilityChange?.Invoke(false, null);
        }
    }
}
