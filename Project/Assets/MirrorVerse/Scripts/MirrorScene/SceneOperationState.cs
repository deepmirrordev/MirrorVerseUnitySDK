namespace MirrorVerse
{
    // Operation states for a given MirrorScene session.
    public enum SceneOperationState
    {
        // Inactive state
        Uninitialized,                        // The system is not yet initialized.
        Idle,                                 // The system is inactive without an active scene.

        // Scene preparing states
        Standby,                              // System has an active scene but not running any operation.
        Detecting,                            // System is detecting markers nearby to activate a scene.
        Streaming,                            // System is streaming image and renderable for the current scene.
        Processing,                           // System is processing the captured active scene in the cloud.
        Downloading,                          // System is downloading an active scene which is processed.

        // Post scene preparation states
        Ready,                                // System has prepared the active scene and is ready for localiztion.
        Localizing                            // System is localizing with active scene info.
    }
}
