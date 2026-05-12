using UnityEngine;

namespace MirrorVerse
{
    public enum LocalizationStatus
    {
        // Localization request not requested yet.
        NotRequested,
        // Localization request failed at precondition.
        FailedPrecondition,
        // Localization requested, and waiting for response.
        WaitForResponse,
        // Successfully localizaed.
        Localized
    }

    // Represents results from localization system.
    public struct LocalizationResult
    {
        // The timestamp (milliseconds since epoch) of the frame that localization result is for.
        public long timestamp;

        // Latency in milliseconds of resolving the localization result of the frame.
        public double latency;

        // A pose that localized to local Scene origin, or Space with an offset to fit in device's XR session.
        // See IMirrorSpace.SetCoordinatesOffsetBase method to set offset to a space.
        public Pose localizedPose;

        // Optional Earth ECEF coordinate pose if localized with MirrorSpace.
        // Note: Unity cannot directly interact with this pose because it's earth based and usually
        // with very large values.
        public EcefPose? ecefPose;

        // Optional Earth geodetic pose of the tracked image in physical reality. It's the same position of Ecef but in WGS84 format.
        public GeodeticPose? geodeticPose;

        // Status of the localization request.
        public LocalizationStatus localizationStatus;
    }
}
