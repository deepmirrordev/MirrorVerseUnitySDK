using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmReviewMenu : SubMenu<ConfirmReviewMenu>
    {
        public override void ShowMenu()
        {
            base.ShowMenu();
            ClassyUI.Instance.FireReviewSceneEvents(true);
            ClassyUI.Instance.DisplayStaticMesh(true);
        }

        public override void HideMenu()
        {
            ClassyUI.Instance.DisplayStaticMesh(false);
            base.HideMenu();
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    ClassyUI.Instance.FireReviewSceneEvents(false, true);
                    ClassyUI.Instance.Finish();
                    break;
                case "Rescan":
                    ClassyUI.Instance.FireReviewSceneEvents(false, false);
                    ClassyUI.Instance.RestartToCreate();
                    break;
                case "Back":
                    ClassyUI.Instance.FireReviewSceneEvents(false, false);
                    ClassyUI.Instance.Cancel();
                    ClassyUI.Instance.TriggerBackClicked();
                    break;
                default:
                    break;
            }
        }
    }
}
