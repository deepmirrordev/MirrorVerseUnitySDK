using MirrorVerse.Options;
using System;
using UnityEngine;

namespace MirrorVerse
{

    // Space represents a large area on Earth space and provides localization functionalities
    // for application to interact with physical world.

    // methods are thread safe?
    public interface IMirrorSpace
    {
        // Initializes the Space API. This must be called before any further operations.
        // Returns error status if not sucessful.
        Status Initialize();

        // ================================================================================
        //   Blocking accessors to retrieve data from the Space.
        //   These methods are thread-safe.
        // ================================================================================

        // Returns the XR Platform adapter. On most phone/tablet, this could be AR Foudnation adapter.
        // But in other platforms like AR/MR headsets, including Quest/Pico/Xreal/Rokid, this is platform-specific adapter.
        XrPlatformAdapter GetXrPlatformAdapter();

        // Returns the current operation state.
        SpaceOperationState GetOperationState();

        // Whether the Space is under an ecef offset origin after successful localization.
        // Equivalent to GetOriginMode() != SpaceOriginMode.LocalOrigin
        bool IsLocalized();

        // Returns the current origin mode.
        SpaceOriginMode GetOriginMode();

        // Returns the localizer options for localization feature configuration.
        LocalizerOptions GetLocalizerOptions();

        // Returns the localization result if localized or error status if not localized.
        StatusOr<LocalizationResult> GetLocalizationResult();

        // Adds tracked images for detecting and tracking.
        Status AddTrackedImageData(TrackedImageData[] trackedImageData, Action trackedImageUpdated = null);

        // Removes tracked images for detecting and tracking.
        Status RemoveTrackedImageData(string[] trackedImageNames, Action trackedImageUpdated = null);

        // Sets the offset base value for Unity coordinates origin's real Earth location (ECEF Right handed). Developers can use call at startup to attach their AR scene on Earth.
        public void SetCoordinatesOffsetBase(Ecef3d offsetBase);

        // Sets the offset base value for Unity coordinates origin's real Earth location (WGS84 LLA). Developers can use call at startup to attach their AR scene on Earth.
        public void SetCoordinatesOffsetBase(Wgs3d offsetBase);

        // Gets the offset base value for Unity coordinates origin's real Earth location (ECEF Right handed). Developers can use this offset to attach their AR scene on Earth.
        public Ecef3d GetCoordinatesOffsetBase();

        // Gets the local pose of a given real Earth location (ECEF Right handed) to coordinates offset base.
        public Pose GetLocalPoseFromCoordinatesOffsetBase(EcefPose earthPose);

        // Gets the real Earth localtion (ECEF Right handed) with local pose offset from the coordiantes offset base.
        public EcefPose GetEarthPoseFromCoordinatesOffsetBase(Pose localPose);

        // ================================================================================
        //   Async operations to consume the Space localization.
        //   Operations could be triggered from any thread or coroutines asynchronously.
        // ================================================================================

        // Starts an async operation to localize with the loaded or processed scene with camera images with cloud localization service.
        // OnLocalizationUpdate is called there is a pose update of the device in the scene.
        // State change: Idle -> Localizing
        Status StartLocalization(OnLocalizationUpdate onLocalizationUpdate = null);

        // Stops the running async operation.
        // State change: Localizing -> Idle
        Status StopLocalization();

        // Starts an async operation to localize with the loaded or processed scene with camera images on device.
        // OnLocalizationUpdate is called there is a pose update of the device in the scene.
        // State change: Idle -> Localizing
        Status StartTrackedImageLocalization(string referenceImageName = null, OnTrackedImageDetected onTrackedImageDetected = null, OnLocalizationUpdate onLocalizationUpdate = null);

        // Stops the running async operation.
        // State change: Localizing -> Idle
        Status StopTrackedImageLocalization();

        // ================================================================================
        //    Delegates and events in Space API.
        // ================================================================================
        delegate void OnApiReady();
        delegate void OnDeviceTrackingStatusChange(StatusOr<TrackingStatus> newStatus);
        delegate void OnLocalizationUpdate(StatusOr<Pose> localizedPose, StatusOr<EcefPose> ecefPose);
        delegate void OnTrackedImageDetected(StatusOr<TrackedImageResult> trackedImageResult);

        // Events that triggered when this API is loaded and ready to use.
        // This event is only trigger once for a given assigned event handler.
        // If the event handler is assigned after API is ready, the handler gets triggered immediately.
        event OnApiReady onApiReady;

        // Events that triggered when device tracking status changes (from Tracking to Not Tracking, or vice versa).
        // This tracking status represents whether the local pose of AR camera can correctly
        // follow the physical pose of the device in the real world.
        event OnDeviceTrackingStatusChange onDeviceTrackingStatusChange;

        // Events that triggered every frame when is in Localizing state (after one of the localization is started).
        // After localization is started, event gets raw local pose from local AR world origin, which is the first pose
        // after AR camera is started (same as ARCore/ARKit/AREngine).
        //
        // At the same time, each frame will attempt to do either cloud vision localizing or local image detecting.
        // If succeeded, the origin of the AR world will be shift to a determined pose (earth, or tracked image), and
        // event also gets ecef pose if the cloud localizar or image marker contain absolute pose under earth origin.
        event OnLocalizationUpdate onLocalizationUpdate;

        // Event tha triggered when using tracked image localization and a reference image is detected. When this event
        // triggered, the localization process is successfully localized, and AR world origin is shifted accordingly.
        event OnTrackedImageDetected onTrackedImageDetected;
    }
}
