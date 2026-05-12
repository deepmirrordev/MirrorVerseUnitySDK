using UnityEngine;

namespace MirrorVerse
{
    /// <summary>
    /// This component sets an ECEF location as world root base offset on the MirrorSpace API.
    /// All game objects under this world root object will be located relative to this world root.
    /// It's a useful component if developer want to offset an entire object tree to a specific absolute location on Earth.
    /// Note: Only one world root instance per scene. TODO: Add singleton restriction check.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class SpaceWorldRoot : MonoBehaviour
    {
        public Ecef3d worldRootEcef = Ecef3d.zero;

        private void Start()
        {
            if (!MirrorSpace.IsAvailable())
            {
                Debug.LogWarning("MirrorSpace API is not available. SpaceWorldRoot offset base will not be applied.");
                return;
            }

            Debug.Log($"World root is set, automatically sets coordinates offset base to ECEF: {worldRootEcef}");
            MirrorSpace.Get().SetCoordinatesOffsetBase(worldRootEcef);
        }
    }
}
