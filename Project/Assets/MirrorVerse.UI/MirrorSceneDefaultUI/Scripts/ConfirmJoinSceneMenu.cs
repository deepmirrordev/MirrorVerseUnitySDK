using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ConfirmJoinSceneMenu : SubMenu<ConfirmJoinSceneMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Join":
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    DefaultUI.Instance.TriggerJoinSceneFromQrCode();
                    break;
                case "Cancel":
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ScanQrCodeMenu);
                    DefaultUI.Instance.TriggerScanQrCode();
                    break;
                default:
                    break;
            }
        }
    }
}
