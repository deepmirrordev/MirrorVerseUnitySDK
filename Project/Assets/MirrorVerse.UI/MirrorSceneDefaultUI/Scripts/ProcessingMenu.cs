using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public enum ProcessingState
    {
        Processing,
        Downloading
    }

    public class ProcessingMenu : SubMenu<ProcessingMenu>
    {
        public Text processingText;

        public void UpdateProcessingText(ProcessingState state)
        {
            switch (state)
            {
                case ProcessingState.Processing:
                    processingText.text = "Processing...";
                    break;
                case ProcessingState.Downloading:
                    processingText.text = "Downloading...";
                    break;
            }
        }
    }
}
