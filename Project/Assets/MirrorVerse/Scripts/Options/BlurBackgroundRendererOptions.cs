using UnityEngine;

namespace MirrorVerse.Options
{
    public class BlurBackgroundRendererOptions : ScriptableObject
    {
        // Distance between the base texel and the texel to be sampled.
        public float radius = 4;

        // Half the number of time to process the image. It is half because the real number of iteration must alway be even. Using half also make calculation simpler. Non-negative.
        public int iteration = 4;

        // Clamp the minimum size of the intermediate texture. Reduce flickering and blur. Larger than 0.
        public int maxDepth = 6;

        // Material that renders the blurred screen.
        public Material material;
    }
}
