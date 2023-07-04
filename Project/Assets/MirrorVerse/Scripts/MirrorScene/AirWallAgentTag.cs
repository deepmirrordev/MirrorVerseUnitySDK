using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse
{
    // Tag component that is an agent which may trigger air wall on a surface.
    public class AirWallAgentTag : MonoBehaviour
    {
        public static List<AirWallAgentTag> airWallAgentTags = new List<AirWallAgentTag>();
        public static Action onAirWallAgentUpdated;

        private void OnEnable()
        {
            // TODO: Support multiple air wall agents. Current implementation only allow one agent at a time.
            if (airWallAgentTags.Count == 1)
            {
                return;
            }
            airWallAgentTags.Add(this);
            onAirWallAgentUpdated?.Invoke();
        }

        void OnDisable()
        {
            airWallAgentTags.Remove(this);
            onAirWallAgentUpdated?.Invoke();
        }
    }
}
