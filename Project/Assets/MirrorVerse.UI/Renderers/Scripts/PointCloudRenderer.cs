using MirrorVerse.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    // Point cloud renderer visualize the point cloud of real world surroundings during scene streaming.
    public class PointCloudRenderer : MonoBehaviour
    {
        public PointCloudRendererOptions options;

        private GameObject _cameraObject;
        private GameObject _cameraBackgroundMask;
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        private Bounds _displayBounds;

        private float _shaderTimestampSec;
        private float _lastRenderPointTimestamp;
        private Queue<MeshProperties> _queuedPoints;
        private List<MeshProperties> _allPoints;
        private ComputeBuffer _meshPropertiesBuffer;
        private ComputeBuffer _argsBuffer;
        private int _currentBatch;

        // Client ID to color map.
        private Dictionary<string, Color> _colorsMap = new();

        private struct MeshProperties
        {
            public Matrix4x4 transform;
            public Vector4 color;
            public int batch;
            public float enterTime;
            public float exitTime;

            public static int Size()
            {
                return sizeof(float) * 4 * 4  // transform
                     + sizeof(float) * 4      // color
                     + sizeof(int)            // batch
                     + sizeof(float)          // enterTime
                     + sizeof(float);         // exitTime
            }
        }

        private void Start()
        {
            _lastRenderPointTimestamp = -1;
            _currentBatch = 0;

            CreateDisk();

            // Start with no points
            _allPoints = new List<MeshProperties>();
            _queuedPoints = new Queue<MeshProperties>();
            _displayBounds = new Bounds(Vector3.zero, Vector3.one * options.drawBounds);

            options.pointCloudMaterial.SetFloat("_fadeInDuration", options.fadeInDuration);
            options.pointCloudMaterial.SetFloat("_fadeOutDuration", options.fadeOutDuration);
            options.pointCloudMaterial.SetFloat("_displayDistance", options.displayDistance);
        }

        void Update()
        {
            if (!options.visible)
            {
                if (_allPoints.Count > 0)
                {
                    ClearBuffers();
                }
                return;
            }

            _shaderTimestampSec = Shader.GetGlobalVector("_Time").y;
            RefreshBuffers();
            if (_allPoints.Count > 0)
            {
                // Disk will fade to given opacity or transparent, and infate to a given scale.
                options.pointCloudMaterial.SetFloat("_opacity", options.opacity);
                options.pointCloudMaterial.SetFloat("_inflationScale", options.inflationScale);
                
                AilgnCameraRotation();

                Graphics.DrawMeshInstancedIndirect(_mesh, 0, options.pointCloudMaterial, _displayBounds, _argsBuffer);

                MaybeToggleCameraMask();
            }
        }

        void OnDisable()
        {
            ClearBuffers();
        }

        public void SetCameraObject(GameObject cameraObject)
        {
            _cameraObject = cameraObject;
        }

        public void RenderPoints(string clientId, Vector3[] points, bool isCurrentClient)
        {
            Color[] colors = new Color[points.Length];
            Array.Fill(colors, GetClientColor(clientId, isCurrentClient));

            QueuePoints(points, colors);
            TrimPoints();
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


        public void GetPointCloudStats(out int visible, out int queued, out int all)
        {
            // TODO: Find a way to display these debugging values.
            visible = _meshPropertiesBuffer != null ? _meshPropertiesBuffer.count : 0;
            queued = _queuedPoints != null ? _queuedPoints.Count : 0;
            all = _allPoints != null ? _allPoints.Count : 0;
        }

        public void SetNewBatch()
        {
            // New batch may come with new incompatiple data.
            _currentBatch++;
            // Remove all existing queued points, and enqueue.
            _queuedPoints.Clear();
        }

        public void ClearBuffers()
        {
            if (_meshPropertiesBuffer != null)
            {
                _meshPropertiesBuffer.Release();
            }
            _meshPropertiesBuffer = null;

            if (_argsBuffer != null)
            {
                _argsBuffer.Release();
            }
            _argsBuffer = null;

            _allPoints.Clear();
            _queuedPoints.Clear();

            if (_cameraBackgroundMask != null)
            {
                _cameraBackgroundMask.SetActive(false);
            }

            if (_colorsMap != null)
            {
                _colorsMap.Clear();
            }
        }

        private void MaybeToggleCameraMask()
        {
            if (_cameraBackgroundMask == null)
            {
                if (_cameraObject != null && options.cameraBackgroundMaskPrefab != null)
                {
                    // Initiate mask
                    _cameraBackgroundMask = Instantiate(options.cameraBackgroundMaskPrefab, _cameraObject.transform);
                }
            }
            if (_cameraBackgroundMask != null && _cameraBackgroundMask.activeSelf != options.transparentMode)
            {
                _cameraBackgroundMask.SetActive(options.transparentMode);
            }
        }

        private void RefreshBuffers()
        {
            // Pop points from queue.
            List<MeshProperties> meshPropertiesToDraw = MaybeRenderPointsFromQueue();

            // Create buffers.
            if (_meshPropertiesBuffer != null)
            {
                _meshPropertiesBuffer.Release();
            }
            // Only flush material if there are points to draw.
            if (meshPropertiesToDraw.Count > 0)
            {
                _meshPropertiesBuffer = new ComputeBuffer(meshPropertiesToDraw.Count, MeshProperties.Size());
                _meshPropertiesBuffer.SetData(meshPropertiesToDraw.ToArray());
                options.pointCloudMaterial.SetBuffer("_properties", _meshPropertiesBuffer);

                uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
                args[0] = (uint)_mesh.GetIndexCount(0);
                args[1] = (uint)meshPropertiesToDraw.Count;
                args[2] = (uint)_mesh.GetIndexStart(0);
                args[3] = (uint)_mesh.GetBaseVertex(0);
                if (_argsBuffer != null)
                {
                    _argsBuffer.Release();
                }
                _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                _argsBuffer.SetData(args);
            }
        }

        private void TrimPoints()
        {
            // Clean old points those already faded out.
            _allPoints.RemoveAll((point) =>
            {
                // Remove faded out points (has exit time longer than a given duration). 
                return point.exitTime > 0 && _shaderTimestampSec - point.exitTime > options.fadeOutDuration;
            });

            // TODO: Also merge nearby points.
        }

        private void QueuePoints(Vector3[] points, Color[] colors)
        {
            // Put points into a temporary new queue.
            List<MeshProperties> newQueue = new List<MeshProperties>();
            for (int i = 0; i < points.Length; i++)
            {
                newQueue.Add(GenerateMeshProperties(points[i], colors[i]));
            }

            // Enqueue points to the top of the queue so they can be emitted first.
            newQueue.AddRange(_queuedPoints);
            _queuedPoints = new Queue<MeshProperties>(newQueue);
        }

        private float GetPointPerSec()
        {
            if (_queuedPoints.Count / options.pointPerSec > 2.0)
            {
                return options.peekPointPerSec;
            }
            else
            {
                return options.pointPerSec;
            }
        }

        private MeshProperties GenerateMeshProperties(Vector3 position, Color color, float enterTime = 0, float exitTime = 0)
        {
            MeshProperties instance = new MeshProperties();
            instance.transform = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            instance.color = color;
            instance.batch = _currentBatch;
            instance.enterTime = enterTime;
            instance.exitTime = exitTime;
            return instance;
        }

        private List<MeshProperties> MaybeRenderPointsFromQueue()
        {
            List<MeshProperties> pointsToDraw = new List<MeshProperties>();
            // Find points to emit from queue.
            List<MeshProperties> pointsToEmit = new List<MeshProperties>();
            if (_queuedPoints.Count > 0)
            {
                float currentTimestamp = Time.fixedTime;
                float deltaTimestamp = 1.0f / GetPointPerSec();
                int pointsPerFrame = (int)Math.Round(Time.fixedDeltaTime / deltaTimestamp);

                // Determine whether to emit N points per frame, or 1 point every N frames.
                if (pointsPerFrame > 1)
                {
                    // N points every 1 frame.
                    for (int i = 0; i < pointsPerFrame; i++)
                    {
                        if (_queuedPoints.Count == 0)
                        {
                            break;
                        }
                        MeshProperties point = _queuedPoints.Dequeue();
                        point.enterTime = _shaderTimestampSec;
                        pointsToEmit.Add(point);
                    }
                }
                else
                {
                    // 1 point every N frames.
                    if (currentTimestamp - _lastRenderPointTimestamp > deltaTimestamp)
                    {
                        MeshProperties point = _queuedPoints.Dequeue();
                        point.enterTime = _shaderTimestampSec;
                        pointsToEmit.Add(point);
                        _lastRenderPointTimestamp = currentTimestamp;
                    }
                }
            }

            // Add to all points
            // Note newly added points has no exit time and won't fade.
            _allPoints.AddRange(pointsToEmit);

            int fading = 0;
            // Filter and adjust existing points and push to draw.
            for (int i = 0; i < _allPoints.Count; i++)
            {
                MeshProperties point = _allPoints[i];
                // Fade out old points which has not yet fading.
                if (point.batch < _currentBatch && point.exitTime <= 0)
                {
                    // Set exit time to start fading out.
                    // If still entering, then delay the exit time.
                    point.exitTime = Math.Max(_shaderTimestampSec, point.enterTime + options.fadeInDuration);
                    fading++;
                }
                else
                {
                    // Note that if the point is in current batch or already fading, do nothing.
                }
                _allPoints[i] = point;

                // Only push to draw if within distance.
                float dist = 0;
                if (_cameraObject != null)
                {
                    dist = Vector3.Distance(_cameraObject.transform.position, point.transform.GetPosition());
                }
                if (dist < options.displayDistance)
                {
                    pointsToDraw.Add(point);
                }
            }
            return pointsToDraw;
        }

        private void AilgnCameraRotation()
        {
            if (_cameraObject == null)
            {
                return;
            }
            Vector3[] instancedVertices = new Vector3[options.diskSides];
            for (int i = 0; i < options.diskSides; i++)
            {
                instancedVertices[i] = _cameraObject.transform.rotation * _meshVertices[i];
            }
            _mesh.vertices = instancedVertices;
            options.pointCloudMaterial.SetVector("_cameraPosition", _cameraObject.transform.position);
        }

        private float GetPointSize()
        {
            return options.pointSize;
        }
        
        private void CreateDisk()
        {
            Vector3[] vertices = GetCircumferencePoints(options.diskSides, GetPointSize(), Vector3.zero).ToArray();
            int[] triangles = DrawFilledTriangles(options.diskSides, 0).ToArray();
            Vector3[] normals = new Vector3[options.diskSides];
            Array.Fill(normals, -Vector3.forward);

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;

            _meshVertices = vertices;
            _mesh = mesh;
        }

        private List<Vector3> GetCircumferencePoints(int sides, float radius, Vector3 center)
        {
            List<Vector3> points = new List<Vector3>(sides);
            float circumferenceProgressPerStep = (float)1 / sides;
            float TAU = 2 * Mathf.PI;
            float radianProgressPerStep = circumferenceProgressPerStep * TAU;

            for (int i = 0; i < sides; i++)
            {
                float currentRadian = radianProgressPerStep * i;
                points.Add(center + new Vector3(Mathf.Cos(currentRadian) * radius, Mathf.Sin(currentRadian) * radius, 0));
            }
            return points;
        }

        private List<int> DrawFilledTriangles(int diskPointsCount, int baseIndex)
        {
            int triangleAmount = diskPointsCount - 2;
            List<int> newTriangles = new List<int>(3 * triangleAmount);
            for (int i = 0; i < triangleAmount; i++)
            {
                newTriangles.Add(baseIndex);
                newTriangles.Add(baseIndex + i + 2);
                newTriangles.Add(baseIndex + i + 1);
            }
            return newTriangles;
        }
    }
}