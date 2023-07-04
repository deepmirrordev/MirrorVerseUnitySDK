using UnityEngine;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public enum ScanState
    {
        Default,
        Scanning,
    }

    public class ScanSceneMenu : SubMenu<ScanSceneMenu>
    {
        public Button reloadRecentButton;
        public Image scanButtonImage;
        public Sprite defaultScanButtonTexture;
        public Sprite scanningButtonTexture;

        private ScanState _scanState = ScanState.Default;
        private bool _isQrCodeShow = false;

        public override void ShowMenu()
        {
            base.ShowMenu();
            reloadRecentButton.interactable = DefaultUI.Instance.HasRecentScene();
        }

        public void ResetMenu()
        {
            // 1. Set scan icon back to default texture
            // 2. Reset all states like _scanState
            SwitchScanState(ScanState.Default);
        }

        public void SwitchScanState(ScanState state)
        {
            switch (state)
            {
                case ScanState.Default:
                    scanButtonImage.sprite = defaultScanButtonTexture;
                    reloadRecentButton.interactable = DefaultUI.Instance.HasRecentScene();
                    _scanState = ScanState.Default;
                    break;
                case ScanState.Scanning:
                    scanButtonImage.sprite = scanningButtonTexture;
                    reloadRecentButton.interactable = false;
                    _scanState = ScanState.Scanning;
                    break;
            }
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "ScanOrStop":
                    if (_scanState == ScanState.Default)
                    {
                        SwitchScanState(ScanState.Scanning);
                        DefaultUI.Instance.TriggerCreateScene();
                    }
                    else
                    {
                        DefaultUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanFinishMenu);
                    }
                    break;
                case "ShowQrCode":
                    if (!_isQrCodeShow)
                    {
                        if (DefaultUI.Instance.TriggerShowQrCode())
                        {
                            _isQrCodeShow = true;
                        }
                    }
                    else
                    {
                        DefaultUI.Instance.TriggerHideQrCode();
                        _isQrCodeShow = false;
                    }
                    break;
                case "Reset":
                    // Start the stream over. So continue to capture.
                    DefaultUI.Instance.TriggerResetScan();
                    break;
                case "ReloadRecent":
                    // Load the recent finished scene.
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ConfirmReloadMenu);
                    break;
                case "Back":
                    if (_scanState == ScanState.Scanning)
                    {
                        DefaultUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanCancelMenu);
                    }
                    else
                    {
                        DefaultUI.Instance.SwitchMenu(SystemMenuType.StartMenu);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
