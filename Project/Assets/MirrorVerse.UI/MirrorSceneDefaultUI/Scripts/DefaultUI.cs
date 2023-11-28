using System;
using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public enum SystemMenuType
    {
        NoMenu,
        StartMenu,
        ScanSceneMenu,
        ScanQrCodeMenu,
        ProcessingMenu,
        ConfirmScanFinishMenu,
        ConfirmScanCancelMenu,
        ConfirmJoinSceneMenu,
        ConfirmReloadMenu,
        FinishMenu,
    }

    public delegate void OnMenuFinish();
    public delegate void OnMenuCancel();
    public delegate void OnMenuCreateScene();
    public delegate void OnMenuExitScene();
    public delegate void OnMenuLoadRecentScene();
    public delegate void OnMenuJoinScene(string sQRCodeInfo);
    public delegate (bool, string) OnRequestQRCodeExtraData();

    public class DefaultUI : MonoBehaviour
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


        private const string _RECENT_SCENE_ID_PREF = "MIRRORVERSE_RECENT_SCENE_ID";
        private const string _RECENT_SCENE_TIMESTAMP_PREF = "MIRRORVERSE_RECENT_SCENE_TIMESTAMP";

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
            public DateTimeOffset timestamp;
        }

        private IMirrorScene _api;
        private RecentScenePref _recentScenePref;
        private SystemMenuType _currentMenuType = SystemMenuType.NoMenu;
        private TriggerLocalizationState _currentTriggerLocalizationState = TriggerLocalizationState.Default;
        private QrCodeDetectState _currentQrCodeDetectState = QrCodeDetectState.Default;
        private MarkerInfo? _currentQrCodeInfo = null;
        private static DefaultUI _instance;

        public static DefaultUI Instance { get { return _instance; } }

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
                _api = MirrorScene.Get();
                _api.onSceneStandby += OnSceneStandby;
                _api.onMarkerDetected += OnMarkerDetected;
                _api.onSceneReady += OnSceneReady;
                _api.onSceneStreamFinish += OnSceneStreamFinish;
            }
            else
            {
                ToastManager.Instance.Show("MirrorScene API is not available.");
            }
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
                        status = _api.DownloadSceneMesh();
                        if (!status.IsOk)
                        {
                            ToastManager.Instance.Show("Failed to start scene downloading.");
                            Debug.LogWarning($"Failed to start scene downloading. [{status}]");
                        }
                        break;
                    case SceneStatus.Empty:
                    case SceneStatus.Capturing:
                        Debug.Log("Scene is empty or capturing. Start capture stream...");
                        status = _api.StartSceneStream();
                        if (status.IsOk)
                        {
                            if (_currentQrCodeDetectState == QrCodeDetectState.QrCodeDetected)
                            {
                                _currentQrCodeDetectState = QrCodeDetectState.HasTriggerCaptureByQrCode;
                                ScanSceneMenu.Instance.SwitchScanState(ScanState.Scanning);
                            }
                        }
                        else
                        {
                            ToastManager.Instance.Show("Failed to start scene streaming.");
                            Debug.LogWarning($"Failed to start scene streaming. [{status}]");
                        }
                        break;
                }
            }
            else
            {
                ToastManager.Instance.Show("Failed to create or join the scene.");
                Debug.LogWarning($"Failed to create or join the scene. [{sceneInfo.Status}]");
                Restart();
            }
        }

        public void OnSceneStreamFinish(Status status)
        {
            // OnSceneStreamFinish is triggered when capture stream is finished and the scene will be processing.
            if (status.IsOk)
            {
                Debug.Log($"Scene stream finished. [{status}]");
                // Could be triggered by either current device, or others.
                ScanSceneMenu.Instance.ResetMenu();
                ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Processing);
                SwitchMenu(SystemMenuType.ProcessingMenu);
            }
            else if (status.Code == StatusCode.Cancelled)
            {
                ToastManager.Instance.Show("Scene stream cancelled.");
                Debug.LogWarning($"Scene stream cancelled. [{status}]");
                Restart();
            }
            else
            {
                ToastManager.Instance.Show("Scene stream aborted.");
                Debug.LogWarning($"Scene stream aborted. [{status}]");
                Restart();
            }
        }

        public void OnSceneReady(StatusOr<SceneInfo> sceneInfo)
        {
            if (sceneInfo.Status.IsOk)
            {
                SwitchMenu(SystemMenuType.FinishMenu);
                SaveRecentScenePref(sceneInfo.Value.sceneId);
                TriggerSceneLocalization();
            }
            else
            {
                ToastManager.Instance.Show("Scene preparation is failed.");
                Debug.LogWarning($"Failed to prepare the scene. [{sceneInfo.Status}]");
                Restart();
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
                case SystemMenuType.StartMenu:
                    StartMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ScanSceneMenu:
                    ScanSceneMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ConfirmScanFinishMenu:
                    ConfirmScanFinishMenu.Instance.HideMenu();
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
                case SystemMenuType.ConfirmReloadMenu:
                    ConfirmReloadMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.ProcessingMenu:
                    ProcessingMenu.Instance.HideMenu();
                    break;
                case SystemMenuType.FinishMenu:
                    FinishMenu.Instance.HideMenu();
                    break;
            }

            switch (menuType)
            {
                case SystemMenuType.NoMenu:
                    // Show nothing.
                    _currentMenuType = SystemMenuType.NoMenu;
                    break;
                case SystemMenuType.StartMenu:
                    if (StartMenu.Instance != null && StartMenu.Instance.gameObject.activeSelf)
                    {
                        StartMenu.Instance.ShowMenu();
                        _currentMenuType = SystemMenuType.StartMenu;
                    }
                    else
                    {
                        // Note: if developer deactivate the StartMenu prefab, then skip it and directly call Cancel()
                        // as developer will use their own start UI screen.
                        Cancel();
                    }
                    break;
                case SystemMenuType.ScanSceneMenu:
                    ScanSceneMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ScanSceneMenu;
                    break;
                case SystemMenuType.ConfirmScanFinishMenu:
                    ConfirmScanFinishMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmScanFinishMenu;
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
                case SystemMenuType.ConfirmReloadMenu:
                    ConfirmReloadMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ConfirmReloadMenu;
                    break;
                case SystemMenuType.ProcessingMenu:
                    ProcessingMenu.Instance.ShowMenu();
                    _currentMenuType = SystemMenuType.ProcessingMenu;
                    break;
                case SystemMenuType.FinishMenu:
                    if (FinishMenu.Instance != null && FinishMenu.Instance.gameObject.activeSelf)
                    {
                        FinishMenu.Instance.ShowMenu();
                        _currentMenuType = SystemMenuType.FinishMenu;
                    }
                    else
                    {
                        // Note: if developer deactivate the FinishMenu prefab, then skip it and directly call Finish().
                        Finish();
                    }
                    break;
            }
        }

        public Status TriggerCreateScene()
        {
            Status status = _api.CreateScene();
            if (status.IsOk)
            {
                this.onMenuCreateScene();
            }
            else
            {
                ToastManager.Instance.Show("Failed to create a new scene.");
                Debug.LogWarning($"Failed to create a new scene. [{status}]");
            }
            return status;
        }

        public Status TriggerJoinScene(string sceneId)
        {
            Status status = _api.JoinScene(sceneId);
            if (!status.IsOk)
            {
                ToastManager.Instance.Show("Failed to join the given scene.");
                Debug.LogWarning($"Failed to join the given scene. [{status}, {sceneId}]");
            }
            return status;
        }

        public void TriggerLoadRecentScene()
        {
            if (HasRecentScene())
            {
                Debug.Log($"Joining the recent scene. [{_recentScenePref.sceneId}]");
                Status status = TriggerJoinScene(_recentScenePref.sceneId);
                if (status.IsOk)
                {
                    this.onMenuLoadRecentScene();
                }
            }
            else
            {
                ToastManager.Instance.Show("No recent scene available.");
                Debug.Log($"No recent scene available on this device.");
            }
        }

        public void TriggerJoinSceneFromQrCode()
        {
            if (_currentQrCodeInfo != null)
            {
                Debug.Log($"Joining the scene from detected QR code. [{_currentQrCodeInfo.Value.sceneId}]");
                Status status = TriggerJoinScene(_currentQrCodeInfo.Value.sceneId);
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
                Debug.Log($"No detected QrCode info available.");
            }
        }

        public void TriggerResetScan()
        {
            Status status = _api.ResetSceneStream();
            if (!status.IsOk)
            {
                ToastManager.Instance.Show("Failed to reset the scene streaming.");
                Debug.LogWarning($"Failed to reset the scene streaming. [{status}]");
            }
        }

        public void TriggerStopSceneCapture()
        {
            Status status = _api.FinishSceneStream();

            if (!status.IsOk)
            {
                ToastManager.Instance.Show("Failed to finish the scene streaming.");
                Debug.LogWarning($"Failed to finish the scene streaming. [{status}]");
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
                    ToastManager.Instance.Show("Failed to show QR code. QR code extra data is requested but not ready.");
                    return StatusCode.Unavailable;
                }
            }
            Status status = _api.ShowMarker(qrCodeExtraData);
            if (!status.IsOk)
            {
                if (status.Code == StatusCode.FailedPrecondition)
                {
                    ToastManager.Instance.Show("Failed to show QR code. Create or join a scene first.");
                }
                else
                {
                    ToastManager.Instance.Show("Failed to show QR code for the given scene.");
                }
                Debug.LogWarning($"Failed to show QR code. [{status}]");
            }
            return status;
        }

        public void TriggerHideQrCode()
        {
            _api.HideMarker();
        }

        public void TriggerScanQrCode()
        {
            Status status = _api.StartMarkerDetection();
            if (!status.IsOk)
            {
                ToastManager.Instance.Show("Failed to start QR Code detection.");
                Debug.LogWarning($"Failed to start QR Code detection [{status}]");
            }
        }

        public void TriggerSceneLocalization()
        {
            _currentTriggerLocalizationState = TriggerLocalizationState.TryTriggerLocalization;

            Status status = _api.StartLocalization();
            if (status.IsOk)
            {
                _currentTriggerLocalizationState = TriggerLocalizationState.HasTriggerLocalization;
            }
            else
            {
                ToastManager.Instance.Show("Failed to start localization for the scene.");
                Debug.LogWarning($"Failed to start localization for the scene. [{status}] [{_currentTriggerLocalizationState}]");
            }
        }

        public void TriggerCancelSceneCapture()
        {
            Debug.Log($"Scene streaming has been cancelled.");
            Restart();
        }

        public void ExitScene()
        {
            if (_api != null && _api.GetOperationState() != SceneOperationState.Idle)
            {
                _api.ExitScene();
            }
            _currentQrCodeInfo = null;
            _currentTriggerLocalizationState = TriggerLocalizationState.Default;
            _currentQrCodeDetectState = QrCodeDetectState.Default;

            onMenuExitScene();
        }

        public void Restart()
        {
            ExitScene();
            ShowMenu();
            ScanSceneMenu.Instance.ResetMenu();
            SwitchMenu(SystemMenuType.StartMenu);
        }

        public void RestartToCreate()
        {
            ExitScene();
            ShowMenu();
            ScanSceneMenu.Instance.ResetMenu();
            SwitchMenu(SystemMenuType.ScanSceneMenu);
        }

        public void RestartToJoin()
        {
            ExitScene();
            ShowMenu();
            SwitchMenu(SystemMenuType.ScanQrCodeMenu);
            TriggerScanQrCode();
        }

        public void Finish()
        {
            SwitchMenu(SystemMenuType.NoMenu);
            HideMenu();
            onMenuFinish();
        }

        public void Cancel()
        {
            SwitchMenu(SystemMenuType.NoMenu);
            HideMenu();
            onMenuCancel();
        }

        public bool HasRecentScene()
        {
            return _recentScenePref != null;
        }

        public RecentScenePref GetRecentScenePref()
        {
            return _recentScenePref;
        }

        public void SaveRecentScenePref(string sceneId)
        {
            DateTimeOffset timestamp = DateTime.Now;
            PlayerPrefs.SetString(_RECENT_SCENE_TIMESTAMP_PREF, timestamp.ToUnixTimeMilliseconds().ToString());
            PlayerPrefs.SetString(_RECENT_SCENE_ID_PREF, sceneId);
            _recentScenePref = new RecentScenePref()
            {
                sceneId = sceneId,
                timestamp = timestamp
            };
            Debug.Log($"Scene ready, stored to pref. [{sceneId}]");
        }

        public RecentScenePref LoadRecentScenePref()
        {
            Debug.Log($"Loading stored recent scene from pref.");
            string sceneId = PlayerPrefs.GetString(_RECENT_SCENE_ID_PREF);
            if (String.IsNullOrEmpty(sceneId))
            {
                Debug.Log($"Stored recent scene not exists.");
                return null;
            }

            string epochString = PlayerPrefs.GetString(_RECENT_SCENE_TIMESTAMP_PREF);
            if (String.IsNullOrEmpty(epochString))
            {
                Debug.Log($"Stored recent scene timestamp not exists.");
                return null;
            }
            DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(epochString));
            if (((DateTimeOffset)DateTime.Now).CompareTo(timestamp.AddHours(24)) > 0)
            {
                // Recent scene was captured earlier than 24 hours ago, skip.
                Debug.Log($"Stored recent scene in pref was too old. [{sceneId}]");
                return null;
            }
            return new RecentScenePref()
            {
                sceneId = sceneId,
                timestamp = timestamp
            };
        }
    }
}
