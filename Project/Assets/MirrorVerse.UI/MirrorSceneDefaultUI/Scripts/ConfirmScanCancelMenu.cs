namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ConfirmScanCancelMenu : SubMenu<ConfirmScanCancelMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    DefaultUI.Instance.TriggerCancelSceneCapture();
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
