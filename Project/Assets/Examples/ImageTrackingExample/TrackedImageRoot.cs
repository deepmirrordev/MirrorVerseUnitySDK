using MirrorVerse;
using UnityEngine;

public class TrackedImageRoot: MonoBehaviour
{
    public Wgs3d worldRoot = Wgs3d.zero;
        
    void Start()
    {
        if (MirrorSpace.IsAvailable())
        {
            Debug.Log($"MirrorSpace API is available.");
            MirrorSpace.Get().onTrackedImageDetected += OnTrackedImageDetected;
        }
        else
        {
            Debug.Log($"MirrorSpace API is not available.");
        }
    }

    private void OnTrackedImageDetected(StatusOr<TrackedImageResult> trackedImageResult)
    {
        transform.eulerAngles = new Vector3(90, 0, 0);
        // Set the world based on detected image's embedded earth location.
        // That said, if this world root is not around the detected image, the content under this node will be very far away.
        // Consider hide it if futher than a threshold.
        MirrorSpace.Get().SetCoordinatesOffsetBase(trackedImageResult.Value.geodeticPose.Value.location);
    }
}
