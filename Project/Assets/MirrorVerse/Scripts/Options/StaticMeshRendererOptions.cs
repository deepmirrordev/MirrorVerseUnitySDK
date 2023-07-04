using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "StaticMeshRendererOptions", menuName = "MirrorVerse/Static Mesh Renderer Options")]
    public class StaticMeshRendererOptions : ScriptableObject
    {
        // Whether the static mesh is visible.
        public bool visible = true;

        // Whether the static mesh is collidable.
        public bool collidable = true;

        // Whether the static mesh is transparent with occlusion.
        public bool withOcclusion = true;

        // Whether the static mesh can cast shadow.
        public bool castsShadow = false;

        // Whether the static mesh can receive shadow.
        public bool receivesShadow = true;

        // List of materials to visualize default mesh without occlusion.
        public Material[] defaultMaterials;

        // Material to visualize textured mesh.
        public Material textureMaterial;

        // Material to make the mesh transparent with occlusion. 
        public Material occlusionMaterial;

        // Material to make the mesh transparent with occlusion and casting shadows.
        public Material occlusionShadowCasterMaterial;

        // Material to make the mesh transparent and receving shadows. Usually work with occlusion materials.
        public Material shadowReceiverMaterial;
    }
}
