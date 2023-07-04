﻿using UnityEngine;

namespace MirrorVerse
{
    // Represneds device's physical spec.
    public struct DevicePhysicalSpec
    {
        public enum DeviceFormFactor
        {
            Phone,
            Tablet,
            Other
        }

        public DeviceFormFactor formFactor;

        public Vector2 displayDimension;

        public Vector2 cameraPosition;
    }
}
