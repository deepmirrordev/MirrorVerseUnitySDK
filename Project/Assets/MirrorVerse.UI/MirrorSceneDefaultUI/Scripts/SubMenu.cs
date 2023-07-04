using UnityEngine;

namespace MirrorVerse.UI.MirrorSceneDefaultUI
{
    // Singleton base menu UI component.
    public class SubMenu<T> : MonoBehaviour where T : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        public virtual void ShowMenu()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void HideMenu()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public static T Instance { get; private set; } = null;

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Debug.LogWarning("Cannot create multiple instances for a singleton of a given sub type.");
                Destroy(gameObject);
            }
        }
    }
}
