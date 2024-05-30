using UnityEngine;

namespace MirrorVerse
{
    // Static helper class for common access to the MirrorSpace API.
    public static class MirrorSpace
    {
        private static IMirrorSpace _instance;

        // Registers an API implementation insatnce to this helper.
        // Usually this is self registered by implementation itself at Awaken step.
        public static void Register(IMirrorSpace instance)
        {
            if (_instance == null)
            {
                Debug.Log("MirrorSpace singleton has been registered.");
                _instance = instance;
            }
            else
            {
                Debug.LogWarning("Cannot instantiate multiple MirrorSpace implementations.");
            }
        }

        // Unregisters the existing API implemetation.
        // Usually this is self unregistered by implmentation itself at OnDestroy step.
        public static void Unregister(IMirrorSpace instance)
        {
            if (_instance == instance)
            {
                Debug.Log("MirrorSpace singleton has been unregistered.");
                _instance = null;
            }
        }

        // Returns the MirrorSpace API singleton reference,
        // or null if MirrorSpace game object is not loaded in the scene thus the API is not available.
        public static IMirrorSpace Get()
        {
            if (_instance != null)
            {
                return _instance;
            }
            else
            {
                Debug.Log("MirrorSpace implementation is not available.");
                return null;
            }
        }

        // Whether MirrorSpace API is available.
        public static bool IsAvailable()
        {
            return Get() != null;
        }
    }
}
