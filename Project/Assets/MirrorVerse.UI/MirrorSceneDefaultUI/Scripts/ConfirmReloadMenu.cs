using System;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ConfirmReloadMenu : SubMenu<ConfirmReloadMenu>
    {
        public Text timestampText;
        public Button confirmButton;

        public override void ShowMenu()
        {
            base.ShowMenu();
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeMilliseconds(DefaultUI.Instance.GetRecentScenePref().updateTimestampMs);
            timestampText.text = "Created at " + dateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");

            confirmButton.enabled = DefaultUI.Instance.HasRecentScene();
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Downloading);
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ProcessingMenu);
                    DefaultUI.Instance.TriggerLoadRecentScene();
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
