using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirrorVerse.UI.MirrorSceneClassyUI
{
    public enum ProcessingState
    {
        Processing = 0,
        Downloading = 1,
        Done = 2,
    }

    public class ProcessingMenu : SubMenu<ProcessingMenu>
    {
        public Transform progressParent;
        public float noInteruptDuration = 5.0f;  // Only after this amount of seconds that the this progress UI can be interupted.
        public float noProgressWarningDuration = 15.0f;  // If no progress on current stage for this amount of time, show warning UI text.
        public GameObject warningTip;

        private ProcessingState _processingState;
        private float _processingStartTime = float.MaxValue;
        private float _processingStepStartTime = float.MaxValue;


        public override void ShowMenu()
        {
            base.ShowMenu();
            warningTip.SetActive(false);
        }

        private void Update()
        {
            if (!warningTip.activeSelf && Time.time - _processingStepStartTime > noProgressWarningDuration)
            {
                warningTip.SetActive(true);
            }
        }

        public void UpdateProcessingText(ProcessingState state, float progress = 0)
        {
            if (progress == 0)
            {
                warningTip.SetActive(false);
                _processingStartTime = Time.time;
            }
            
            if (_processingState != state)
            {
                _processingStepStartTime = Time.time;
                _processingState = state;
            }
            
            foreach (Transform t in progressParent)
            {
                bool selected = t.GetSiblingIndex() == (int)state;
                t.gameObject.SetActive(selected);
                
                int percent = Mathf.CeilToInt(progress * 100);

                RectTransform progressImageBg = t.GetChild(0).GetComponent<RectTransform>();
                RectTransform progressImageValue = t.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                float progressImageValueWidth = Mathf.Clamp(progress * progressImageBg.sizeDelta.x, progressImageValue.sizeDelta.y, progressImageBg.sizeDelta.x);
                progressImageValue.sizeDelta = new Vector2(progressImageValueWidth, progressImageValue.sizeDelta.y);

                Text processingText = t.GetChild(2).GetComponent<Text>();
                processingText.text = $"{percent}%";
            }
        }

        public void UpdateProcessingTextDownloaded(UnityAction onDownloaded)
        {
            UpdateProcessingText(ProcessingState.Downloading, 1);
            StartCoroutine(DelayCall(onDownloaded, 1));
        }

        public void UpdateProcessingTextDone(UnityAction onDone)
        {
            UpdateProcessingText(ProcessingState.Done, 1);
            StartCoroutine(DelayCall(onDone, 1));
        }

        private IEnumerator DelayCall(UnityAction callback, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        public void ClickButton(string buttonName)
        {
            switch (buttonName)
            {
                case "Back":
                    // Only allow interuption after the set duration.
                    if (Time.time - _processingStartTime > noInteruptDuration)
                    {
                        if (_processingState == ProcessingState.Processing)
                        {
                            Debug.Log($"Progress has been interupted during processing.");
                            ClassyUI.Instance.SwitchMenu(SystemMenuType.ConfirmScanCancelMenu);
                        }
                        else if(_processingState == ProcessingState.Downloading)
                        {
                            // For downloading, just restart without confirm.
                            Debug.Log($"Progress has been interupted during downloading.");
                            ClassyUI.Instance.Restart();
                        }
                        else if(_processingState == ProcessingState.Done)
                        {
                            Debug.Log($"Progress has done.");
                        }
                        ClassyUI.Instance.TriggerBackClicked();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
