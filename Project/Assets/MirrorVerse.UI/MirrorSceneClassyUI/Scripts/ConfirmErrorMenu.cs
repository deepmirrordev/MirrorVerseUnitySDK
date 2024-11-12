using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class ConfirmErrorMenu : SubMenu<ConfirmErrorMenu>
    {
        public GameObject backButton;
        public float allowCancelPeriod = 1.0f;  // Period of seconds allowing cancel if showing repeatedly.

        public Text messageText;
        public Text suggestionText;

        private float _lastShownTimestamp = float.MaxValue;

        public override void ShowMenu()
        {
            base.ShowMenu();
            if (Time.time - _lastShownTimestamp < allowCancelPeriod)
            {
                // If error menu showing again within given period, allow user to cancel the flow.
                backButton.SetActive(true);
            }
            else
            {
                backButton.SetActive(false);
            }
            _lastShownTimestamp = Time.time;
        }

        public void SetErrorMessage(string message, string suggestion = "Please try again later.")
        {
            messageText.text = message;
            suggestionText.text = suggestion;
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Redo":
                    ClassyUI.Instance.Restart();
                    break;
                case "Back":
                    // Cancel from error dialog, if error always occurs.
                    ClassyUI.Instance.Cancel();
                    break;
                default:
                    break;
            }
        }
    }
}
