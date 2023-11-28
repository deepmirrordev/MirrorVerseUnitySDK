using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "MinimapRendererOptions", menuName = "MirrorVerse/Minimap Renderer Options")]
    public class MinimapRendererOptions : ScriptableObject
    {
        // Whether to enable this mini map.
        public bool enabled = false;

        // Layer for minimap camera.
        // Keep it the same as the layer of the minimap immediate mesh renderer.
        public int minimapLayer = 20;

        // Prefab of the device model.
        public GameObject deviceModelPrefab;

        // Background color.
        public Color backgroundColor = new Color32(128, 128, 128, 48);
    }
}
