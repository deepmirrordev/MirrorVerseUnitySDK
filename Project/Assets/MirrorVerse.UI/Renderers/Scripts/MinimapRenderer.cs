using MirrorVerse.Options;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorVerse.UI.Renderers
{
    public class MinimapRenderer : MonoBehaviour
    {
        public MinimapRendererOptions options;

        private Camera _minimapCamera;
        private Camera _arCamera;

        private const float Damping = 0.3f;
        private const float CameraHeight = 5.0f;

        private string _currentClientId;
        private GameObject _currentDeviceModel;

        private class ClientDeviceModel
        {
            public GameObject modelObject;
            public Pose targetPose;
        }
        private Dictionary<string, ClientDeviceModel> _otherDeviceModels = new();

        public void ToggleVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            // Set culling mask for minimap top down camera.
            _minimapCamera = GetComponentInChildren<Camera>();
            if (_minimapCamera != null)
            {
                _minimapCamera.cullingMask = 1 << options.minimapLayer;
            }
        }

        private GameObject InstantiateDeviceModule(Vector3 position, Quaternion rotation)
        {
            GameObject deviceModelObject = null;
            if (options.deviceModelPrefab != null)
            {
                deviceModelObject = Instantiate(options.deviceModelPrefab, position, rotation);

                var allParts = deviceModelObject.GetComponentsInChildren<Transform>();
                foreach (var part in allParts)
                {
                    part.gameObject.layer = options.minimapLayer;
                }
            }
            return deviceModelObject;
        }

        public void SetArCameraObject(GameObject arCameraObject)
        {
            // Update minimap's camera and components with given layer mask.
            // Remove minimap layer from arCamera culling mask.
            _arCamera = arCameraObject.GetComponent<Camera>();
            _arCamera.cullingMask &= ~(1 << options.minimapLayer);
        }

        public void UpdateCurrentClient(string currentClientId)
        {
            _currentClientId = currentClientId;
            _currentDeviceModel = InstantiateDeviceModule(_arCamera.transform.position, _arCamera.transform.rotation);
        }


        public void UpdateClientPose(string clientId, Pose clientPose)
        {
            if (clientId == _currentClientId)
            {
                // Skip current client as current client is driven by ar camera locally.
                return;
            }

            if (_otherDeviceModels.TryGetValue(clientId, out var client))
            {
                client.targetPose = clientPose;
            }
            else
            {
                _otherDeviceModels[clientId] = new ClientDeviceModel
                {
                    modelObject = InstantiateDeviceModule(clientPose.position, clientPose.rotation),
                    targetPose = clientPose
                };
            }
        }

        // Update the minimap camera in LateUpdate() to ensure the camera is updated after the AR camera.
        private void LateUpdate()
        {
            if (_arCamera != null)
            {
                Vector3 newPosition = _arCamera.transform.position;
                newPosition.y += CameraHeight;
                _minimapCamera.transform.SetPositionAndRotation(
                    newPosition + Quaternion.Euler(0f, _arCamera.transform.eulerAngles.y, 0f) * Vector3.back * 1f,
                    Quaternion.Euler(60f, _arCamera.transform.eulerAngles.y, 0f));
                if (_currentDeviceModel != null)
                {
                    _currentDeviceModel.transform.SetPositionAndRotation(_arCamera.transform.position, _arCamera.transform.rotation);
                }
            }

            foreach (var otherDevice in _otherDeviceModels.Values)
            {
                otherDevice.modelObject.transform.SetPositionAndRotation(
                    Vector3.Lerp(otherDevice.modelObject.transform.position, otherDevice.targetPose.position, Damping),
                    Quaternion.Slerp(otherDevice.modelObject.transform.rotation, otherDevice.targetPose.rotation, Damping));
            }
        }

        public void ClearAll()
        {
            _currentClientId = null;
            if (_currentDeviceModel != null)
            {
                Destroy(_currentDeviceModel);
            }
            if (_otherDeviceModels != null)
            {
                foreach (var deviceModel in _otherDeviceModels.Values)
                {
                    Destroy(deviceModel.modelObject);
                }
                _otherDeviceModels.Clear();
            }
        }
    }
}
