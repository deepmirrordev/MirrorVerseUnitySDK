using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class FinishMenu : SubMenu<FinishMenu>
    {
        public bool destoryCustomPanelOnHide = true;
        public GameObject customPanelPrefab;
        private GameObject _customPanelInstance;
        private bool _isQrCodeShow = false;

        public override void ShowMenu()
        {
            base.ShowMenu();
            if (customPanelPrefab != null && _customPanelInstance == null)
            {
                // Create panel instance only if prefab exists and panel is not created.
                Canvas canvas = canvasGroup.GetComponentInChildren<Canvas>();
                _customPanelInstance = Instantiate(customPanelPrefab, canvas.transform);
                _customPanelInstance.transform.SetAsFirstSibling();
            }
        }

        public override void HideMenu()
        {
            base.HideMenu();
            if (_customPanelInstance != null && destoryCustomPanelOnHide)
            {
                Destroy(_customPanelInstance);
                _customPanelInstance = null;
            }
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Back":
                    DefaultUI.Instance.Restart();
                    break;
                case "Finish":
                    DefaultUI.Instance.Finish();
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
                default:
                    break;
            }
        }
    }
}
