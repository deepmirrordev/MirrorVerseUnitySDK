using MirrorVerse.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Experimental trajectory renderer to visualize the movement of devices during scene streaming.
    public class TrajectoryRenderer : MonoBehaviour
    {
        public TrajectoryRendererOptions options;

        private bool _visible = false;

        private Dictionary<string, GameObject> _allTrajectories = new Dictionary<string, GameObject>();

        // Client ID to color map.
        private Dictionary<string, Color> _colorsMap = new Dictionary<string, Color>();


        private void Start()
        {
            ToggleVisibility(options.visible, true);
        }

        private void Update()
        {
            ToggleVisibility(options.visible);
        }

        public void ToggleVisibility(bool visible, bool force = false)
        {
            if (!force && _visible == visible)
            {
                // No change. Skip.
                return;
            }

            foreach (var trajectoryObject in _allTrajectories.Values)
            {
                trajectoryObject.SetActive(visible);
            }
            _visible = visible;
        }

        private GameObject GetOrCreateTrajectoryGroup(string clientId)
        {
            if (!_allTrajectories.ContainsKey(clientId))
            {
                GameObject trajectoryObj = new GameObject("trajectory_" + clientId);
                trajectoryObj.transform.SetParent(gameObject.transform);
                MeshRenderer meshRenderer = trajectoryObj.AddComponent<MeshRenderer>();
                meshRenderer.material = options.material;
                MeshFilter meshFilter = trajectoryObj.AddComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                {
                    meshFilter.sharedMesh = new Mesh();
                }
                _allTrajectories[clientId] = trajectoryObj;

                trajectoryObj.SetActive(options.visible);
            }
            return _allTrajectories[clientId];
        }

        public void RenderTrajectory(string clientId, Pose[] trajectory, bool isCurrentClient)
        {
            GameObject trajectoryObj = GetOrCreateTrajectoryGroup(clientId);
            MeshFilter meshFilter = trajectoryObj.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            mesh.Clear();

            Vector3[] points = Array.ConvertAll(trajectory, (pose) => { return pose.position; });

            DrawQuad(mesh, options.pointSize, points);

            Color[] colors = new Color[points.Length];
            Array.Fill(colors, GetClientColor(clientId, isCurrentClient));

            if (colors != null)
            {
                Color[] vertexColors = new Color[mesh.vertices.Length];
                int verticesPerPoint = mesh.vertices.Length / points.Length;
                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = 0; j < verticesPerPoint; j++)
                    {
                        vertexColors[verticesPerPoint * i + j] = colors[i];
                    }
                }
                mesh.colors = vertexColors;
            }

            // Upload modifications to the graphics API without waiting.
            mesh.UploadMeshData(false);
        }
        
        private Color GetClientColor(string clientId, bool isCurrentClient)
        {
            if (_colorsMap.TryGetValue(clientId, out Color color))
            {
                // Return already exists client's color.
                return color;
            }

            Color newColor;
            if (isCurrentClient)
            {
                // From current device, use primary color.
                newColor = options.primaryColor;
            }
            else if (options.paletteColors == null || options.paletteColors.Length == 0)
            {
                // If palette is empty, use primary color.
                newColor = options.primaryColor;
            }
            else
            {
                // Otherwise pick a color from the palette.
                int index = _colorsMap.Keys.Count % options.paletteColors.Length;
                newColor = options.paletteColors[index];
            }
            _colorsMap[clientId] = newColor;
            return newColor;
        }

        public void ClearAll()
        {
            foreach (var trajectoryObject in _allTrajectories.Values)
            {
                Destroy(trajectoryObject);
            }
            _allTrajectories.Clear();

            if (_colorsMap != null)
            {
                _colorsMap.Clear();
            }
        }

        private void DrawCube(Mesh mesh, float pointSize, Vector3[] points)
        {
            Vector3[] vertices = new Vector3[8 * points.Length];
            int[] quads = new int[24 * points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vertices[8 * i] = new Vector3(points[i].x + pointSize, points[i].y + pointSize, points[i].z + pointSize);
                vertices[8 * i + 1] = new Vector3(points[i].x - pointSize, points[i].y + pointSize, points[i].z + pointSize);
                vertices[8 * i + 2] = new Vector3(points[i].x - pointSize, points[i].y + pointSize, points[i].z - pointSize);
                vertices[8 * i + 3] = new Vector3(points[i].x + pointSize, points[i].y + pointSize, points[i].z - pointSize);
                vertices[8 * i + 4] = new Vector3(points[i].x + pointSize, points[i].y - pointSize, points[i].z + pointSize);
                vertices[8 * i + 5] = new Vector3(points[i].x - pointSize, points[i].y - pointSize, points[i].z + pointSize);
                vertices[8 * i + 6] = new Vector3(points[i].x - pointSize, points[i].y - pointSize, points[i].z - pointSize);
                vertices[8 * i + 7] = new Vector3(points[i].x + pointSize, points[i].y - pointSize, points[i].z - pointSize);

                quads[24 * i] = 8 * i + 3;
                quads[24 * i + 1] = 8 * i + 2;
                quads[24 * i + 2] = 8 * i + 1;
                quads[24 * i + 3] = 8 * i;

                quads[24 * i + 4] = 8 * i + 7;
                quads[24 * i + 5] = 8 * i + 6;
                quads[24 * i + 6] = 8 * i + 5;
                quads[24 * i + 7] = 8 * i + 4;

                quads[24 * i + 8] = 8 * i + 1;
                quads[24 * i + 9] = 8 * i + 5;
                quads[24 * i + 10] = 8 * i + 4;
                quads[24 * i + 11] = 8 * i;

                quads[24 * i + 12] = 8 * i + 2;
                quads[24 * i + 13] = 8 * i + 6;
                quads[24 * i + 14] = 8 * i + 5;
                quads[24 * i + 15] = 8 * i + 1;

                quads[24 * i + 16] = 8 * i + 3;
                quads[24 * i + 17] = 8 * i + 7;
                quads[24 * i + 18] = 8 * i + 6;
                quads[24 * i + 19] = 8 * i + 2;

                quads[24 * i + 20] = 8 * i;
                quads[24 * i + 21] = 8 * i + 4;
                quads[24 * i + 22] = 8 * i + 7;
                quads[24 * i + 23] = 8 * i + 3;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);
        }

        private void DrawQuad(Mesh mesh, float pointSize, Vector3[] points)
        {
            Vector3[] vertices = new Vector3[4 * points.Length];
            int[] quads = new int[4 * points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vertices[4 * i] = new Vector3(points[i].x + pointSize, points[i].y + pointSize, points[i].z);
                vertices[4 * i + 1] = new Vector3(points[i].x - pointSize, points[i].y + pointSize, points[i].z);
                vertices[4 * i + 2] = new Vector3(points[i].x - pointSize, points[i].y - pointSize, points[i].z);
                vertices[4 * i + 3] = new Vector3(points[i].x + pointSize, points[i].y - pointSize, points[i].z);

                // Reverse normal.
                quads[4 * i] = 4 * i + 3;
                quads[4 * i + 1] = 4 * i + 2;
                quads[4 * i + 2] = 4 * i + 1;
                quads[4 * i + 3] = 4 * i;
            }
            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
