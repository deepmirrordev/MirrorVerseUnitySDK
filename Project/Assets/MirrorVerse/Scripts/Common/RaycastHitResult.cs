using UnityEngine;

namespace MirrorVerse
{
    // Enum of raycast hit mode.
    public enum RaycastHitMode
    {
        // Raycast from the screen center to detected plane in the scene. Cursor texture will align with the plane surface.
        Plane = 0,

        // Raycast from the screen center to meshes in the scene. Cursor texture will align with the terrain of the mesh it hit.
        Mesh = 1,
    }

    // A structure carrying necessary information for a raycast hit result.
    public struct RaycastHitResult
    {
        public RaycastHitMode mode;

        public Pose raycastHitPose;

        // TODO: add detected plane polygon vertices
    }
}
