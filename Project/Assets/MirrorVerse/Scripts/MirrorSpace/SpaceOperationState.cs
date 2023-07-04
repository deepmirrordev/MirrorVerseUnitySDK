namespace MirrorVerse
{
    // Operation states for a given MirrorSpace session.
    public enum SpaceOperationState
    {
        Uninitialized,                        // System is not initialized.
        Idle,                                 // System is not running any operation.
        Localizing                            // System is localizing.
    }
}
