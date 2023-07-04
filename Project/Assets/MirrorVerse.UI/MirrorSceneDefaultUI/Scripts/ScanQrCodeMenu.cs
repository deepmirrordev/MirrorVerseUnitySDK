namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ScanQrCodeMenu : SubMenu<ScanQrCodeMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Back":
                    DefaultUI.Instance.Restart();
                    break;
                default:
                    break;
            }
        }
    }
}
