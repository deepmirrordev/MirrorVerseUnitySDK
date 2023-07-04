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

        // Returns the current operation state.
        SpaceOperationState GetOperationState();

        // Whether the Space is under an ecef offset origin after successful localization.
        // Equivalent to GetOriginMode() != SpaceOriginMode.LocalOrigin
        bool IsLocalized();

        // Returns the current origin mode.
        SpaceOriginMode GetOriginMode();

        // Returns the localization result if localized or error status if not localized.
        StatusOr<LocalizationResult> GetLocalizationResult();

        // ================================================================================
        //   Async operations to consume the Space localization.
        //   Operations could be triggered from any thread or coroutines asynchronously.
        // ================================================================================

        // Starts an async operation to localize with the loaded or processed scene with camera images.
        // OnLocalizationUpdate is called there is a pose update of the device in the scene.
        // State change: Idle -> Localizing
        Status StartLocalization(OnLocalizationUpdate onLocalizationUpdate = null);

        // Stops the running async operation.
        // State change: Localizing -> Idle
        Status StopLocalization();

        // ================================================================================
        //    Delegates and events in Space API.
        // ================================================================================
        delegate void OnLocalizationUpdate(StatusOr<Pose> localizedPose, StatusOr<EcefPose> ecefPose);

        event OnLocalizationUpdate onLocalizationUpdate;
    }
}
