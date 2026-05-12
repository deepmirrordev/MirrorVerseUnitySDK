using MirrorVerse;
using UnityEngine;
using UnityEngine.UI;

public class ExampleApp : MonoBehaviour
{
    public Text statusText;
    public Text ecefPoseText;
    public Text localPoseText;
    public Text coordsBaseText;

    private void Start()
    {
        
        if (MirrorSpace.IsAvailable())
        {
            Debug.Log($"MirrorSpace API is available.");
            // Application can listen to some events.
            MirrorSpace.Get().onLocalizationUpdate += OnLocalizationUpdate;
        }
        else
        {
            Debug.Log($"MirrorSpace API is not available.");
        }
    }

    public void OnLocalizationUpdate(StatusOr<Pose> localizedPose, StatusOr<EcefPose> ecefPose)
    {
        if (localizedPose.HasValue)
        {
            // Localized pose always reports value.
            Vector3 lp = localizedPose.Value.position;
            Vector3 lq = localizedPose.Value.rotation.eulerAngles;

            string label = "Local: ";
            if (ecefPose.HasValue)
            {
                // If ecef pose has value, means it's localized. This value become an coordsBase of current device ECEF pose to coordinates base ECEF pose.
                label = "Offset: ";
            }
            localPoseText.text = string.Format("{0}: ({1:F2}, {2:F2}, {3:F2}) - ({4:F2},  {5:F2},  {6:F2})", label, lp.x, lp.y, lp.z, lq.x, lq.y, lq.z);
        }
        else
        {
            localPoseText.text = "";
        }

        if (ecefPose.HasValue)
        {
            // Localized.
            ecefPoseText.text = $"Earth Pose: {ecefPose.Value.position.ToGeodetic()}\nECEF: {ecefPose.Value}";
        }
        else
        {
            // Not localized.
            ecefPoseText.text = "";
        }

        var result = MirrorSpace.Get().GetLocalizationResult();
        if (result.HasValue)
        {
            Ecef3d coordsBase = MirrorSpace.Get().GetCoordinatesOffsetBase();
            coordsBaseText.text = $"Coords Base: {coordsBase.ToGeodetic()}\nECEF: {coordsBase}";

            statusText.text = string.Format("Status: {0}, Latency: {1:F0}ms", result.Value.localizationStatus, result.Value.latency);
        }
        else
        {
            statusText.text = "No Localization";
            coordsBaseText.text = "";
        }
    }

    public void OnLocalizeButtonClicked()
    {
        if (MirrorSpace.Get().GetOperationState() == SpaceOperationState.Localizing)
        {
            Debug.Log($"MirrorSpace API is already localizing. Skip.");
            return;
        }

        MirrorSpace.Get().StartLocalization();
        statusText.text = "Localizing...";
    }

    public void OnResetButtonClicked()
    {
        MirrorSpace.Get().StopLocalization();
        Debug.Log($"MirrorSpace API localization stopped.");
        statusText.text = "No Localization";
        ecefPoseText.text = "";
        localPoseText.text = "";
        coordsBaseText.text = "";
    }
}
