using UnityEngine;

namespace MirrorVerse
{
    // Static helper class for common access to the MirrorScene API.
    public static class MirrorScene
    {
        // Reference of MirrorScene API singleton instance.
        private static IMirrorScene _instance;

        // Whether the API has been initialized.
        private static bool _initialized;

        // Returns the MirrorScene API singleton reference,
        // or null if MirrorScene game object is not active thus the API is not available.
        public static IMirrorScene Get()
        {
            if (_instance == null && !_initialized)
            {
                // Only examine once.
                _initialized = true;

                System.Type type = System.Type.GetType("Dm.Core.MirrorSceneImpl", false);
                if (type != null )
                {
                    Object obj = Object.FindObjectOfType(type);
                    if (obj != null)
                    {
                        _instance = (IMirrorScene)obj;
                        return _instance;
                    }
                    else
                    {
                        Debug.LogError($"MirrorScene implementation object not found.");
                    }
                }
                else
                {
                    Debug.LogError($"MirrorScene implementation type cannot be loaded.");
                }
            }
            return _instance;
        }

        // Whether MirrorScene API is available.
        public static bool IsAvailable()
        {
            return Get() != null;
        }
    }
}
