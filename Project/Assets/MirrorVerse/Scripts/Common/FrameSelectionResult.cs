namespace MirrorVerse
{
    // Represents frame selection result from camera captures.
    public struct FrameSelectionResult
    {
        public enum SelectionResultType
        {
            // Frame is selected.
            Selected,
            // Frame is not selected because pose is too similar to a selected frame.
            SimilarPose,
            // Frame is not selected because the image is too blurry.
            BlurryImage,
            // Frame is not selected because of other reasons.
            Unselected
        }
        public SelectionResultType selectionResultType;


        public enum WarningType
        {
            // No warning emitted.
            None = 0,
            // Environment is too dark to select frames.
            LowBrightness,
            // Connectivity is poor.
            PoorConnectivity,
            // Other reason for low selection. User maybe not moving camera or too blur or featureless.
            LowSelectionRate
        }
        public WarningType warningType;

        // Number of selected frames.
        public int selectedFrameCount;
    }
}
