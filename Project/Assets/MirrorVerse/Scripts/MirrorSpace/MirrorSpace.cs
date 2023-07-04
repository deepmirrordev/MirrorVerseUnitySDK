using UnityEngine;

namespace MirrorVerse
{
    // Static helper class for common access to the MirrorSpace API.
    public static class MirrorSpace
    {
        // Reference of MirrorSpace API singleton instance.
        private static IMirrorSpace _instance;

        // Whether the API has been initialized.
        private static bool _initialized;

        // Returns the MirrorSpace API singleton reference,
        // or null if MirrorSpace game object is not active thus the API is not available.
        public static IMirrorSpace Get()
        {
            if (_instance == null && !_initialized)
            {
                // Only examine once.
                _initialized = true;

                System.Type type = System.Type.GetType("Dm.Core.MirrorSpaceImpl", false);
                if (type != null)
                {
                    Object obj = Object.FindObjectOfType(type);
                    if (obj != null)
                    {
                        _instance = (IMirrorSpace)obj;
                        return _instance;
                    }
                    else
                    {
                        Debug.LogError($"MirrorSpace implementation object not found.");
                    }
                }
                else
                {
                    Debug.LogError($"MirrorSpace implementation type cannot be loaded.");
                }
            }
            return _instance;
        }

        // Whether MirrorSpace API is available.
        public static bool IsAvailable()
        {
            return Get() != null;
        }
    }
}
