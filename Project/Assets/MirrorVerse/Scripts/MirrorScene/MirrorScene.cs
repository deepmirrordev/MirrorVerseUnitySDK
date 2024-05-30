using UnityEngine;

namespace MirrorVerse
{
    // Static helper class for common access to the MirrorScene API.
    public static class MirrorScene
    {
        private static IMirrorScene _instance;

        // Registers an API implementation insatnce to this helper.
        // Usually this is self registered by implementation itself at Awaken step.
        public static void Register(IMirrorScene instance) {
            if (_instance == null)
            {
                Debug.Log("MirrorScene singleton has been registered.");
                _instance = instance;
            }
            else
            {
                Debug.LogWarning("Cannot instantiate multiple MirrorScene implementations.");
            }
        }

        // Unregisters the existing API implemetation.
        // Usually this is self unregistered by implmentation itself at OnDestroy step.
        public static void Unregister(IMirrorScene instance)
        {
            if (_instance == instance)
            {
                Debug.Log("MirrorScene singleton has been unregistered.");
                _instance = null;
            }
        }

        // Returns the MirrorScene API singleton reference,
        // or null if MirrorScene game object is not loaded in the scene thus the API is not available.
        public static IMirrorScene Get()
        {
            if (_instance != null)
            {
                return _instance;
            }
            else
            {
                Debug.Log("MirrorScene implementation is not available.");
                return null;
            }
        }

        // Whether MirrorScene API is available.
        public static bool IsAvailable()
        {
            return Get() != null;
        }
    }
}
