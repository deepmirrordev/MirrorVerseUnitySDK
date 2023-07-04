using System;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public class ConfirmReloadMenu : SubMenu<ConfirmReloadMenu>
    {
        public Text timestampText;

        public override void ShowMenu()
        {
            base.ShowMenu();
            DateTimeOffset dateTime = DefaultUI.Instance.GetRecentScenePref().timestamp;
            timestampText.text = "Created at " + dateTime.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss");
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Confirm":
                    ProcessingMenu.Instance.UpdateProcessingText(ProcessingState.Downloading);
                    DefaultUI.Instance.SwitchMenu(SystemMenuType.ProcessingMenu);
                    DefaultUI.Instance.TriggerJoinRecentScene();
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
