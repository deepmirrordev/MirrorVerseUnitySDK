using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MirrorVerse
{
    // Tag component that is used to mark an object as a nav mesh area.
    public class NavMeshTag : MonoBehaviour
    {
        public static Action onMeshFilterUpdated;

        public static HashSet<NavMeshTag> navMeshTags = new HashSet<NavMeshTag>();

        [SerializeField] public NavMeshBuildSourceShape shape;

        // The type of the area. This is the same as the Area Types in Unity.
        [SerializeField] public int area;

        [SerializeField] public MeshFilter meshFilter;

        [SerializeField] public Bounds bounds;

        public NavMeshBuildSource GetNavMeshBuildSource()
        {
            return new NavMeshBuildSource
            {
                sourceObject = meshFilter.sharedMesh,
                shape = shape,
                transform = transform.localToWorldMatrix,
                area = area,
            };
        }

        public void OnEnable()
        {
            navMeshTags.Add(this);
            if (shape == NavMeshBuildSourceShape.Mesh)
            {
                meshFilter = GetComponent<MeshFilter>();
                bounds = meshFilter.mesh.bounds;
            }

            onMeshFilterUpdated?.Invoke();
        }

        void OnDisable()
        {
            navMeshTags.Remove(this);
            onMeshFilterUpdated?.Invoke();
        }
    }
}
