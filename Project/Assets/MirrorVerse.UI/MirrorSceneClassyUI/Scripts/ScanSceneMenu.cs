using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public enum ScanState
    {
        Idle,     // Initiate state, creating scene.
        Standby,  // Scene created. Standing by.
        Scanning, // Scene scan started.
    }

    // Sub states under Scanning State.
    public enum ScanningSubState
    {
        Inactive,
        Normal,
        Limit
    }

    public class ScanSceneMenu : SubMenu<ScanSceneMenu>
    {
        public GameObject backButton;
        public GameObject reloadRecentButton;
        public GameObject scanButtonLabel;
        public GameObject scanButton;
        public GameObject scanningButton;
        public GameObject scanningInactiveButton;
        public GameObject scanningLimitButton;

        public GameObject forceToFinishTip;
        public GameObject readyToFinishTip;
        public GameObject scanningTip;
        public GameObject timeoutTip;
        public GameObject timeoutReportButton;

        public int minimumRegisteredImages = 7; // At least this number of registered images before scan can be finished immediately.
        public int minimumMeshFases = 1500;  // At least this number of mesh faces show up before scan can be finished immediately.
        public int minimumMeshFasesWithDuration = 800;  // At least this number of mesh faces show up before scan can be finished after a certain duration.
        public float minimumDuration = 10.0f; // At least this amount of time has pass before scan can be finished if enough mesh already shows up.
        public float timeoutDuration = 15.0f; // If no mesh at all and this amount of time has pass, trigger a timeout.
        public bool supportIssueReport = false; // Whether to show a report button if encounters issues. Application need to support a reporting flow.
        public int captureHardLimit = 300; // Auto finish will happen when this number of frames have captured.
        public int captureWarningLimit = 250;   // If more than this number of frames are captured, show some warning as it is approaching the limit.
        public float tipDuration = 5.0f;  // Seconds of tip displaying on screen.
        public float backButtonDelay = 2.0f;  // Seconds of delay of backend button displaying after open.

        private int _currentTipIndex = -1;
        private List<GameObject> _allTips = new(5);
        
        private ScanState _scanState = ScanState.Idle;
        private ScanningSubState _scanningStubState = ScanningSubState.Inactive;
        private bool _isQrCodeShow = false;
        private int _lastRegisteredImageCount = 0;
        private int _lastImmediateMeshFacesCount = 0;
        private float _lastScanStartTimestamp = float.MaxValue;
        private float _lastTimeoutTimestamp = float.MaxValue;
        private float _lastShownTimestamp = float.MaxValue;
        private FrameSelectionResult.WarningType _lastWarningType = FrameSelectionResult.WarningType.None;

        public override void Awake()
        {
            base.Awake();
            // Prioritized tips. Low -> High. No tip = -1.
            _allTips.Add(scanningTip);
            _allTips.Add(readyToFinishTip);
            _allTips.Add(forceToFinishTip);
        }

        private void Start()
        {
            // Only show report button if issue report is supported.
            timeoutReportButton.SetActive(supportIssueReport);
        }

        private void OnEnable()
        {
            if (MirrorScene.IsAvailable())
            {
                MirrorScene.Get().onSceneStandby += OnSceneStandby;
                MirrorScene.Get().onSceneStreamUpdate += OnSceneStreamUpdate;
            }
        }

        private void OnDisable()
        {
            if (MirrorScene.IsAvailable())
            {
                MirrorScene.Get().onSceneStandby -= OnSceneStandby;
                MirrorScene.Get().onSceneStreamUpdate -= OnSceneStreamUpdate;
            }
        }

        private void Update()
        {
            if (!backButton.activeSelf && Time.time - _lastShownTimestamp > backButtonDelay)
            {
                // Show back button after given delay.
                backButton.SetActive(true);
            }

            // Require more than minimum registered image to finish.
            if (_lastRegisteredImageCount > minimumRegisteredImages)
            {
                if (_lastImmediateMeshFacesCount > minimumMeshFases)
                {
                    // If more than minimimMeshFaces
                    SwitchScanningSubState(ScanningSubState.Normal);
                }
                // Check if scan time has long enough after immediate mesh appears.
                else if (Time.time - _lastScanStartTimestamp > minimumDuration && _lastImmediateMeshFacesCount > minimumMeshFasesWithDuration)
                {
                    // If more than minimumMeshFasesWithDuration after minimumDuration.
                    SwitchScanningSubState(ScanningSubState.Normal);
                }
            }

            // Check if scan time has long enough and no immediate mesh appears.
            if (_lastImmediateMeshFacesCount <= 0 && Time.time - _lastTimeoutTimestamp > timeoutDuration && _scanningStubState == ScanningSubState.Inactive)
            {
                ShowTimeoutTip();
            }
            else if (_scanningStubState != ScanningSubState.Inactive)
            {
                HideTimeoutTip();
            }
        }

        private void OnSceneStandby(StatusOr<SceneInfo> sceneInfo)
        {
            SwitchScanState(ScanState.Standby);
        }

        private void OnSceneStreamUpdate(StatusOr<FrameSelectionResult> frameSelectionResult, StatusOr<SceneStreamRenderable> streamRenderable)
        {
            // Check if there are enough mesh faces.
            if (streamRenderable.HasValue)
            {
                if (streamRenderable.Value.immediateMesh != null)
                {
                    int meshFaceCount = GetMeshFacesCount(streamRenderable.Value.immediateMesh);
                    _lastImmediateMeshFacesCount = meshFaceCount;
                    _lastRegisteredImageCount = streamRenderable.Value.registeredImageCount;
                    Debug.Log($"Immediate meshe face count {meshFaceCount}, registered image count {_lastRegisteredImageCount}.");
                }
            }
            // Check if there are warnings or too many frames captured.
            if (frameSelectionResult.HasValue)
            {
                if (frameSelectionResult.Value.selectedFrameCount > captureWarningLimit)
                {
                    SwitchScanningSubState(ScanningSubState.Limit);
                }

                if (frameSelectionResult.Value.selectedFrameCount >= captureHardLimit - 1)
                {
                    // Reaches the limit. Prepare to show warning and stop.
                    MirrorScene.Get().SkipCaptureFrames(true);
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanLimitMenu);
                }

                if (frameSelectionResult.Value.warningType != FrameSelectionResult.WarningType.None)
                {
                    // Check warnings. Currently not showing any warning tips.
                    if (frameSelectionResult.Value.warningType != _lastWarningType)
                    {
                        _lastWarningType = frameSelectionResult.Value.warningType;
                    }
                }
            }
        }

        public override void ShowMenu()
        {
            _lastShownTimestamp = Time.time;
            backButton.SetActive(false);
            base.ShowMenu();
        }

        public void ResetMenu()
        {
            // 1. Set scan icon back to default texture
            // 2. Reset all states like _scanState
            SwitchScanState(ScanState.Idle);
        }

        private int GetMeshFacesCount(Mesh mesh)
        {
            if (mesh != null)
            {
                return mesh.triangles.Length / 3;
            }
            return 0;
        }

        private void SwitchScanningSubState(ScanningSubState subState)
        {
            switch (subState)
            {
                case ScanningSubState.Normal:
                    if (!scanningButton.activeSelf && _scanningStubState == ScanningSubState.Inactive)
                    {
                        _scanningStubState = ScanningSubState.Normal;
                        ToggleScanButtonLabel();
                        MaybeShowTip(readyToFinishTip);

                        Debug.Log($"Scene scan is allowed to finish.");
                    }
                    break;
                case ScanningSubState.Limit:
                    if (!scanningLimitButton.activeSelf && _scanningStubState == ScanningSubState.Normal)
                    {
                        _scanningStubState = ScanningSubState.Limit;
                        ToggleScanButtonLabel();
                        MaybeShowTip(forceToFinishTip);

                        Debug.Log($"Scene scan is reaching to limit");
                    }
                    break;
                case ScanningSubState.Inactive:
                    _scanningStubState = ScanningSubState.Inactive;
                    break;
            }
        }

        private void ToggleScanButtonLabel()
        {
            // Disable button interaction if in idle state when scene is still creating and not standingby.
            scanButton.GetComponent<Button>().interactable = _scanState != ScanState.Idle;

            scanButton.SetActive(_scanState != ScanState.Scanning);
            scanButtonLabel.SetActive(_scanState != ScanState.Scanning);

            scanningButton.SetActive(_scanState == ScanState.Scanning && _scanningStubState == ScanningSubState.Normal);
            scanningInactiveButton.SetActive(_scanState == ScanState.Scanning && _scanningStubState == ScanningSubState.Inactive);
            scanningLimitButton.SetActive(_scanState == ScanState.Scanning && _scanningStubState == ScanningSubState.Limit);
        }

        private void ToggleReloadRecentButton()
        {
            reloadRecentButton.SetActive(_scanState == ScanState.Standby && ClassyUI.Instance.HasRecentScene());
        }
        
        private void MaybeShowTip(GameObject tipObj, bool autoHide = true)
        {
            if (tipObj != null)
            {
                int showingIndex = _allTips.IndexOf(tipObj);
                if (showingIndex > _currentTipIndex)
                {
                    if (_currentTipIndex != -1)
                    {
                        _allTips[_currentTipIndex].SetActive(false);
                    }
                    _currentTipIndex = showingIndex;
                    tipObj.SetActive(true);
                    if (autoHide)
                    {
                        StartCoroutine(DelayHideTip(tipObj));
                    }
                }
            }
            else
            {
                if (_currentTipIndex != -1)
                {
                    _allTips[_currentTipIndex].SetActive(false);
                }
                _currentTipIndex = -1;
            }
        }

        private IEnumerator DelayShowTip(GameObject tipObj, bool autoHide = true)
        {
            yield return new WaitForSeconds(2);
            MaybeShowTip(tipObj, autoHide);
        }

        private IEnumerator DelayHideTip(GameObject tipObj)
        {
            yield return new WaitForSeconds(tipDuration);
            tipObj.SetActive(false);
            int hidingIndex = _allTips.IndexOf(tipObj);
            if (hidingIndex == _currentTipIndex)
            {
                _currentTipIndex = -1;
            }
        }

        private void HideAllTips()
        {
            foreach(var tip in _allTips)
            { 
                if (tip != null)
                {
                    tip.SetActive(false);
                }
            }
            _currentTipIndex = -1;
        }

        private void ShowTimeoutTip()
        {
            if (!timeoutTip.activeSelf)
            {
                Debug.Log("Showing no mesh timeout tip.");
                timeoutTip.SetActive(true);
                ClassyUI.Instance.FireTimeoutEvents("show");
            }
        }

        private void HideTimeoutTip()
        {
            if (timeoutTip.activeSelf)
            {
                Debug.Log("Hiding no mesh timeout tip.");
                _lastTimeoutTimestamp = Time.time;
                timeoutTip.SetActive(false);
            }
        }

        public void SwitchScanState(ScanState state)
        {
            _scanState = state;
            ToggleScanButtonLabel();
            ToggleReloadRecentButton();
            switch (state)
            {
                case ScanState.Idle:
                case ScanState.Standby:
                    _scanningStubState = ScanningSubState.Inactive;
                    _lastScanStartTimestamp = float.MaxValue;
                    _lastTimeoutTimestamp = float.MaxValue;
                    _lastImmediateMeshFacesCount = 0;
                    _lastRegisteredImageCount = 0;
                    // reset all tips.
                    HideAllTips();
                    HideTimeoutTip();
                    break;
                case ScanState.Scanning:
                    // When just switch state, always show inactive button first. The active button is shown after first stream packet has enough mesh.
                    _scanningStubState = ScanningSubState.Inactive;
                    _lastScanStartTimestamp = Time.time;
                    _lastTimeoutTimestamp = Time.time;
                    _lastImmediateMeshFacesCount = 0;
                    _lastRegisteredImageCount = 0;

                    // Always show scanning tip after scanning started.
                    StartCoroutine(DelayShowTip(scanningTip, false));
                    break;
            }
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "ScanOrStop":
                    if (_scanState == ScanState.Standby)
                    {
                        SwitchScanState(ScanState.Scanning);
                        ClassyUI.Instance.TriggerStartSceneStreaming();
                    }
                    else
                    {
                        // Skip capturing during finish confirmation.
                        MirrorScene.Get().SkipCaptureFrames(true);
                        ClassyUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanFinishMenu);
                    }
                    break;
                case "ShowQrCode":
                    if (!_isQrCodeShow)
                    {
                        if (ClassyUI.Instance.TriggerShowQrCode().IsOk)
                        {
                            _isQrCodeShow = true;
                        }
                    }
                    else
                    {
                        ClassyUI.Instance.TriggerHideQrCode();
                        _isQrCodeShow = false;
                    }
                    break;
                case "Reset":
                    // Start the stream over. So continue to capture.
                    ClassyUI.Instance.TriggerResetScan();
                    break;
                case "ReloadRecent":
                    // Load the recent finished scene.
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ConfirmLoadMenu);
                    break;
                case "Back":
                    if (_scanState == ScanState.Scanning)
                    {
                        // Skip capturing during cancel confirmation.
                        MirrorScene.Get().SkipCaptureFrames(true);
                        ClassyUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanCancelMenu);
                    }
                    else
                    {
                        ClassyUI.Instance.Cancel();
                    }
                    ClassyUI.Instance.TriggerBackClicked();
                    break;
                case "CloseTimeout":
                    HideTimeoutTip();
                    break;
                case "Retry":
                    HideTimeoutTip();
                    // Start over.
                    Debug.Log("Retry the scanning because nothing was scanned.");
                    ClassyUI.Instance.FireTimeoutEvents("retry");
                    ClassyUI.Instance.RestartToCreate();
                    break;
                case "Report":
                    HideTimeoutTip();
                    // Also start over. App could trigger any reporting manner.
                    Debug.Log("Report the scanning because nothing was scanned.");
                    ClassyUI.Instance.FireTimeoutEvents("report");
                    ClassyUI.Instance.RestartToCreate();
                    break;
                default:
                    break;
            }
        }
    }
}
