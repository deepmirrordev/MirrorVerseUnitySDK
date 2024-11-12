using System;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmLoadMenu : SubMenu<ConfirmLoadMenu>
    {
        public Text timestampText;
        public Button loadButton;

        public override void ShowMenu()
        {
            base.ShowMenu();
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeMilliseconds(ClassyUI.Instance.GetRecentScenePref().updateTimestampMs);
            string format = timestampText.text;
            // TODO: date format should be also localized.
            timestampText.text = string.Format(format, dateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            loadButton.enabled = ClassyUI.Instance.HasRecentScene();
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Load":
                    ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Downloading);
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ProcessingMenu);
                    ClassyUI.Instance.TriggerLoadRecentScene();
                    break;
                case "Cancel":
                    ClassyUI.Instance.SwitchMenu(SystemMenuType.ScanSceneMenu);
                    break;
                default:
                    break;
            }
        }
    }
}
