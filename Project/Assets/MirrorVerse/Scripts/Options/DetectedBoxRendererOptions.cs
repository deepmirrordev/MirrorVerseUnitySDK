using System;
using UnityEngine;

namespace MirrorVerse.Options
{
    [CreateAssetMenu( fileName = "DetectedBoxRendererOptions", menuName = "MirrorVerse/Detected Box Renderer Options")]
    public class DetectedBoxRendererOptions : ScriptableObject
    {
        // Orange tone
        public static readonly Color[] DEFAULT_PALETTE_COLORS = new Color[] {
            new Color32(246, 81, 29, 160),   //#F6511D
            new Color32(255, 180, 0, 160),   //#FFB400
            new Color32(127, 184, 0, 160),   //#7FB800
            new Color32(13, 44, 84,  160),   //#0D2C54
            new Color32(197, 126, 214, 160), //#C57ED6
            new Color32(21, 104, 207, 160),  //#1568CF
        };

        // Set the detected box visible or not.
        public bool visible;

        // Line material of box edges.
        public Material lineMaterial;
        
        // Text font for detected box label.
        public Font textFont;

        // Box line colors
        public Color[] paletteColors = Array.ConvertAll(DEFAULT_PALETTE_COLORS, color => color);
    }
}
