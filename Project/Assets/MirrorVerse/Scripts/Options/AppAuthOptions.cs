using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "AppAuthOptions", menuName = "MirrorVerse/App Auth Options")]
    public class AppAuthOptions : ScriptableObject
    {
        // API key
        public string appKey;

        // API secret
        public string appSecret;

        // Cloud service endpoint label.
        public string endpoint = "US";
    }
}
