using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "ScanLineRendererOptions", menuName = "MirrorVerse/Scan Line Renderer Options")]
    public class ScanLineRendererOptions : ScriptableObject
    {
        // Whether to enable scanline effect.
        public bool enabled = false;

        // Whether to loop the effect forever.
        public bool loop = true;
    }
}
