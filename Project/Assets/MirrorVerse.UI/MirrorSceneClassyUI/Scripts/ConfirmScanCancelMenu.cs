using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmScanCancelMenu : SubMenu<ConfirmScanCancelMenu>
    {
        public void ClickButton(string buttonName)
        {
            SceneOperationState state = MirrorScene.Get().GetOperationState();
            switch (buttonName)
            {
                case "Abandon":
                    Debug.Log($"Abandon scene at state {state}.");
                    if (state == SceneOperationState.Streaming)
                    {
                        ClassyUI.Instance.RestartToCreate();
                    }
                    else
                    {
                        // Go back and restart with last entry point.
                        ClassyUI.Instance.Restart();
                    }
                    break;
                case "Resume":
                    Debug.Log($"Resume scene cancellation at state {state}.");
                    if (state == SceneOperationState.Streaming)
                    {
                        // Go back and continue scanning.
                        ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                        MirrorScene.Get().SkipCaptureFrames(false);
                    }
                    else if (state == SceneOperationState.Processing || state == SceneOperationState.Downloading)
                    {
                        // Go back and continue waiting for progress.
                        ClassyUI.Instance.SwitchMenu(SystemMenuType.ProcessingMenu);
                    }
                    else
                    {
                        Debug.Log($"Unexpected resuming. Skip.");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
