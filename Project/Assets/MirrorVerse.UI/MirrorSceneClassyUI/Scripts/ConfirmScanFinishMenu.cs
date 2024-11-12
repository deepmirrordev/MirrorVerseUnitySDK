namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmScanFinishMenu : SubMenu<ConfirmScanFinishMenu>
    {
        public override void ShowMenu()
        {
            base.ShowMenu();
            ClassyUI.Instance.FireReviewSceneEvents(true);
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    ClassyUI.Instance.FireReviewSceneEvents(false, true);
                    ClassyUI.Instance.TriggerStopSceneCapture();
                    break;
                case "Resume":
                    ClassyUI.Instance.FireReviewSceneEvents(false, false);
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    MirrorScene.Get().SkipCaptureFrames(false);
                    break;
                case "Back":
                    ClassyUI.Instance.FireReviewSceneEvents(false, false);
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    MirrorScene.Get().SkipCaptureFrames(false);
                    ClassyUI.Instance.TriggerBackClicked();

                    break;
                default:
                    break;
            }
        }
    }
}
