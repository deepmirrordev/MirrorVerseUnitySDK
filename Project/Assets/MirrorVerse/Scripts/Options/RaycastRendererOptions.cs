using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "RaycastRendererOptions", menuName = "MirrorVerse/Raycast Renderer Options")]
    public class RaycastRendererOptions : ScriptableObject
    {
        // Set the raycast cursor visible or not.
        public bool cursorVisible = true;

        // Set the detected plane visible or not. Useful for debugging.
        public bool detectedPlaneVisible = false;

        // Set the raycast on mesh enabled or not. This requires the mesh is collidable.
        public bool raycastOnMeshEnabled = true;

        // Whether to enable plane detection offered by AR foundation.
        public bool raycastOnPlaneEnabled = true;

        // Prefab of the raycast hit cursor.
        public GameObject cursorPrefab;

        // Prefab of the detected plane.
        public GameObject detectedPlanePrefab;
    }
}
