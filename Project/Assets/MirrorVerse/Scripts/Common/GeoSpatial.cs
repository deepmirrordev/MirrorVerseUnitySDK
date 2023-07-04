using System;
using UnityEngine;

// Data types for common Geodetic coordinates system and conversions.
namespace MirrorVerse
{
    // WGS data type for earth-based geodetic coordinate system.
    [Serializable]
    public struct Wgs3d
    {
        // Latitude of the earth in degrees [-90(S), 90(N)].
        public double latitude;

        // Longitude of the earth in degrees [-180(W), 180(E)].
        public double longitude;

        // Elevation of mean sea level in meters.
        public double altitude;

        public static Wgs3d zero
        {
            get
            {
                return new Wgs3d
                {
                    latitude = 0,
                    longitude = 0,
                    altitude = 0
                };
            }
        }

        public override string ToString()
        {
            string latitudeLabel = new string[] { "S", "", "N" }[Math.Sign(latitude) + 1];
            string longitudeLabel = new string[] { "W", "", "E" }[Math.Sign(latitude) + 1];
            return String.Format("({0}°{1}, {2}°{3}, {4}m)", latitude, latitudeLabel, longitude, longitudeLabel, altitude);
        }
    }

    // ECEF data type for earth-based geodetic coordinate system.
    [Serializable]
    public struct Ecef3d
    {
        // Meters from earth center towards the intersection of equator and prime meridian 0° longitude.
        public double x;

        // Meters from earth center towards the intersection of equator and 90°E longitude.
        public double y;

        // Meters from earth center towards North pole.
        public double z;

        public static Ecef3d zero
        {
            get
            {
                return new Ecef3d
                {
                    x = 0,
                    y = 0,
                    z = 0
                };
            }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    // A pose that with ECEF coordinate system.
    // Unlike float-based UnityEngine.Pose, transition vector is double-based.
    [Serializable]
    public struct EcefPose
    {
        public Ecef3d position;

        public Quaternion rotation;

        public static EcefPose zero {
            get
            {
                return new EcefPose
                {
                    position = Ecef3d.zero,
                    rotation = Quaternion.identity
                };
            }
        }

        public override string ToString()
        {
            return String.Format("({0}), ({1})", position.ToString(), rotation.ToString());
        }
    }

    // A pose that with WGS coordinate system.
    // Unlike float-based UnityEngine.Pose, transition vector is double-based.
    [Serializable]
    public struct GeodeticPose
    {
        public Wgs3d location;        // ENU lat/lng/alt value
        public Vector3 orientation;   // ENU   yaw/pitch/roll

        // TODO: yaw/pitch/roll is conventional and maybe not suitabe. consider use quaternion instead.

        public static GeodeticPose zero
        {
            get
            {
                return new GeodeticPose
                {
                    location = Wgs3d.zero,
                    orientation = Vector3.zero
                };
            }
        }

        public override string ToString()
        {
            return String.Format("({0}), ({1})", location.ToString(), orientation.ToString());
        }
    }
}
