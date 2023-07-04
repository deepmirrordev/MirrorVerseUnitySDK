namespace MirrorVerse
{
    // Represents object detection results.
    public struct ObjectDetectionResult
    {
        // One or more detected objects' bounding boxes with label and vertices.
        public BoundingBox[] boxDetections;
    }
}
