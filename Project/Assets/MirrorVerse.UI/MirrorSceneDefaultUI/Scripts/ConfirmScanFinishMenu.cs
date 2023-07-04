namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ConfirmScanFinishMenu : SubMenu<ConfirmScanFinishMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    DefaultUI.Instance.TriggerStopSceneCapture();
                    break;
                case "Cancel":
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    break;
                default:
                    break;
            }
        }

    }
}
