namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ScanQrCodeMenu : SubMenu<ScanQrCodeMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Back":
                    ClassyUI.Instance.TriggerStopScanQrCode();
                    ClassyUI.Instance.Cancel();
                    ClassyUI.Instance.TriggerBackClicked();
                    break;
                default:
                    break;
            }
        }
    }
}
