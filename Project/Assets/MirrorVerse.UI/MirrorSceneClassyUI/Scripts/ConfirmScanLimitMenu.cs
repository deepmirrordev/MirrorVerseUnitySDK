namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmScanLimitMenu : SubMenu<ConfirmScanLimitMenu>
    {
        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Next":
                    ClassyUI.Instance.TriggerStopSceneCapture();
                    break;
                case "Rescan":
                    ClassyUI.Instance.TriggerCancelSceneCapture();
                    break;
                default:
                    break;
            }
        }
    }
}
