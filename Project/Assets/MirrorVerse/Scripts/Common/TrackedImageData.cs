using System;
using UnityEngine;

namespace MirrorVerse
{
    [Serializable]
    public class TrackedImageData
    {
        // Unique name to identify the tracked image.
        public string refernceName;

        // Texture of the trackable image reference.
        [SerializeField, Tooltip("The texture for the reference image. Must be marked as readable.")]
        public Texture2D refernceTexture;

        // Physical pose (location+orientation) in Geodetic format (lat/lng/alt + yaw/pitch/roll).
        // Required for outdoor or used with GNSS.
        // Optional if indoor or in a local surrounding space.
        public GeodeticPose physicalPose;

        // Expected physical size of this image may appear in the real world. Optional.
        public Vector2 physicalSize;
    }
}
