namespace MirrorVerse
{

    // Enum of scene mesh type.
    public enum SceneMeshType
    {
        Obj,
        DracoObj,
        TexturedObjZip
    }

    // Enum of scene status processed in the cloud.
    public enum SceneStatus
    {
        Empty,
        Capturing,
        Processing,
        Completed,
        Failed
    }

    // Represents a specific scene session.
    public struct SceneInfo
    {
        // TODO: we may want to obfusticate this ID in order not to use real ID directly.
        public string sceneId;

        public SceneStatus status;

        public SceneMeshType meshType;

        public long updateTimestamp;

        public ObjectDetectionResult detectionResult;
    }
}
