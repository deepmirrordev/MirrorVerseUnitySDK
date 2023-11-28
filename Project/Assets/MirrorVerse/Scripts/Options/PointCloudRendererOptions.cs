using System;
using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "PointCloudRendererOptions", menuName = "MirrorVerse/Point Cloud Renderer Options")]
    public class PointCloudRendererOptions : ScriptableObject
    {
        // Default color value.
        public static readonly Color DEFAULT_PRIMARY_COLOR = new Color32(67, 168, 88, 255); //#43A858  client 1
        public static readonly Color[] DEFAULT_PALETTE_COLORS = new Color[] {
            new Color32(199, 0, 57, 255),   //#C70039   client 2
            new Color32(255, 205, 55, 255), //#FFCD37   client 3
            new Color32(255, 81, 18, 255),  //#FF5112   client 4
            new Color32(227, 25, 101, 255), //#E31965   client 5
            new Color32(181, 153, 224, 255),//#B599E0   client 6
            new Color32(52, 134, 217, 255), //#3486d9   client 7
            new Color32(208, 208, 208, 255),//#D0D0D0   client 8
        };

        // Whetehr to show point clouds.
        public bool visible = false;

        // Bounds for point cloud drawing volume. In meters.
        public int drawBounds = 20;

        // Number of sides to draw a point disk.
        public int diskSides = 16;

        // Radius of a point disk. In meters.
        public float pointSize = 0.015f;

        // Seconds for fading in duration.
        public float fadeInDuration = 2;

        // Seconds for fading out duration.
        public float fadeOutDuration = 1;

        // Distance from camera where points will not display if out of this distance. In meters.
        public float displayDistance = 2f;

        // Average points per second when revealing new point clouds.
        public float pointPerSec = 500;

        // Peek points per second when revealing new point clouds.
        public float peekPointPerSec = 5000;

        // A scale ratio to original size when a point is fading in.
        public float inflationScale = 1.5f;

        // Opacity of point disk.
        public float opacity = 1f;

        // This has to work with PointCloudOcclusion shader.
        public Material pointCloudMaterial;

        // Whether to visualize as black mask with transparent points. False to use color-coded points.
        public bool transparentMode = false;

        // This is used when transparent mode is set to true.
        public GameObject cameraBackgroundMaskPrefab;

        // Colors for point cloud of each client.
        public Color primaryColor = DEFAULT_PRIMARY_COLOR;
        public Color[] paletteColors = Array.ConvertAll(DEFAULT_PALETTE_COLORS, color => color);
    }
}
