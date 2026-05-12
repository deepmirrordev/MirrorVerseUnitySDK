using MirrorVerse;
using UnityEngine;

public class ExampleObject : MonoBehaviour
{
    public SpaceWorldRoot worldRoot;
    public TextMesh axisLabelText;

    // Start is called before the first frame update
    void Start()
    {
        if (!MirrorSpace.IsAvailable())
        {
            Debug.LogWarning("MirrorSpace API is not available.");
            return;
        }
        MirrorSpace.Get().onLocalizationUpdate += OnLocalizationUpdate;
    }

    private void OnLocalizationUpdate(StatusOr<Pose> localizedPose, StatusOr<EcefPose> ecefPose)
    {
        var result = MirrorSpace.Get().GetLocalizationResult();
        if (result.HasValue)
        {
            if (result.Value.localizationStatus == LocalizationStatus.Localized)
            {
                Pose objectLocalPose = GetRelativeOffsetToWorldRoot();
                EcefPose objectEarthPose = MirrorSpace.Get().GetEarthPoseFromCoordinatesOffsetBase(objectLocalPose);
                axisLabelText.text = $"{name}\n" +
                    $"Local: {objectLocalPose.position} - {objectLocalPose.rotation.eulerAngles}\n" +
                    $"ECEF: {objectEarthPose.position}\n" +
                    $"WGS84: {objectEarthPose.position.ToGeodetic()}";
                return;
            }
            else if (result.Value.localizationStatus == LocalizationStatus.WaitForResponse)
            {
                axisLabelText.text = $"{name}\nLocalizing...";
                return;
            }
        }
        axisLabelText.text = $"{name}\nNot Localized.";
    }

    private Pose GetRelativeOffsetToWorldRoot()
    {
        // Get relative offset from the current object (maybe moving) transform of this object to the world root node (stational).
        Matrix4x4 offsetMatrix = worldRoot.transform.worldToLocalMatrix * this.transform.localToWorldMatrix;
        Vector3 pos = offsetMatrix.GetColumn(3);
        Quaternion rot = offsetMatrix.rotation;
        return new Pose(pos, rot);
    }
}
