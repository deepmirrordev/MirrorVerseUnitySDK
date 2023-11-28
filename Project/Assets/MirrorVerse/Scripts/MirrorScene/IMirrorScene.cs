using UnityEngine;

namespace MirrorVerse
{

    // Scene represents a surrounding area with scene identifier and spatial information
    // for application to interact with physical world.
    public interface IMirrorScene
    {
        // Initializes the Scene API. This must be called before any further operations.
        // Returns error status if not successful.
        Status Initialize();

        // ====================================================================
        //   Blocking accessors to retrieve data from the active Scene.
        //   These readonly accessors can be called in any threads or coroutines
        //   and return immediately.
        // ====================================================================

        // Returns the current operation state.
        SceneOperationState GetOperationState();

        // Returns the current origin mode.
        SceneOriginMode GetOriginMode();

        // Returns the SceneInfo status.
        SceneStatus GetSceneStatus();

        // Returns the SceneInfo metadata object that encapsulates all needed information of a scene
        // or error status if not prepared or loaded.
        StatusOr<SceneInfo> GetSceneInfo();

        // Returns the a detected marker if exists, or a host marker.
        StatusOr<MarkerInfo> GetMarkerInfo();

        // Displays a QR code image for the current scene.
        Status ShowMarker(string extraData = null);

        // Hides the QR code image if shown.
        void HideMarker();

        // Temporarily skip or unskip the scene streaming. During the skipping period, no frame is captured.
        // Note that a new stream is started with skip set to false by default.
        void SkipCaptureFrames(bool shouldSkip);

        // Returns the localization result if localized or error status if not localized.
        StatusOr<LocalizationResult> GetLocalizationResult();

        // Returns the wrapper game object of the processed mesh of the scene.
        StatusOr<GameObject> GetSceneMeshWrapperObject();

        // Returns the raycast result of the current cursor. Returns null if no raycast result.
        // When scene is not ready, the cursor follows at raycast on detected planes.
        // After scene is ready, the cursor follows at raycast on the processed mesh.
        StatusOr<Pose> GetCurrentCursorPose();

        // ================================================================================
        //   Asynchronous operations to manipulate the active Scene.
        //   Operations could be triggered from any thread or coroutines asynchronously.
        // ================================================================================

        // Creates a new empty scene. Returns error status if not successful.
        // OnSceneStandby is called when the scene has been created in the cloud.
        // State change: Idle -> Standby
        Status CreateScene(OnSceneStandby onSceneStandby = null);

        // Joins an existing scene created by others. Returns error status if not successful.
        // OnSceneStandby is called when the existing scene has been fetched from cloud.
        // State change: Idle -> Standby
        // Depends the active joined scene's status, the following call could be:
        //    Empty|Capturing: StartSceneStream then FinishSceneStream to capture the scene.
        //    Completed:       DownloadSceneMesh to download the processed scene mesh.
        Status JoinScene(string sceneId, OnSceneStandby onSceneStandby = null);

        // Starts an async operation for marker detection.
        // OnMarkerDetected is called when an existing scene is detected through the marker.
        // State change: Idle -> Detecting
        Status StartMarkerDetection(OnMarkerDetected onMarkerDetected = null);

        // Stops the marker detection operation.
        // State change: Detecting -> Idle
        Status StopMarkerDetection();

        // Starts an async operation to start stream scene informations until the operation is stopped.
        // OnSceneStreamUpdate is called whenever the stream has an update.
        // OnSceneStreamFinish is called when the stream finishes, triggered by other clients.
        // State change: Standby -> Streaming
        Status StartSceneStream(OnSceneStreamUpdate onSceneStreamUpdate = null, OnSceneStreamFinish onSceneStreamFinish = null);

        // Resets the stream to start over again.
        // State change: Streaming -> Streaming
        Status ResetSceneStream();

        // Finishes this client's stream operation and starts to wait for the scene to be processed and downloaded.
        // OnSceneReady is called when the scene is processed and downloaded and ready for localization.
        // State change: Streaming -> Processing -> Downloading -> Ready
        Status FinishSceneStream(OnSceneReady onSceneReady = null, OnSceneProcessUpdate onSceneProcessUpdate = null);

        // Downloads the terrain mesh for a active completely processed scene.
        // OnSceneReady is called when the scene is downloaded and ready for localization.
        // State change: Standby -> Downloading -> Ready
        Status DownloadSceneMesh(OnSceneReady onSceneReady = null, OnSceneDownloadUpdate onSceneDownloadUpdate = null);

        // Starts an async operation to localize with the loaded or processed scene with camera images.
        // OnLocalizationUpdate is called there is a pose update of the device in the scene.
        // State change: Ready -> Localizing
        Status StartLocalization(OnLocalizationUpdate onLocalizationUpdate = null);

        // Stops the localization operation and back to ready state. This is a resumable state that user can call resume localization later.
        // State change: Localizing -> Ready
        Status StopLocalization();

        // Exits the active scene and back to idle state, whether the active scene is processing or ready.
        // State change: Detecting|Streaming|Processing|Downloading|Localizing|Ready -> Idle.
        void ExitScene();

        // ================================================================================
        //    Delegates and events in Mirror Scene API.
        // ================================================================================

        delegate void OnMarkerDetected(StatusOr<MarkerInfo> marker, StatusOr<Pose> markerPose, StatusOr<Pose> localizedPose);
        delegate void OnSceneStandby(StatusOr<SceneInfo> sceneInfo);
        delegate void OnSceneStreamUpdate(StatusOr<FrameSelectionResult> frameSelectionResult, StatusOr<SceneStreamRenderable> streamRenderable);
        delegate void OnSceneStreamFinish(Status status);
        delegate void OnSceneReady(StatusOr<SceneInfo> sceneInfo);
        delegate void OnLocalizationUpdate(StatusOr<Pose> localizedPose);
        delegate void OnSceneProcessUpdate(float progress);
        delegate void OnSceneDownloadUpdate(float progress);

        event OnMarkerDetected onMarkerDetected;
        event OnSceneStandby onSceneStandby;
        event OnSceneStreamUpdate onSceneStreamUpdate;
        event OnSceneStreamFinish onSceneStreamFinish;
        event OnSceneReady onSceneReady;
        event OnLocalizationUpdate onLocalizationUpdate;
        event OnSceneProcessUpdate onSceneProcessUpdate;
        event OnSceneDownloadUpdate onSceneDownloadUpdate;
    }
}
