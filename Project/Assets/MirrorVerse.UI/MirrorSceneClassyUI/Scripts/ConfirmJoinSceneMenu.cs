namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmJoinSceneMenu : SubMenu<ConfirmJoinSceneMenu>
    {
        public bool requireReview = true;

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    ClassyUI.Instance.TriggerJoinSceneFromQrCode(requireReview);
                    break;
                case "Cancel":
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanQrCodeMenu);
                    ClassyUI.Instance.TriggerScanQrCode();
                    break;
                default:
                    break;
            }
        }
    }
}
