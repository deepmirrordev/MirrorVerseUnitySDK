using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Boxes renderer for detected objects in the scene.
    // The implementation is in experiment and only virtual interface here.
    public class DetectedBoxRenderer : MonoBehaviour
    {
        public virtual void RenderDetectedBoxes(IList<BoundingBox> boxes)
        {
            // Implemented by experimental subclass.
        }

        public virtual int GetCurrentBoxesCount()
        {
            // Implemented by experimental subclass.
            return 0;
        }

        public virtual void ClearAllBoxes()
        {
            // Implemented by experimental subclass.
        }
    }
}
