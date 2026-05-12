using MirrorVerse;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingExample : MonoBehaviour
{
    public Text statusText;
    public Text poseText;
    public Text gnssText;
    public Toggle planeDetectionToggle;
    public GameObject detectedPlanePrefab;
    public GameObject raycastCursorPrefab;

    [SerializeField]
    public List<TrackedImageData> trackedImageData = new();

    public GameObject trackedImageInfoPrefab;
    
    private GameObject _trackedImageInfo;
    private TrackedImageResult? _trackedImageResult = null;

    private GameObject _raycastCursor;

    private void Start()
    {
        if (MirrorSpace.IsAvailable())
        {
            Debug.Log($"MirrorSpace API is available.");
            // Application can listen to some events.
            MirrorSpace.Get().onApiReady += OnApiReady;
            MirrorSpace.Get().onDeviceTrackingStatusChange += OnDeviceTrackingStatusChange;
            MirrorSpace.Get().onLocalizationUpdate += OnLocalizationUpdate;
            MirrorSpace.Get().onTrackedImageDetected += OnTrackedImageDetected;

            // Apply initial value from toggle.
            OnPlaneDetectionToggled();
        }
        else
        {
            Debug.Log($"MirrorSpace API is not available.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlanesAndRayacstHit();
    }

    private void UpdatePlanesAndRayacstHit()
    {
        // This is called every frame to draw detected planes and raycast cursor on the hit pose.
        
        // App can take the hitResult pose to interact with the plane that device is pointing to.
        Pose? cursorPose = GetCursorPoseOnPlane();

        // Draw cursor
        if (cursorPose.HasValue)
        {
            Pose pose = cursorPose.Value;
            // For plane cursor, rotate 90 on x-axis of the quad, same as set in cursor prefab.
            pose.rotation = Quaternion.AngleAxis(90, Vector3.right);
            if (_raycastCursor == null)
            {
                _raycastCursor = Instantiate(raycastCursorPrefab);
            }
            _raycastCursor.SetActive(true);
            _raycastCursor.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        else
        {
            if (_raycastCursor != null)
            {
                _raycastCursor.SetActive(false);
            }
        }

        // Get data from planes' boundary
        ARPlaneManager arPlaneManager = MirrorSpace.Get().GetXrPlatformAdapter().GetComponent<ARPlaneManager>();
        if (arPlaneManager != null && arPlaneManager.enabled)
        {
            int numOfPlanes = arPlaneManager.trackables.count;
            foreach (ARPlane arPlane in arPlaneManager.trackables)
            {
                // get arPlane.boundary data points
            }
            planeDetectionToggle.GetComponentInChildren<Text>().text = $"Planes: {numOfPlanes}";
        }
    }

    public Pose? GetCursorPoseOnPlane()
    {
        // Returns the pose on the detect plane that device is pointing to interact.

        // NOTE: this currently only worked without localization or image tracking.
        // NOTE: Replace Matrix4x4.identity with a correct transformation matrix if localization is taken into consideration.
        RaycastHitResult? hitResult = MirrorSpace.Get().GetXrPlatformAdapter().TriggerRaycastOnPlane(Matrix4x4.identity);
        if (hitResult.HasValue && hitResult.Value.mode == RaycastHitMode.Plane)
        {
            return hitResult.Value.raycastHitPose;
        }
        return null;
    }

    public void OnApiReady()
    {
        // Get called when this API is ready to use.
        statusText.text = "API Ready";
    }

    public void OnDeviceTrackingStatusChange(StatusOr<TrackingStatus> newTrackingStatus)
    {
        // Get called when tracking status changed. Could be Tracking or NotTracking.
        // How to test: cover the camera and let it failed to track and see if this gets called with new status "NotTracking".
        poseText.text = newTrackingStatus.Value.ToString();
    }

    public void OnTrackedImageDetected(StatusOr<TrackedImageResult> trackedImageResult)
    {
        // Get called when image is detected.

        statusText.text = "Detected";

        Debug.Log($"OnTrackedImageDetected {trackedImageResult.HasValue}");
        if (_trackedImageInfo != null)
        {
            Destroy(_trackedImageInfo);
        }

        _trackedImageInfo = Instantiate(trackedImageInfoPrefab);
        _trackedImageResult = trackedImageResult.Value;
    }

    public void OnLocalizationUpdate(StatusOr<Pose> localizedPose, StatusOr<EcefPose> ecefPose)
    {
        // Get called every frame, telling the offset pose of the device camera in physical world, but the origin of this world.
        //
        // Before image is detected:
        // - localizedPose: Device camera pose in local AR origin (first pose as origin when AR camera is started, and Y-axis is always UP).
        //                  How to test: before image is dectected, move the phone back to the position when it's started, this value is back to zero.
        // - ecefPose:      No value.

        // After image is detected:
        // - localizedPose: Device camera pose using detected image pose as origin.
        //                  How to test: after image is dectected, move the phone to very close to the image marker plain, this value is back to zero.
        // - ecefPose:      Localized pose + ECEF offset come with image marker. This pose's position in absolute earth coordinate (ECEF coords).
        //                  If no embedded lat/lng or ECEF information in image marker, ecefPose is the same as localizedPose.
        //
        //                  For example, the tracked image "Rafflesia" (the red flower picture) has an embeded earth location (Guangzhou)
        //                  at lat=22.78 lng=113.52, and alt=20(meter), which means this marker image is exactly placed at
        //                  this location on earth.  Once a device has detected this image in physical world, we will know
        //                  the device's absolute earth pose (the value of ecefPose of this event) by combining the marker image ecef
        //                  pose (from input tracked image info) and local offset pose (the value of localizedPose of this event).

        Vector3 p = localizedPose.Value.position;
        Vector3 q = localizedPose.Value.rotation.eulerAngles;

        string txt = string.Format("Localized pose: ({0:F1},  {1:F1},  {2:F2}) - ({3:F2},  {4:F2},  {5:F2})", p.x, p.y, p.z, q.x, q.y, q.z);

        if (ecefPose.HasValue)
        {
            Ecef3d ep = ecefPose.Value.position;
            Vector3 eq = ecefPose.Value.rotation.eulerAngles;
            txt += string.Format("\nECEF: ({0:F1},  {1:F1},  {2:F2}) - ({3:F2},  {4:F2},  {5:F2})", ep.x, ep.y, ep.z, eq.x, eq.y, eq.z);
        }
        poseText.text = txt;


        if (!_trackedImageResult.HasValue)
        {
            return;
        }
        if (_trackedImageInfo == null)
        {
            return;
        }

        TrackedImageData data = null;
        foreach (var trackedImage in trackedImageData)
        {
            if (trackedImage.refernceName == _trackedImageResult.Value.referenceImageName)
            {
                data = trackedImage;
            }
        }

        if (data == null)
        {
            statusText.text = "Localization Failed: Detected image has no reference.";
            return;
        }

        var canvas = _trackedImageInfo.GetComponentInChildren<Canvas>();
        var text = _trackedImageInfo.GetComponentInChildren<Text>();

        text.supportRichText = true;
        text.text = $"{_trackedImageResult.Value.referenceImageName}:\n{_trackedImageResult.Value.offsetPose}\n{_trackedImageResult.Value.ecefPose}\n{data.physicalPose}";
        // Note: canvas has rotation x=90 so that local scale on x-y plane.
        canvas.transform.localScale = new Vector3(_trackedImageResult.Value.trackedSize.x, _trackedImageResult.Value.trackedSize.y, 1f);

        var panelParentGo = _trackedImageInfo.transform.GetChild(0).gameObject;
        // image plane is vertical so local scale on x-z plane.
        panelParentGo.transform.localScale = new Vector3(_trackedImageResult.Value.trackedSize.x, 1f, _trackedImageResult.Value.trackedSize.y);
        var panel = panelParentGo.transform.GetChild(0).gameObject;
        var material = panel.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = data.refernceTexture;

        statusText.text = $"Localized: {_trackedImageResult.Value.referenceImageName}";
    }

    public void OnLocalizeButtonClicked()
    {
        if (MirrorSpace.Get().GetOperationState() == SpaceOperationState.Localizing)
        {
            Debug.Log($"MirrorSpace API is already localizing. Skip.");
            return;
        }
        MirrorSpace.Get().StartTrackedImageLocalization();
        statusText.text = "Localizing...";
    }

    public void OnResetButtonClicked()
    {
        MirrorSpace.Get().StopTrackedImageLocalization();
        Debug.Log($"MirrorSpace API localization stopped.");
        statusText.text = "Not Localized";
        poseText.text = "";
    }

    public void OnLoadTrackedImagesClicked()
    {
        MirrorSpace.Get().AddTrackedImageData(trackedImageData.ToArray(), () =>
        {
            statusText.text = $"{trackedImageData.Count} tracked images loaded.";
        });
        Debug.Log($"MirrorSpace API tracked images loaded.");
    }

    public void OnPlaneDetectionToggled()
    {
        // Enable plane detection.
        MirrorSpace.Get().GetXrPlatformAdapter().SetRaycastOnPlaneEnabled(planeDetectionToggle.isOn);

        if (planeDetectionToggle.isOn && detectedPlanePrefab != null)
        {
            // Set detected plane to be visible, with given visualizer prefab.
            MirrorSpace.Get().GetXrPlatformAdapter().ToggleDetectedPlaneVisibility(true);
            MirrorSpace.Get().GetXrPlatformAdapter().SetDetectedPlanePrefab(detectedPlanePrefab);
        }
    }
}

