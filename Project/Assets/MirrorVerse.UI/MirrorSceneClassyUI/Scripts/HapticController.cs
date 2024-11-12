using UnityEngine;
#if DM_HAPTICFEEDBACK
using CandyCoded.HapticFeedback;  // Only works when hapticfeedback package is installed.
#endif

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public class HapticController : MonoBehaviour
    {
        public float vibrateIntervalSeconds = 0.3f; 

        private float _lastFeedbackTime = -1;
        
        private bool _trigger;
        
        public static HapticController Instance { get; private set; } = null;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
#if !DM_HAPTICFEEDBACK
            Debug.Log($"Haptic Feedback package is not installed. Skip any vibration.");
#endif
        }

        // Vibrate the device with a light feedback
        public void TriggerVibrate()
        {
            _trigger = true;
        }

        private void Update()
        {
            if (_trigger)
            {
                _trigger = false;
                if (Time.time - _lastFeedbackTime > vibrateIntervalSeconds)
                {
                    // Should only call the feedback function in the main thread.
#if DM_HAPTICFEEDBACK
                    HapticFeedback.MediumFeedback();  // Only works when hapticfeedback package is installed.
#endif
                    _lastFeedbackTime = Time.time;
                }
            }
        }
    }
}
