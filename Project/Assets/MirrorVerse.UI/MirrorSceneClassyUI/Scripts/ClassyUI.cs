using MirrorVerse.UI.Renderers;
using System;
using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public enum SystemMenuType
    {
        NoMenu,
        ScanSceneMenu,
        ScanQrCodeMenu,
        ProcessingMenu,
        ConfirmScanFinishMenu,
        ConfirmScanLimitMenu,
        ConfirmScanCancelMenu,
        ConfirmJoinSceneMenu,
        ConfirmLoadMenu,
        ConfirmReviewMenu,
        ConfirmErrorMenu,
    }

    public delegate void OnMenuFinish();
    public delegate void OnMenuCancel();
    public delegate void OnMenuCreateScene();
    public delegate void OnMenuExitScene();
    public delegate void OnMenuLoadRecentScene();
    public delegate void OnMenuJoinScene(string sQRCodeInfo);
    public delegate (bool, string) OnRequestQRCodeExtraData();
    public delegate void OnMenuReviewSceneOpen();
    public delegate void OnMenuReviewSceneClose(bool confirm);
    public delegate void OnMenuScanSceneTimeout();
    public delegate void OnMenuScanSceneTimeoutRetry();
    public delegate void OnMenuScanSceneTimeoutReport();
    public delegate void OnMenuBackClick();

    public class ClassyUI : MonoBehaviour
    {
        // Called when the whole flow finishes with a mirror scene ready for AR game.
        public event OnMenuFinish onMenuFinish = delegate { };

        // Called when the flow is cancelled. No scene in place.
        public event OnMenuCancel onMenuCancel = delegate { };

        // Called when user starts a scene streaming.
        public event OnMenuCreateScene onMenuCreateScene = delegate { };

        // Called when user exits the scene during streaming or after it's ready.
        public event OnMenuExitScene onMenuExitScene = delegate { };

        // Called when user loads a recent scene and joins.
        public event OnMenuLoadRecentScene onMenuLoadRecentScene = delegate { };

        // Called when user joins a scene with QR code.
        public event OnMenuJoinScene onMenuJoinScene = delegate { };

        // Note: this is optional. Only called if set.
        public event OnRequestQRCodeExtraData onRequestQRCodeExtraData = null;

        // Called when user open review screen to review a scene with orange mesh.
        public event OnMenuReviewSceneOpen onMenuReviewSceneOpen = delegate { };

        // Called when user close review screen confirming or canceling the reviewed scene.
        public event OnMenuReviewSceneClose onMenuReviewSceneClose = delegate { };

        // Called when user scan a scene and timeout without any mesh showing.
        public event OnMenuScanSceneTimeout onMenuScanSceneTimeout = delegate { };

        // Called when user retry in a scene scan timeout.
        public event OnMenuScanSceneTimeoutRetry onMenuScanSceneTimeoutRetry = delegate { };

        // Called when user report in a scene scan timeout.
        public event OnMenuScanSceneTimeoutReport onMenuScanSceneTimeoutReport = delegate { };

        // Called when user click back button.
        public event OnMenuBackClick onMenuBackClick = delegate { };

        public RaycastRenderer raycastRenderer;
        public StaticMeshRenderer staticMeshRenderer;

        private const string _RECENT_SCENE_ID_PREF = "MIRRORVERSE_RECENT_SCENE_ID";
        private const string _RECENT_SCENE_EXPIRE_TIMESTAMP_PREF = "MIRRORVERSE_RECENT_SCENE_EXPIRE_TIMESTAMP";
        private const string _RECENT_SCENE_UPDATE_TIMESTAMP_PREF = "MIRRORVERSE_RECENT_SCENE_UPDATE_TIMESTAMP";

        // Remember the flow is started as host or guest.
        private enum UserMode
        {
            Host,
            Guest,
        }

        public enum ErrorMode
        {
            Toast,
            Dialog,
        }

        private enum TriggerLocalizationState
        {
            Default,
            TryTriggerLocalization,
            HasTriggerLocalization,
        }

        private enum QrCodeDetectState
        {
            Default,
            QrCodeDetected,
            HasTriggerCaptureByQrCode,
        }

        public class RecentScenePref
        {
            public string sceneId;
            public long updateTimestampMs;
            public long expireTimestampMs;
        }

        private RecentScenePref _recentScenePref;
        private SystemMenuType _currentMenuType = SystemMenuType.NoMenu;
        private TriggerLocalizationState _currentTriggerLocalizationState = TriggerLocalizationState.Default;
        private QrCodeDetectState _currentQrCodeDetectState = QrCodeDetectState.Default;
        private MarkerInfo? _currentQrCodeInfo = null;
        private UserMode _userMode = UserMode.Host;
        private bool _requireReview = false;
        private bool _forceRequireReview = false; // Overriden by application
        private int _lastSelectedFrameCount = 0;
        private static ClassyUI _instance;

        public static ClassyUI Instance { get { return _instance; } }

        public void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Debug.LogWarning("Cannot create multiple instances for a singleton.");
                Destroy(gameObject);
                return;
            }
            _recentScenePref = LoadRecentScenePref();
        }

        public void Start()
        {
            if (MirrorScene.IsAvailable())
            {
                // Application can listen to some events.
                MirrorScene.Get().onSceneStandby += OnSceneStandby;
                MirrorScene.Get().onMarkerDetected += OnMarkerDetected;
                MirrorScene.Get().onSceneReady += OnSceneReady;
                MirrorScene.Get().onSceneStreamUpdate += OnSceneStreamUpdate;
                MirrorScene.Get().onSceneStreamFinish += OnSceneStreamFinish;
                MirrorScene.Get().onSceneProcessUpdate += OnSceneProcessUpdate;
                MirrorScene.Get().onSceneDownloadUpdate += OnSceneDownloadUpdate;
            }
            else
            {
                Debug.Log("MirrorScene API is not available.");
            }

            // Classy UI currently only works with landscape orientation.
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        public void ShowMenu()
        {
            gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
        }

        public void OnSceneStandby(StatusOr<SceneInfo> sceneInfo)
        {
            if (sceneInfo.Status.IsOk)
            {
                Status status;
                switch (sceneInfo.Value.status)
                {
                    case SceneStatus.Completed:
                        Debug.Log("Scene is completed. Start download...");
                        ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Downloading);
                        SwitchMenu(SystemMenuType.ProcessingMenu);
                        status = MirrorScene.Get().DownloadSceneMesh();
                        _requireReview = _forceRequireReview; // Joined a completed scene, require to review after download, or app force to skip review.
                        if (!status.IsOk)
                        {
                            ShowError(ErrorMode.Dialog, status, "Failed to start scene downloading.");
                        }
                        break;
                    case SceneStatus.Empty:
                    case SceneStatus.Capturing:
                        _requireReview = false; // Start a new scene and scan, do not trigger review.
                        // When joining a scene, start the stream immediately.
                        if (_currentQrCodeDetectState == QrCodeDetectState.QrCodeDetected && 
                            _currentQrCodeInfo.HasValue &&
                            _currentQrCodeInfo.Value.sceneId == sceneInfo.Value.sceneId)
                        {
                            status = MirrorScene.Get().StartSceneStream();
                            if (status.IsOk)
                            {
                                _currentQrCodeDetectState = QrCodeDetectState.HasTriggerCaptureByQrCode;
                                ScanSceneMenu.Instance.SwitchScanState(ScanState.Scanning);
                            }
                            else
                            {
                                ShowError(ErrorMode.Dialog, status, "Failed to start scene streaming in a joined scene.");
                            }
                        }
                        break;
                    case SceneStatus.Failed:
                        ShowError(ErrorMode.Dialog, StatusCode.Unavailable, "Given scene is not available.");
                        break;
                }
            }
            else
            {
                if (sceneInfo.Status.Code == StatusCode.ResourceExhausted)
                {
                    ShowError(ErrorMode.Dialog, sceneInfo.Status, "Requests too frequent. Try again later.");
                }
                else
                {
                    ShowError(ErrorMode.Dialog, sceneInfo.Status, "Failed to prepare the scene.");
                }
            }
        }

        public void OnSceneStreamUpdate(StatusOr<FrameSelectionResult> frameSelectionResult, StatusOr<SceneStreamRenderable> streamRenderable)
        {
            if (frameSelectionResult.HasValue && frameSelectionResult.Value.selectionResultType == FrameSelectionResult.SelectionResultType.Selected)
            {
                // Vibrate if a new frame is selected and sent to cloud during streaming.
                if (frameSelectionResult.Value.selectedFrameCount > _lastSelectedFrameCount)
                {
                    HapticController.Instance.TriggerVibrate();
                    _lastSelectedFrameCount = frameSelectionResult.Value.selectedFrameCount;
                }
            }
        }

        public void OnSceneStreamFinish(Status status)
        {
            // OnSceneStreamFinish is triggered when capture stream is finished and the scene will be processing.
            if (status.IsOk)
            {
                Debug.Log($"Scene stream finished. [{status}]");
                // Could be triggered by either current device, or others.
                //ScanSceneMenu.Instance.ResetMenu();
                ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Processing);
                SwitchMenu(SystemMenuType.ProcessingMenu);
            }
            else if (status.Code == StatusCode.Cancelled)
            {
                ShowError(ErrorMode.Toast, status, "Scene stream cancelled.");
            }
            else
            {
                ShowError(ErrorMode.Toast, status, "Scene stream aborted.");
            }
        }

        private void OnSceneProcessUpdate(float progress)
        {
            if (MirrorScene.Get().GetOperationState() == SceneOperationState.Processing)
            {
                ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Processing, progress);
            }
            else
            {
                Debug.Log($"Received process update event outside of processing cycle. Scene could be exited. {MirrorScene.Get().GetOperationState()}");
            }
        }

        private void OnSceneDownloadUpdate(float progress)
        {
            if (MirrorScene.Get().GetOperationState() == SceneOperationState.Downloading)
            {
                ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Downloading, progress);
            }
            else
            {
                Debug.Log($"Received download update event outside of downloading cycle. Scene could be exited. {MirrorScene.Get().GetOperationState()}");
            }
        }

        public void OnSceneReady(StatusOr<SceneInfo> sceneInfo)
        {
            if (sceneInfo.Status.IsOk)
            {
                ProcessingMenu.Instance.UpdateProcessingTextDownloaded(() => {
                    ProcessingMenu.Instance.UpdateProcessingTextDone(() => {
                        if (_requireReview)
                        {
                            // Review scene to after progress if joining a completed scene.
                            SwitchMenu(SystemMenuType.ConfirmReviewMenu);
                        }
                        else
                        {
                            // If just scanned, then finish as it's already confirmed by ScanFinish menu.
                            Finish();
                        }
                    });
                });
                SaveRecentScenePref(sceneInfo.Value);
                TriggerSceneLocalization();

                if (raycastRenderer != null)
                {
                    raycastRenderer.options.cursorVisible = raycastRenderer.options.raycastOnMeshEnabled;
                }
            }
            else
            {
                ShowError(ErrorMode.Dialog, sceneInfo.Status, "Scene preparation is failed.", "Please try again with adequate light and stable networks.");
            }
        }

        public void OnMarkerDetected(StatusOr<MarkerInfo> markerInfo, StatusOr<Pose> markerPose, StatusOr<Pose> localizedPose)
        {
            if (markerInfo.Status.IsOk)
            {
                if (markerInfo.HasValue)
                {
                    _currentQrCodeInfo = markerInfo.Value;
                }
            }
            _currentQrCodeDetectState = QrCodeDetectState.QrCodeDetected;
            SwitchMenu(SystemMenuType.ConfirmJoinSceneMenu);
        }

        public void SwitchMenu(SystemMenuType menuType)
        {
            if (_currentMenuType == menuType)
            {
                return;
            }

            switch (_currentMenuType)
            {
                case SystemMenuType.ScanSceneMenu:
                    ScanSceneMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmScanFinishMenu:
                    ConfirmScanFinishMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmScanLimitMenu:
                    ConfirmScanLimitMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmScanCancelMenu:
                    ConfirmScanCancelMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ScanQrCodeMenu:
                    ScanQrCodeMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmJoinSceneMenu:
                    ConfirmJoinSceneMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmLoadMenu:
                    ConfirmLoadMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmReviewMenu:
                    ConfirmReviewMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ProcessingMenu:
                    ProcessingMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmErrorMenu:
                    ConfirmErrorMenu.Instance.HideMenu();
                    break;
            }

            switch (menuType)
            {
                case SystemMenuType.NoMenu:
                    // Show nothing.
                    _currentMenuType = SystemMenuType.NoMenu;
                    break;
                case SystemMenuType.ScanSceneMenu:
                    ScanSceneMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ScanSceneMenu;
                    break;
                case SystemMenuType.ConfirmScanFinishMenu:
                    ConfirmScanFinishMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmScanFinishMenu;
                    break;
                case SystemMenuType.ConfirmScanLimitMenu:
                    ConfirmScanLimitMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmScanLimitMenu;
                    break;
                case SystemMenuType.ConfirmScanCancelMenu:
                    ConfirmScanCancelMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmScanCancelMenu;
                    break;
                case SystemMenuType.ScanQrCodeMenu:
                    ScanQrCodeMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ScanQrCodeMenu;
                    break;
                case SystemMenuType.ConfirmJoinSceneMenu:
                    ConfirmJoinSceneMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmJoinSceneMenu;
                    break;
                case SystemMenuType.ConfirmLoadMenu:
                    ConfirmLoadMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmLoadMenu;
                    break;
                case SystemMenuType.ConfirmReviewMenu:
                    ConfirmReviewMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmReviewMenu;
                    break;
                case SystemMenuType.ProcessingMenu:
                    ProcessingMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ProcessingMenu;
                    break;
                case SystemMenuType.ConfirmErrorMenu:
                    ConfirmErrorMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmErrorMenu;
                    break;
            }
        }

        public void ShowError(ErrorMode errorMode, Status status, string message, string suggestion = "")
        {
            if (errorMode == ErrorMode.Toast)
            {
                ToastManager.Instance.Show(message);
            }
            else
            {
                if (suggestion.Length == 0)
                {
                    suggestion = status.Message;
                }
                ConfirmErrorMenu.Instance.SetErrorMessage(message, suggestion);
                SwitchMenu(SystemMenuType.ConfirmErrorMenu);
            }
            Debug.LogError($"{message} [{status}]");
        }

        public Status TriggerCreateScene()
        {
            Status status = MirrorScene.Get().CreateScene();
            if (status.IsOk)
            {
                // Do not trigger onMenuCreateScene, as creating a scene now happens when AR camera is opened.
            }
            else
            {
                ShowError(ErrorMode.Dialog, status, "Failed to create a new scene.");
            }
            return status;
        }

        public Status TriggerStartSceneStreaming()
        {
            Status status = MirrorScene.Get().StartSceneStream();
            
            if (status.IsOk)
            {
                // Trigger onMenuCreateScene after the scan button is pressed.
                this.onMenuCreateScene();
            }
            else
            {
                ShowError(ErrorMode.Toast, status, "Failed to start scene streaming.");
            }
            return status;
        }

        public Status TriggerJoinScene(string sceneId, bool requireReview = true)
        {
            // Application can request to skip review step.
            _forceRequireReview = requireReview;

            Status status = MirrorScene.Get().JoinScene(sceneId);
            if (!status.IsOk)
            {
                ShowError(ErrorMode.Dialog, status, "Failed to join the given scene.");
            }
            return status;
        }

        public void TriggerLoadRecentScene(bool requireReview = true)
        {
            if (HasRecentScene())
            {
                Debug.Log($"Joining the recent scene. [{_recentScenePref.sceneId}]");
                Status status = TriggerJoinScene(_recentScenePref.sceneId, requireReview);
                if (status.IsOk)
                {
                    this.onMenuLoadRecentScene();
                }
            }
            else
            {
                ShowError(ErrorMode.Dialog, StatusCode.Unavailable, "No recent scene available.");
            }
        }

        public void TriggerJoinSceneFromQrCode(bool requireReview = true)
        {
            if (_currentQrCodeInfo != null)
            {
                Debug.Log($"Joining the scene from detected QR code. [{_currentQrCodeInfo.Value.sceneId}] [require review={requireReview}]");
                Status status = TriggerJoinScene(_currentQrCodeInfo.Value.sceneId, requireReview);
                if (status.IsOk)
                {
                    string qrCodeExtraData = "";
                    if (_currentQrCodeInfo.Value.extraData != null)
                    {
                        qrCodeExtraData = System.Text.Encoding.UTF8.GetString(_currentQrCodeInfo.Value.extraData);
                    }
                    this.onMenuJoinScene(qrCodeExtraData);
                }
            }
            else
            {
                ShowError(ErrorMode.Dialog, StatusCode.Unavailable, "No detected QrCode info available.");
            }
        }

        public void TriggerResetScan()
        {
            Status status = MirrorScene.Get().ResetSceneStream();
            if (!status.IsOk)
            {
                ShowError(ErrorMode.Toast, status, "Failed to reset the scene streaming.");
            }
        }

        public void TriggerStopSceneCapture()
        {
            Status status = MirrorScene.Get().FinishSceneStream();

            if (!status.IsOk)
            {
                ShowError(ErrorMode.Toast, status, "Failed to finish the scene streaming.");
            }
        }

        public Status TriggerShowQrCode()
        {
            string qrCodeExtraData = "";
            if (this.onRequestQRCodeExtraData != null)
            {
                bool isReady;
                (isReady, qrCodeExtraData) = this.onRequestQRCodeExtraData();
                if (isReady == false)
                {
                    ShowError(ErrorMode.Toast, StatusCode.Unavailable, "Failed to show QR code. QR code extra data is requested but not ready.");
                    return StatusCode.Unavailable;
                }
            }
            Status status = MirrorScene.Get().ShowMarker(qrCodeExtraData);
            if (!status.IsOk)
            {
                if (status.Code == StatusCode.FailedPrecondition)
                {
                    ShowError(ErrorMode.Toast, status, "Failed to show QR code. Create or join a scene first.");
                }
                else
                {
                    ShowError(ErrorMode.Toast, status, "Failed to show QR code for the given scene.");
                }
            }
            return status;
        }

        public void TriggerHideQrCode()
        {
            MirrorScene.Get().HideMarker();
        }

        public void TriggerScanQrCode()
        {
            Status status = MirrorScene.Get().StartMarkerDetection();
            if (!status.IsOk)
            {
                ShowError(ErrorMode.Dialog, status, "Failed to start QR Code detection.");
            }
            else
            {
                if (raycastRenderer != null)
                {
                    raycastRenderer.options.cursorVisible = false;
                }
            }
        }

        public void TriggerStopScanQrCode()
        {
            Status status = MirrorScene.Get().StopMarkerDetection();
            if (!status.IsOk)
            {
                ShowError(ErrorMode.Toast, status, "Failed to stop QR Code detection.");
            }
            else
            {
                if (raycastRenderer != null)
                {
                    raycastRenderer.options.cursorVisible = raycastRenderer.options.raycastOnMeshEnabled && staticMeshRenderer.GetMeshObject() != null;
                }
            }
        }

        public void TriggerSceneLocalization()
        {
            _currentTriggerLocalizationState = TriggerLocalizationState.TryTriggerLocalization;

            Status status = MirrorScene.Get().StartLocalization();
            if (status.IsOk)
            {
                _currentTriggerLocalizationState = TriggerLocalizationState.HasTriggerLocalization;
            }
            else
            {
                ShowError(ErrorMode.Toast, status, "Failed to start localization for the scene.");
                Debug.Log($"Current localization trigger state: {_currentTriggerLocalizationState}]");
            }
        }

        public void TriggerCancelSceneCapture()
        {
            Debug.Log($"Scene streaming has been cancelled.");
            ExitScene();
            Cancel();
        }

        public void TriggerReviewScene()
        {
            // Review scene with visible mesh.
            ShowMenu();
            SwitchMenu(SystemMenuType.ConfirmReviewMenu);
        }

        public void TriggerBackClicked(){
            this.onMenuBackClick();
        }

        internal void FireTimeoutEvents(string eventName)
        {
            // Events can be "show", "retry" or "report". No event for "hide".
            switch(eventName)
            {
                case "show":
                    this.onMenuScanSceneTimeout();
                    break;
                case "retry":
                    this.onMenuScanSceneTimeoutRetry();
                    break;
                case "report":
                    this.onMenuScanSceneTimeoutReport();
                    break;
            }
        }

        internal void FireReviewSceneEvents(bool open, bool confirm = false)
        {
            if (open)
            {
                this.onMenuReviewSceneOpen();
            }
            else
            {
                this.onMenuReviewSceneClose(confirm);
            }
        }

        public void ExitScene()
        {
            if (MirrorScene.Get() != null && MirrorScene.Get().GetOperationState() != SceneOperationState.Idle)
            {
                MirrorScene.Get().ExitScene();
            }
            _currentQrCodeInfo = null;
            _currentTriggerLocalizationState = TriggerLocalizationState.Default;
            _currentQrCodeDetectState = QrCodeDetectState.Default;
            if (raycastRenderer != null)
            {
                raycastRenderer.options.cursorVisible = false;
            }
            onMenuExitScene();
        }

        public void Restart()
        {
            if (_userMode == UserMode.Guest)
            {
                RestartToJoin();
            }
            else
            {
                RestartToCreate();
            }
        }

        public void RestartToCreate()
        {
            ExitScene();
            ShowMenu();
            ScanSceneMenu.Instance.ResetMenu();
            _userMode = UserMode.Host;
            _lastSelectedFrameCount = 0; // Reset the selected frame count.
            SwitchMenu(SystemMenuType.ScanSceneMenu);
            // Create a new scene as soon as possible to warm up.
            TriggerCreateScene();
        }

        public void RestartToJoin()
        {
            ExitScene();
            ShowMenu();
            _userMode = UserMode.Guest;
            _lastSelectedFrameCount = 0; // Reset the selected frame count.
            SwitchMenu(SystemMenuType.ScanQrCodeMenu);
            TriggerScanQrCode();
        }

        public void Finish()
        {
            if(MirrorScene.Get() != null && MirrorScene.Get().GetOperationState() == SceneOperationState.Localizing){
                SwitchMenu(SystemMenuType.NoMenu);
                HideMenu();
                onMenuFinish();
            }
        }

        public void Cancel()
        {
            SwitchMenu(SystemMenuType.NoMenu);
            HideMenu();
            onMenuCancel();
        }

        public void DisplayStaticMesh(bool display)
        {
            if (staticMeshRenderer != null)
            {
                staticMeshRenderer.options.withOcclusion = !display;
                staticMeshRenderer.options.receivesShadow = !display;
            }
        }

        public bool HasRecentScene()
        {
            if (_recentScenePref == null)
            {
                Debug.Log($"No recent scene stored in pref.");
                return false;
            }
            DateTimeOffset expireTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(_recentScenePref.expireTimestampMs);
            // Expiration is controlled by core logic. Here compare now and the set expired time.
            if (((DateTimeOffset)DateTime.Now).CompareTo(expireTimestamp) > 0)
            {
                Debug.Log($"Stored recent scene in pref was too old. [{_recentScenePref.sceneId}]");
                _recentScenePref = null;
                return false;
            }
            return true;
        }

        public RecentScenePref GetRecentScenePref()
        {
            return _recentScenePref;
        }

        public void SaveRecentScenePref(SceneInfo scene)
        {
            if (_recentScenePref != null && _recentScenePref.sceneId == scene.sceneId)
            {
                // Already exists, skip saving.
                Debug.Log($"Scene ready, same scene from pref. [{scene.sceneId}]");
            }
            else
            {
                PlayerPrefs.SetString(_RECENT_SCENE_ID_PREF, scene.sceneId);
                PlayerPrefs.SetString(_RECENT_SCENE_UPDATE_TIMESTAMP_PREF, scene.updateTimestamp.ToString());
                PlayerPrefs.SetString(_RECENT_SCENE_EXPIRE_TIMESTAMP_PREF, scene.expireTimestamp.ToString());
                _recentScenePref = LoadRecentScenePref();
                Debug.Log($"Scene ready, stored to pref. [{scene.sceneId}]");
            }
        }

        public RecentScenePref LoadRecentScenePref()
        {
            Debug.Log($"Loading stored recent scene from pref.");
            string sceneId = PlayerPrefs.GetString(_RECENT_SCENE_ID_PREF);
            string updateTimestampString = PlayerPrefs.GetString(_RECENT_SCENE_UPDATE_TIMESTAMP_PREF);
            string expireTimestampString = PlayerPrefs.GetString(_RECENT_SCENE_EXPIRE_TIMESTAMP_PREF);
            if (String.IsNullOrEmpty(sceneId) || String.IsNullOrEmpty(updateTimestampString) || String.IsNullOrEmpty(expireTimestampString))
            {
                Debug.Log($"Stored recent scene not exists.");
                return null;
            }
            return new RecentScenePref()
            {
                sceneId = sceneId,
                updateTimestampMs = long.Parse(updateTimestampString),
                expireTimestampMs = long.Parse(expireTimestampString)
            };
        }
    }
}
