using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "AirWallRendererOptions", menuName = "MirrorVerse/Air Wall Renderer Options")]
    public class AirWallRendererOptions : ScriptableObject
    {
        // True to visulize air wall.
        public bool airWallVisible = false;

        // True to visulize cliff edge.
        public bool cliffEdgeVisible = false;

        // True if only for visual effect without real collision.
        public bool visualEffectOnly = false;

        // Cliff edge detection interval density
        public float detectIntervalDistance;
        
        // Max height for downward projection. If mesh surface is lower than this value, skip for air wall.
        public float projectionHeight;

        // Materials for the air wall visuals.
        public Material airWallMaterial;
        public Material airWallLineMaterial;
        
        // Air wall size for collision.
        public Vector2 airWallSize = new Vector2(0.01f, 1);
        public float airWallBareSize = 0.05f;
        
        // Air wall visual effect params.
        public float airWallEffectMaxDistance = 0.3f;
        public float airWallEffectEmissionIntensity = 1;

        // Whether to use nav mesh as cliff edge calculation.
        // If false it will use static mesh instead.
        public bool useNavMesh = false;
    }
}
