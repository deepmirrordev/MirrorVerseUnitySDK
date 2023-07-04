using System;
using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu(fileName = "TrajectoryRendererOptions", menuName = "MirrorVerse/Trajectory Renderer Options")]
    public class TrajectoryRendererOptions : ScriptableObject
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

        // Whether to visualize trajectory for capture streaming.
        public bool visible = false;

        // Material to visualize trajectory way points.
        public Material material;

        // Point size of trajectory way point. In meters.
        public float pointSize = 0.01f;

        // Colors for point cloud of each client.
        public Color primaryColor = DEFAULT_PRIMARY_COLOR;
        public Color[] paletteColors = Array.ConvertAll(DEFAULT_PALETTE_COLORS, color => color);
    }
}
