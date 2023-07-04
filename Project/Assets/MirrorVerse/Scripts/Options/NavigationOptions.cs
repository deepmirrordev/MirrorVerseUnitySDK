using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "NavigationOptions", menuName = "MirrorVerse/Navigation Options")]
    public class NavigationOptions : ScriptableObject
    {
        public enum NavigationMode
        {
            None,

            // Unity Navigation Mesh support.
            Unity
        }

        // Whether and how to generate the navigation mesh for the processed mesh.
        public NavigationMode navigationMode = NavigationMode.Unity;


        // The following field has an editor for editing. See NavigationOptionsEditor.

        // All agent types from Navigation settings, keep in sync.
        [SerializeField]
        [HideInInspector]
        public List<int> agentTypeIds = new();

        // Overriden value for agent radius for each agent type.
        [SerializeField]
        [HideInInspector]
        public List<float> radiusOverrides = new();

        public bool Enabled { get { return navigationMode != NavigationMode.None; } }

        public float? GetRadiusOverride(int agentTypeId)
        {
            int index = agentTypeIds.IndexOf(agentTypeId);
            if (index != -1) 
            {
                return radiusOverrides[index];
            }
            return null;
        }

        public void SetRadiusOverride(int agentTypeId, float radiusOverride)
        {
            int index = agentTypeIds.IndexOf(agentTypeId);
            if (index != -1)
            {
                radiusOverrides[index] = radiusOverride;
            }
            else
            {
                agentTypeIds.Add(agentTypeId);
                radiusOverrides.Add(radiusOverride);
            }
        }
    }
}
