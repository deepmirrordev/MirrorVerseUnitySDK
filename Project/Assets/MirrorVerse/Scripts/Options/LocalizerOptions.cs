using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "LocalizerOptions", menuName = "MirrorVerse/Localizer Options")]
    public class LocalizerOptions : ScriptableObject
    {
        // Gets camera image for localization every given interval in seconds.
        public float localizationImageInterval = 0.2f;  // 200ms

        // If set, use edge map and start from the preloaded given map Id.
        public string startupEdgeMapId = string.Empty;

        // If set, override default with the requested global descriptor model name located in cache or cloud.
        public string globalDescriptorModel = string.Empty;

        // If set, override default with the requested local feature model name located in cache or cloud.
        public string localFeatureModel = string.Empty;

        // If set, override default with the requested feature matcher model name located in cache or cloud.
        public string featureMatcherModel = string.Empty;

        public bool modelForceOverride = false;
    }
}
