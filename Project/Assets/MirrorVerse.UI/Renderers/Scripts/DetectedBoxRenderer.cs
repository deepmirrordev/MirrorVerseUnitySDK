using MirrorVerse.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Experimental boxes renderer for detected objects in the scene.
    public class DetectedBoxRenderer : MonoBehaviour
    {
        public DetectedBoxRendererOptions options;

        private struct BoxData
        {
            public Vector3[] vertices;
            public string label;
            public LineRenderer lineRenderer;
            public TextMesh textMesh;
        }

        private readonly IList<BoxData> _boxes = new List<BoxData>();

        public void RenderDetectedBoxes(IList<BoundingBox> boxes)
        {
            if (!options.visible)
            {
                return;
            }
            ClearAllBoxes();
            foreach (BoundingBox box in boxes)
            {
                AddBoxToRender(box.boxVertices, box.label);
            }
        }

        public void AddBoxToRender(Vector3[] vertices, string label = "")
        {
            long currentTimestamp = RendererUtils.NowMs();
            GameObject boxObject = new GameObject("box_" + currentTimestamp);
            boxObject.transform.SetParent(gameObject.transform);

            LineRenderer lineRenderer = boxObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.sharedMaterial = options.lineMaterial;
            lineRenderer.useWorldSpace = false;

            GameObject labelObject = new GameObject("box_" + label + "_" + currentTimestamp);
            labelObject.transform.SetParent(boxObject.transform);
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.characterSize = 0.02f;
            textMesh.font = options.textFont;
            textMesh.anchor = TextAnchor.UpperLeft;
            MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = options.textFont.material;

            BoxData boxData = new BoxData();
            boxData.label = label;
            boxData.vertices = vertices;
            boxData.lineRenderer = lineRenderer;
            boxData.textMesh = textMesh;
            _boxes.Add(boxData);

            RenderBox(boxData);
        }

        private void RenderBox(BoxData data)
        {
            Vector3[] v = data.vertices;
            /* Order of vertices to form a cube.
                  6 -------- 5
                 /|         /|
                7 -------- 4 .
                | |        | |
                . 2 -------- 1
                |/         |/
                3 -------- 0
            */
            Vector3[] linePositions = new Vector3[]{
                v[0], v[1], v[2], v[3], v[0],
                v[4], v[5], v[6], v[7], v[4],
                v[5], v[1], v[2], v[6], v[7], v[3]};
            data.lineRenderer.positionCount = linePositions.Length;
            data.lineRenderer.SetPositions(linePositions);
            // Assign a color from palette by label hashcode.


            Color[] paletteColors = (options.paletteColors != null) ? options.paletteColors : DetectedBoxRendererOptions.DEFAULT_PALETTE_COLORS;
            Color lineColor = paletteColors[Math.Abs(data.label.GetHashCode()) % paletteColors.Length];
            data.lineRenderer.startColor = lineColor;
            data.lineRenderer.endColor = lineColor;

            // Render text label near to one of the top corners and align with an edge.
            data.textMesh.text = data.label;

            Vector3 upCornor = (v[7].y > v[3].y) ? v[7] : v[3];
            Vector3 rightWards = (v[7].y > v[3].y) ? v[4] - v[7] : v[0] - v[3];

            data.textMesh.transform.position = upCornor;
            data.textMesh.transform.rotation = Quaternion.FromToRotation(Vector3.right, rightWards);
        }

        public int GetCurrentBoxesCount()
        {
            return _boxes.Count;
        }

        public void ClearAllBoxes()
        {
            foreach (BoxData boxData in _boxes)
            {
                Destroy(boxData.textMesh.gameObject);
                Destroy(boxData.lineRenderer.gameObject);
            }
            _boxes.Clear();
        }

        private void OnDestroy()
        {
            ClearAllBoxes();
        }
    }
}
