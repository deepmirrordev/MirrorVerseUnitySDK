using System.Collections;
using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    public enum ToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    // Singleton manager to access toast UI component.
    public class ToastManager : MonoBehaviour
    {
        public bool isLoaded = false;
        public GameObject toastUIPrefab;

        private ToastCanvas _toastCanvas;

        public static ToastManager Instance { get; private set; } = null;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private IEnumerator ShowInternal(string text, float duration, Color color, ToastPosition position)
        {
            yield return null;
            if (!isLoaded)
            {
                GameObject instance = Instantiate(toastUIPrefab);
                instance.SetActive(true);
                _toastCanvas = instance.GetComponent<ToastCanvas>();
                isLoaded = true;
            }
            _toastCanvas.Init(text, duration, color, position);
        }

        public void Show(string text)
        {
            StartCoroutine(ShowInternal(text, 2F, Color.black, ToastPosition.TopCenter));
        }


        public void Show(string text, float duration)
        {
            StartCoroutine(ShowInternal(text, duration, Color.black, ToastPosition.TopCenter));
        }

        public void Show(string text, float duration, ToastPosition position)
        {
            StartCoroutine(ShowInternal(text, duration, Color.black, position));
        }

        public void Show(string text, Color color)
        {
            StartCoroutine(ShowInternal(text, 2F, color, ToastPosition.TopCenter));
        }

        public void Show(string text, Color color, ToastPosition position)
        {
            StartCoroutine(ShowInternal(text, 2F, color, position));
        }

        public void Show(string text, float duration, Color color, ToastPosition position)
        {
            StartCoroutine(ShowInternal(text, duration, color, position));
        }

        public void Dismiss()
        {
            _toastCanvas.Dismiss();
        }
    }
}
