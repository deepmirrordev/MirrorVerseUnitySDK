using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Trajectory renderer to visualize the movement of devices during scene streaming.
    // The implementation is in experiment and only virtual interface here.
    public class TrajectoryRenderer : MonoBehaviour
    {
        public virtual void RenderTrajectory(string clientId, Pose[] trajectory, bool isCurrentClient)
        {
            // Implemented by experimental subclass.
        }

        public virtual void ClearAll()
        {
            // Implemented by experimental subclass.
        }
    }
}
