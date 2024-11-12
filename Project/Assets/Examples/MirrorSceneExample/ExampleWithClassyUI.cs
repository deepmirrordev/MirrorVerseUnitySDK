using MirrorVerse;
using MirrorVerse.UI.MirrorSceneClassyUI;
using UnityEngine;

public class ExampleWithClassyUI : MonoBehaviour
{
    private ExampleGame _game;

    private void Awake()
    {
        _game = GetComponent<ExampleGame>();
    }

    private void Start()
    {
        if (MirrorScene.IsAvailable())
        {
            Debug.Log($"MirrorScene API is available.");
            // Application can listen to some events.
            MirrorScene.Get().onSceneReady += OnMirrorSceneReady;
            ClassyUI.Instance.onMenuFinish += OnMenuFinish;
            ClassyUI.Instance.onMenuCancel += OnMenuCancel;
        }
        else
        {
            Debug.Log($"MirrorScene API is not available.");
        }

        _game.ShowMirrorSceneButtons();
    }

    private void OnDestroy()
    {
        ResetArGame();
    }

    public void CreateArGame()
    {
        Debug.Log($"Start MirrorScene menu to create scene.");

        _game.ClearAll();
        _game.HideButtons();

        ClassyUI.Instance.RestartToCreate();
    }

    public void JoinArGame()
    {
        Debug.Log($"Start MirrorScene menu to join scene.");

        _game.ClearAll();
        _game.HideButtons();

        ClassyUI.Instance.RestartToJoin();
    }

    public void ResetArGame()
    {
        Debug.Log($"Exit scene and reset all.");
        _game.ClearAll();

        ClassyUI.Instance.ExitScene();
    }

    public void ShowQrCode()
    {
        ClassyUI.Instance.TriggerShowQrCode();
    }

    public void OnMirrorSceneReady(StatusOr<SceneInfo> sceneInfo)
    {
        // Called by MirrorScene system when a scene has processed.
        if (sceneInfo.HasValue)
        {
            Debug.Log($"Example scanned scene is ready to use. {sceneInfo.Value.status}");
        }
        else
        {
            Debug.Log($"Example scanned scene failed to process.");
        }
    }

    public void OnMenuFinish()
    {
        // Called by mirrorscene flow finished.
        Debug.Log($"Example MirrorScene menu finished.");
        // Update UI.
        _game.ShowGameButtons();
    }

    public void OnMenuCancel()
    {
        // Called by mirrorscene flow cancelled.
        Debug.Log($"Example MirrorScene menu cancelled.");
        _game.ShowMirrorSceneButtons();
    }
}
