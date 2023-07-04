using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class StartMenu : SubMenu<StartMenu>
    {
        public bool destoryCustomPanelOnHide = true;
        public GameObject customPanelPrefab;
        private GameObject _customPanelInstance;

        public override void ShowMenu()
        {
            base.ShowMenu();
            if (customPanelPrefab != null && _customPanelInstance == null)
            {
                // Create panel instance only if prefab exists and panel is not created.
                Canvas canvas = canvasGroup.GetComponentInChildren<Canvas>();
                _customPanelInstance = Instantiate(customPanelPrefab, canvas.transform);
                // the bottom action panel is overlayed on top of custom panel.
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
                case "CreateArGame":
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    break;
                case "JoinArGame":
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ScanQrCodeMenu);
                    DefaultUI.Instance.TriggerScanQrCode();
                    break;
                case "Back":
                    DefaultUI.Instance.Cancel();
                    break;
                default:
                    break;
            }
        }
    }
}
