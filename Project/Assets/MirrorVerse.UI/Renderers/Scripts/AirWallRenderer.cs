using MirrorVerse.Options;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MirrorVerse.UI.Renderers
{
    // Experimental AirWall renderer.
    public class AirWallRenderer : MonoBehaviour
    {
        public AirWallRendererOptions options;

        // Currently air wall and cliff edge require meshes and ray cast to work together.
        // Consider to decouple this in the future.
        public StaticMeshRenderer staticMeshRenderer;
        public NavMeshRenderer navMeshRenderer;
        public RaycastRenderer rayCastRenderer;

        private GameObject _airWallRootObject;
        private GameObject _airWallSubMeshObject;
        private GameObject _cliffEdgeRootObject;
        private MeshCollider _groundMeshCollider;

        private GameObject _agentObject;
        private bool _airWallVisible = false;
        private bool _cliffEdgeVisible = false;
        private bool _requireRerender = false;

        private void OnEnable()
        {
            AirWallAgentTag.onAirWallAgentUpdated += RequireAirWallUpdate;
        }

        private void OnDisable()
        {
            AirWallAgentTag.onAirWallAgentUpdated -= RequireAirWallUpdate;
        }

        private void RequireAirWallUpdate()
        {
            // Agent updated. Rerender if necessary.
            GameObject newAgent = GetAirWallAgent();
            if (newAgent != _agentObject)
            {
                _agentObject = newAgent;
                _requireRerender = true;
            }
        }

        private void Update()
        {
            ToggleAirWallVisibility(options.airWallVisible);
            ToggleCliffEdgeVisibility(options.cliffEdgeVisible);

            if (_requireRerender)
            {
                Debug.Log($"Airwall rerender triggered.");
                RenderAirWall();
                _requireRerender = false;
            }
        }

        private void ToggleAirWallVisibility(bool visible)
        {
            _airWallVisible = visible;
            if (_airWallRootObject == null)
            {
                return;
            }
            if (_airWallRootObject.activeSelf == visible)
            {
                // No change. Skip.
                return;
            }
            _airWallRootObject.SetActive(visible);
            _requireRerender = true;
        }

        private void ToggleCliffEdgeVisibility(bool visible)
        {
            _cliffEdgeVisible = visible;
            if (_cliffEdgeRootObject == null)
            {
                return;
            }
            if (_cliffEdgeRootObject.activeSelf == visible)
            {
                // No change. Skip.
                return;
            }
            _cliffEdgeRootObject.SetActive(visible);
            _requireRerender = true;
        }

        private Mesh GetGroundMesh()
        {
            // Ground mesh is holding the agent's navigation. Could be null if AR Session is not finished yet.
            // Allow developer to use options to choose which mesh to use as ground. Nav mesh or static mesh.
            GameObject groundMeshObject = null;
            if (options.useNavMesh)
            {
                if (navMeshRenderer != null)
                {
                    groundMeshObject = navMeshRenderer.GetMeshObject();
                }
            }
            else
            {
                if (staticMeshRenderer != null)
                {
                    groundMeshObject = staticMeshRenderer.GetMeshObject();
                }
            }

            if (groundMeshObject != null)
            {
                MeshFilter meshFilter = groundMeshObject.GetComponent<MeshFilter>();
                return meshFilter.mesh;
            }
            return null;
        }

        private Vector3? GetFocusPoint()
        {
            Vector3? focusPoint = null;

            // First of all check whether there is an agent.
            if (_agentObject == null)
            {
                _agentObject = GetAirWallAgent();
            }

            // If agent exists and active, use agent's position as target point to select mesh surface.
            if (_agentObject != null && _agentObject.activeSelf)
            {
                focusPoint = _agentObject.transform.position;
            }
            else
            {
                if (rayCastRenderer != null && rayCastRenderer.GetCursorHitMode().HasValue && rayCastRenderer.GetCursorHitMode().Value == RaycastHitMode.Mesh)
                {
                    // Only hit result is on a mesh with cursor value
                    focusPoint = rayCastRenderer.GetCursorPose().Value.position;
                }
            }
            return focusPoint;
        }

        public GameObject GetAirWallAgent()
        {
            if (AirWallAgentTag.airWallAgentTags.Count > 0)
            {
                // TODO: Right now this implementation only support air wall rendering for one current agent.
                GameObject agentObject = AirWallAgentTag.airWallAgentTags[0].gameObject;
                if (agentObject.activeSelf)
                {
                    return agentObject;
                }
            }
            return null;
        }

        public void RenderAirWall()
        {
            // Create and render both Air wall and cliff edge mesh given agent's position.
            if (_agentObject == null)
            {
                _agentObject = GetAirWallAgent();
            }
            Vector3? focusPoint = GetFocusPoint();

            // If agent exists, use agent's position to select mesh surface to render air wall and cliff edge.
            if (focusPoint.HasValue)
            {
                // Create cliff edge of the surface that the agent object's current position is at.
                if (options.airWallVisible || options.cliffEdgeVisible)
                {
                    InitCliffEdgeMeshObject();
                    List<Vector3> recordPointList = ChkCliffEdgeAll(focusPoint.Value);
                    
                    if (options.cliffEdgeVisible)
                    {
                        ShowCliffEdge(recordPointList);
                    }

                    if (options.airWallVisible)
                    {
                        // Create air wall meshes and show at the given agent's current transform.
                        InitAirWallMeshObject();
                        TraverseEdgePath(recordPointList, CreateAirWallSubMesh);
                        CombineAirWallSubMeshes();
                        ShowAirWall(_agentObject);
                        ClearAirWallSubMeshObject();
                    }
                }
            }
        }

        public void RenderCliffEdge()
        {
            // Create and render cliff edge mesh given a target position.
            // Target position could be agent's position, or input position. If none of the case, use current cursor position.
            Vector3? focusPoint = GetFocusPoint();
            if (focusPoint.HasValue)
            {
                // Create cliff edge of the surface with the target position if given.
                InitCliffEdgeMeshObject();
                List<Vector3> recordPointList = ChkCliffEdgeAll(focusPoint.Value);
                ShowCliffEdge(recordPointList);
            }
        }

        public void ResetRenderer()
        {
            ClearAirWallObject();
            ClearAirWallSubMeshObject();
            ClearCliffEdgeObject();
        }

        private void InitAirWallMeshObject()
        {
            if (_airWallRootObject != null)
            {
                ClearAirWallObject();
            }
            long currentTimestamp = RendererUtils.NowMs();
            _airWallRootObject = new GameObject("airWall_" + currentTimestamp);
            _airWallSubMeshObject = new GameObject("airWallNotCombine_" + currentTimestamp);
            _airWallRootObject.transform.SetParent(gameObject.transform);
            _airWallSubMeshObject.transform.SetParent(gameObject.transform);
            Debug.Log($"Initialize a new air wall object: {_airWallRootObject.name}.");
        }

        private void InitCliffEdgeMeshObject()
        {
            if (_cliffEdgeRootObject != null)
            {
                ClearCliffEdgeObject();
            }
            long currentTimestamp = RendererUtils.NowMs();
            _cliffEdgeRootObject = new GameObject("airWallCliffEdge_" + currentTimestamp);
            _cliffEdgeRootObject.transform.SetParent(gameObject.transform);
            Debug.Log($"Initialize a new cliff edge object: {_cliffEdgeRootObject.name}.");

            _groundMeshCollider = _cliffEdgeRootObject.AddComponent<MeshCollider>();
            _groundMeshCollider.sharedMesh = GetGroundMesh();
            Debug.Log($"Initialize a collider attached to cliff edge object: {_cliffEdgeRootObject.name}.");
        }

        private void ClearAirWallObject()
        {
            if (_airWallRootObject != null)
            {
                DestroyImmediate(_airWallRootObject);
                _airWallRootObject = null;
            }
        }
        
        private void ClearAirWallSubMeshObject()
        {
            if (_airWallSubMeshObject != null)
            {
                DestroyImmediate(_airWallSubMeshObject);
                _airWallSubMeshObject = null;
            }
        }

        private void ClearCliffEdgeObject()
        {
            if (_groundMeshCollider != null)
            {
                DestroyImmediate(_groundMeshCollider);
                _groundMeshCollider = null;
            }
            if (_cliffEdgeRootObject != null)
            {
                DestroyImmediate(_cliffEdgeRootObject);
                _cliffEdgeRootObject = null;
            }
        }

        private (bool bHit, Vector3 hitPoint) ChkHitGround(Vector3 curPos)
        {
            float height = 5;
            Vector3 pos = curPos + new Vector3(0, height, 0);
            float dis = height + options.projectionHeight;

            Ray ray = new Ray(pos, Vector3.down);
            if (_groundMeshCollider != null && _groundMeshCollider.Raycast(ray, out RaycastHit raycastHit, dis))
            {
                return (true, raycastHit.point);
            }
            else
            {
                return (false, default);
            }
        }

        private List<Vector3> ChkCliffEdgeAll(Vector3 pos)
        {
            List<Vector3> recordPointList = new List<Vector3>();
            (bool bHit, Vector3 hitPoint) result0 = ChkHitGround(pos);
            if (result0.bHit == false)
            {
                return recordPointList;
            }

            // float disForward = 0.05f;
            // float disRight = 0.05f;
            float disForward = options.detectIntervalDistance;
            float disRight = options.detectIntervalDistance;
            List<Vector3> recordPointLeftList = new List<Vector3>();
            List<Vector3> recordPointRightList = new List<Vector3>();

            ChkCliffEdgeLine(false, pos - Vector3.forward * disForward, disForward, disRight, ref recordPointLeftList, ref recordPointRightList);
            ChkCliffEdgeLine(true, pos, disForward, disRight, ref recordPointLeftList, ref recordPointRightList);

            recordPointList.AddRange(recordPointLeftList);
            recordPointRightList.Reverse();
            recordPointList.AddRange(recordPointRightList);
            OptimizePath(ref recordPointList);
            return recordPointList;
        }

        private void ChkCliffEdgeLine(bool bForward, Vector3 pos, float disForward, float disRight, ref List<Vector3> recordPointLeftList, ref List<Vector3> recordPointRightList)
        {
            int retryTimes = 400;
            bool bNotHitGround = false;
            for (int i = 0; i <= retryTimes; i++)
            {
                Vector3 posTmp;
                if (bForward)
                {
                    posTmp = pos + Vector3.forward * disForward * i;
                }
                else
                {
                    posTmp = pos - Vector3.forward * disForward * i;
                }

                (bool bHit, Vector3 hitPoint) result = ChkHitGround(posTmp);
                if (result.bHit == false)
                {
                    bool bHit = false;
                    for (int j = 1; j <= retryTimes; j++)
                    {
                        Vector3 posTmp2 = posTmp + Vector3.right * disForward * j;
                        (bool bHit, Vector3 hitPoint) result2 = ChkHitGround(posTmp2);
                        if (result2.bHit)
                        {
                            bHit = true;
                            posTmp = posTmp2;
                            break;
                        }
                        else
                        {
                            posTmp2 = posTmp - Vector3.right * disForward * j;
                            (bool bHit, Vector3 hitPoint) result3 = ChkHitGround(posTmp2);
                            if (result3.bHit)
                            {
                                bHit = true;
                                posTmp = posTmp2;
                                break;
                            }
                        }
                    }

                    if (bHit == false)
                    {
                        bNotHitGround = true;
                        break;
                    }
                }

                if (bNotHitGround)
                {
                    break;
                }

                (bool bHit, Vector3 hitPoint) resultRight = ChkCliffEdge(posTmp, Vector3.right, disRight);
                if (resultRight.bHit)
                {
                    if (bForward)
                    {
                        recordPointRightList.Add(resultRight.hitPoint);
                    }
                    else
                    {
                        recordPointRightList.Insert(0, resultRight.hitPoint);
                    }
                }

                (bool bHit, Vector3 hitPoint) resultLeft = ChkCliffEdge(posTmp, -Vector3.right, disRight);
                if (resultLeft.bHit)
                {
                    if (bForward)
                    {
                        recordPointLeftList.Add(resultLeft.hitPoint);
                    }
                    else
                    {
                        recordPointLeftList.Insert(0, resultLeft.hitPoint);
                    }
                }
            }
        }

        private (bool, Vector3) ChkCliffEdge(Vector3 pos, Vector3 forward, float disOne)
        {
            if (disOne <= 0.002f)
            {
                return (true, pos);
            }

            int count = 10000;
            Vector3 lastHitPoint = Vector3.zero;
            bool findCliffEdge = false;
            int i = 0;
            while (i < count)
            {
                i++;
                (bool bHit, Vector3 hitPoint) result = ChkHitGround(pos + forward * disOne * i);
                if (result.bHit)
                {
                    lastHitPoint = result.hitPoint;
                }
                else
                {
                    i++;
                    (bool bHit, Vector3 hitPoint) result2 = ChkHitGround(pos + forward * disOne * i);
                    if (result2.bHit)
                    {
                        lastHitPoint = result2.hitPoint;
                    }
                    else
                    {
                        findCliffEdge = true;
                        break;
                    }
                }
            }

            if (findCliffEdge == false)
            {
                return (false, default);
            }
            else
            {
                if (lastHitPoint == Vector3.zero)
                {
                    (bool bHit, Vector3 hitPoint) result2 = ChkHitGround(pos);
                    if (result2.bHit)
                    {
                        return (true, result2.hitPoint);
                    }
                    else
                    {
                        return (false, default);
                    }
                }
                else
                {
                    return ChkCliffEdge(lastHitPoint, forward, disOne / 5);
                }
            }
        }

        public static void OptimizePath(ref List<Vector3> pointList)
        {
            int childCount = pointList.Count;
            if (childCount <= 2)
            {
                return;
            }

            bool optimize = _OptimizePath(ref pointList);
        }

        private static bool _OptimizePath(ref List<Vector3> pointPosList)
        {
            for (int i = 1; i < pointPosList.Count - 1; i++)
            {
                Vector3 direction1 = pointPosList[i - 1] - pointPosList[i];
                if (direction1 == Vector3.zero)
                {
                    pointPosList.RemoveAt(i);
                    _OptimizePath(ref pointPosList);
                    return true;
                }

                // if (direction1.sqrMagnitude < 0.2f*0.2f)
                // {
                //     pointPosList.RemoveAt(i);
                //     _OptimizePath(ref pointPosList);
                //     return true;
                // }
                Vector3 direction2 = pointPosList[i + 1] - pointPosList[i];
                float angle = Vector3.Angle(direction1, direction2);
                if (angle > 160)
                {
                    pointPosList.RemoveAt(i);
                    _OptimizePath(ref pointPosList);
                    return true;
                }
            }

            return false;
        }

        private void ShowCliffEdge(List<Vector3> edgePointList)
        {
            LineRenderer lineRenderer = this._cliffEdgeRootObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = this._cliffEdgeRootObject.AddComponent<LineRenderer>();
            }
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.red;
            lineRenderer.material = options.airWallLineMaterial;
            lineRenderer.positionCount = edgePointList.Count;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, edgePointList[i]);
            }
            lineRenderer.useWorldSpace = false;
            lineRenderer.generateLightingData = true;
            
            for (int i = 0; i < edgePointList.Count; i++)
            {
                Vector3 pos = edgePointList[i];
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                go.transform.SetParent(this._cliffEdgeRootObject.transform);
                go.transform.position = pos;
            }
        }
        
        private void TraverseEdgePath(List<Vector3> edgePointList, Action<Vector3, Vector3> onTraverse)
        {
            int count = edgePointList.Count;
            if (count >= 2)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector3 point1 = edgePointList[i];
                    Vector3 point2;
                    if (i == count - 1)
                    {
                        point2 = edgePointList[0];
                    }
                    else
                    {
                        point2 = edgePointList[i + 1];
                    }

                    if (onTraverse != null)
                    {
                        onTraverse(point1, point2);
                    }
                }
            }
        }

        private void CreateAirWallSubMesh(Vector3 pos1, Vector3 pos2)
        {
            Vector2 Size = options.airWallSize;
            float BareSize = options.airWallBareSize;

            float dis = Vector3.Distance(pos1, pos2);
            float disHorizontal = Vector3.Distance(new Vector3(pos1.x, 0, pos1.z), new Vector3(pos2.x, 0, pos2.z));

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.enabled = false;

            go.transform.localScale = new Vector3(disHorizontal + BareSize, Size.y, Size.x);

            go.transform.SetParent(this._airWallSubMeshObject.transform);

            // Calculate position
            go.transform.position = pos1 + (pos2 - pos1).normalized * (dis * 0.5f) + new Vector3(0, Size.y / 2 - 0.1f - Mathf.Abs(pos1.y - pos2.y) / 2
                , 0);
            // Calculate rotation
            float z = pos1.z - pos2.z;
            float angle = Mathf.Asin(z / disHorizontal) * Mathf.Rad2Deg;
            // Calculate orientation
            float dir = pos1.x - pos2.x > 0 ? -1 : 1;
            if (angle * dir != 0)
            {
                go.transform.rotation = Quaternion.Euler(new Vector3(0, angle * dir, 0));
            }
        }

        private void CombineAirWallSubMeshes()
        {
            if (this._airWallSubMeshObject == null)
            {
                return;
            }
            Transform transRoot = this._airWallSubMeshObject.transform;
            int childCount = transRoot.childCount;
            if (childCount <= 1)
            {
                return;
            }

            Material sharedMaterial = options.airWallMaterial;
            bool isOnlyShow = options.visualEffectOnly;
            
            GameObject combineMeshGo = this._airWallRootObject;
            MeshRenderer thisMeshRender = combineMeshGo.AddComponent<MeshRenderer>();
            thisMeshRender.receiveShadows = false;
            thisMeshRender.shadowCastingMode = ShadowCastingMode.Off;
            MeshFilter thisMeshFilter = combineMeshGo.AddComponent<MeshFilter>();

            CombineInstance[] combine = new CombineInstance[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transRoot.GetChild(i);
                var meshFs = child.gameObject.GetComponent<MeshFilter>();

                combine[i].mesh = meshFs.sharedMesh;
                combine[i].transform = meshFs.transform.localToWorldMatrix;
            }
            
            thisMeshRender.sharedMaterial = sharedMaterial;

            var newMesh = new Mesh();
            // CombineMesh: true to merge all sub meshes to this mesh
            newMesh.CombineMeshes(combine, true);
            thisMeshFilter.mesh = newMesh;
            MeshCollider collider = combineMeshGo.AddComponent<MeshCollider>();
            if (isOnlyShow)
            {
                collider.isTrigger = true;
            }
            else
            {
                collider.isTrigger = false;
            }
        }

        private void ShowAirWall(GameObject agentObject)
        {
            if (_airWallRootObject == null || agentObject == null)
            {
                return;
            }

            AirWallVisualizer airWallVisualizer = _airWallRootObject.AddComponent<AirWallVisualizer>();
            airWallVisualizer.edgeMaterial = options.airWallMaterial;
            airWallVisualizer.maxDistance = options.airWallEffectMaxDistance;
            airWallVisualizer.emissionIntensity = options.airWallEffectEmissionIntensity;
            airWallVisualizer.enabled = false;
            airWallVisualizer.enabled = true;
            airWallVisualizer.SetAgentTransform(agentObject.transform);
        }
    }

    // Helper class for holding the visual effect of the air wall.
    internal class AirWallVisualizer : MonoBehaviour
    {
        public Transform agentTransform;
        public Material edgeMaterial;

        public float maxDistance = 5;
        public float emissionIntensity = 1;

        [Header("Shader Reference")]
        [SerializeField] private string _protagonistPosition = "_ProtagonistPosition";

        private void Awake()
        {
        }

        private void OnEnable()
        {
            if (edgeMaterial != null)
            {
                edgeMaterial.SetFloat("_MaxDistance", this.maxDistance);
                edgeMaterial.SetFloat("_EmissionIntensity", this.emissionIntensity);
            }
        }

        public void SetAgentTransform(Transform agentTransform)
        {
            this.agentTransform = agentTransform;
        }

        private void Update()
        {
            if (this.agentTransform != null && edgeMaterial != null)
            {
                edgeMaterial.SetVector(_protagonistPosition, this.agentTransform.position);
            }
        }
    }
}
