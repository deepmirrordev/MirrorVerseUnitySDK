using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "ImmediateMeshRendererOptions", menuName = "MirrorVerse/Immediate Mesh Renderer Options")]
    public class ImmediateMeshRendererOptions : ScriptableObject
    {
        // Whether to visualize immediate mesh during scene streaming.
        public bool visible = true;

        // Whether the immediate mesh is collidable.
        public bool collidable = false;

        // List of materials for visualizing immediate mesh during scene streaming.
        public Material[] materials;

        // Layer mask for the immediate mesh. For default immediate mesh, leave it 0.
        // For minimap, set to the same layer of minimap renderer options.
        // Developer can choose an unused number if the default number is occupied.
        public int layer = 0;
    }
}
