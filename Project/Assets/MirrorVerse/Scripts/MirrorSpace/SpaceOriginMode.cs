namespace MirrorVerse
{
    // Enum of the mode of origin of a MirrorSpace session.
    public enum SpaceOriginMode
    {
        // Space's coordinates origin is the same as device's local XR session origin.
        // For this mode, the origin of the Space is the pose that mobile phone's ARCore/ARKit session origin in the physical space.
        // Before localization all poses in the Space are under this origin mode.
        LocalOrigin,

        // Space's coordinates origin is the same as ECEF origin with a given offset.
        // After localization all poses in the space are under this origin mode.
        EcefOffsetOrigin
    }
}
