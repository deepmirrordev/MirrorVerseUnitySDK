namespace MirrorVerse
{
    // Enum of scene mesh type.
    public enum SceneMeshType
    {
        // Uncompressed obj format.
        Obj,
        // Draco compressed obj format
        DracoObj,
        // Uncompressed obj format with textured and bundled as a zip file.
        TexturedObjZip,
        // Use Nerf for reconstruction.
        Nerf
    }

    // Enum of scene status processed in the cloud.
    public enum SceneStatus
    {
        // Scene is just created and not yet been used.
        Empty,
        // Scene is currently scanning and capturing data. 
        Capturing,
        // Scene scanning is finished and is processing in the cloud.
        Processing,
        // Scene processing has been completed and ready to use.
        Completed,
        // Scene processing has failed.
        Failed
    }

    // Represents a specific scene session.
    public struct SceneInfo
    {
        // ID of the scene. It represents a scanning of an area. Host and guests can share the same scene with the scene ID.
        public string sceneId;

        // Current status of the scene. Only completed scene can be used for localization.
        public SceneStatus status;

        // Type of the mesh that the scene is attached. The mesh matches the physical surroundings after localization.
        public SceneMeshType meshType;

        // Timestamp (milliseconds since epoch) when this scene was last updated.
        public long updateTimestamp;

        // Timestamp (milliseconds since epoch) that this scene will be expired.
        public long expireTimestamp;

        // (Experimental) Detected objects inside the physical surrounding that this scene covered.
        public ObjectDetectionResult detectionResult;
    }
}
