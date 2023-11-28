using System;
using UnityEngine;

namespace MirrorVerse
{
    [Serializable]
    // Represends a bounding box with label.
    public struct BoundingBox
    {
        // Category label of the bound object, e.g.  'chair', 'table', 'sofa' ...
        public string label;

        /*
            6 -------- 5
           /|         /|
          7 -------- 4 .
          | |        | |
          . 2 -------- 1
          |/         |/
          3 -------- 0
        X right, Y upward, Z backward
        8 box vertices, order shown in figure above.
        */
        public Vector3[] boxVertices;

        // Confidence of the box prediction. [0, 1]
        public double score;
    }
}
