namespace MirrorVerse
{
    // Enum of the mode of origin of a MirrorScene session.
    public enum SceneOriginMode
    {
        // Scene's coordinates origin is the same as device's local XR session origin.
        // For this mode, the origin of the scene is the pose that mobile phone's ARCore/ARKit session origin in the physical space.
        // Before localization all poses in the scene are under this origin mode.
        LocalOrigin,                        

        // Scene's coordinates origin is the same as the shared scene's own origin processed in the cloud.
        // For this mode, All devices that in this Scene will share the same origin in physical space.
        // After localization all poses in the scene are under this origin mode.
        SharedOrigin
    }
}
