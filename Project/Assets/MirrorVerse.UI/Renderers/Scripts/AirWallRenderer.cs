using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // AirWall renderer to render air wall and cliff edge on AR scene mesh.
    // The implementation is in experiment and only virtual interface here.
    public class AirWallRenderer : MonoBehaviour
    {
        public virtual void RenderAirWall()
        {
            // Implemented by experimental subclass.
        }

        public virtual void ResetRenderer()
        {
            // Implemented by experimental subclass.
        }
    }
}
