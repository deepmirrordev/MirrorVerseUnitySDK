using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "NavMeshRendererOptions", menuName = "MirrorVerse/Nav Mesh Renderer Options")]
    public class NavMeshRendererOptions : ScriptableObject
    {
        // Whether navigation mesh is visible.
        public bool visible = false;

        // Whether navigation mesh is collidable.
        public bool collidable = false;

        // List of materials to visualize the nav mesh.
        public Material[] defaultMaterials;
    }
}
