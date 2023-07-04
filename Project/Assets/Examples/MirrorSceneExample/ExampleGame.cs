using MirrorVerse;
using MirrorVerse.UI.MirrorSceneDefaultUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ExampleGame : MonoBehaviour
{
    public Rigidbody gameCube;
    public Canvas gameUI;
    public GameObject spawnBotButton;
    public GameObject botPrefab;

    private List<GameObject> _bots = new();

    private void Start()
    {
        spawnBotButton.SetActive(false);
        if (MirrorScene.IsAvailable())
        {
            Debug.Log($"MirrorScene API is available.");
            // Application can listen to some events.
            MirrorScene.Get().onSceneReady += OnMirrorSceneReady;
            DefaultUI.Instance.onMenuFinish += OnMenuFinish;
            DefaultUI.Instance.onMenuCancel += OnMenuCancel;

        }
        else
        {
            Debug.Log($"MirrorScene API is not available.");
        }
    }

    private void OnDestroy()
    {
        ClearAllBots();
    }

    private void ClearAllBots()
    {
        foreach (GameObject bot in _bots)
        {
            Destroy(bot);
        }
        _bots.Clear();
    }

    public void StartMainMenu()
    {
        Debug.Log($"Example start MirrorScene menu.");

        // Reset the cube.
        gameCube.isKinematic = true;
        gameCube.transform.position = Vector3.zero;

        // Clear bots
        ClearAllBots();

        // Update UI.
        DefaultUI.Instance.ShowMenu();
        DefaultUI.Instance.Restart();
        gameUI.gameObject.SetActive(false);
        spawnBotButton.SetActive(false);
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
        // Called by custom finish prefab.
        Debug.Log($"Example MirrorScene menu finished.");
        // Update UI.
        gameUI.gameObject.SetActive(true);
        spawnBotButton.SetActive(true);

        // Drop the cube at current cursor.
        StatusOr<Pose> currentPose = MirrorScene.Get().GetCurrentCursorPose();
        if (currentPose.HasValue)
        {
            gameCube.transform.position = currentPose.Value.position + Vector3.up * 0.5f;
            gameCube.isKinematic = false;
        }
    }

    public void OnMenuCancel()
    {
        Debug.Log($"Example MirrorScene menu cancelled.");
        gameUI.gameObject.SetActive(true);
        spawnBotButton.SetActive(false);
    }

    public void OnSpawnButtonClicked()
    {
        var pose = MirrorScene.Get().GetCurrentCursorPose();
        if (pose.HasValue)
        {
            var bot = Instantiate(botPrefab, pose.Value.position, Quaternion.identity);
            _bots.Add(bot);
        }
    }

    private void Update()
    {
        if (MirrorScene.IsAvailable())
        {
            var pose = MirrorScene.Get().GetCurrentCursorPose();
            if (pose.HasValue)
            {
                foreach (var bot in _bots)
                {
                    var unityNavigationAgent = bot.GetComponent<NavMeshAgent>();
                    if (unityNavigationAgent != null)
                    {
                        if (unityNavigationAgent.isOnNavMesh)
                        {
                            unityNavigationAgent.SetDestination(pose.Value.position);
                        }
                    }
                }
            }
        }
    }
}
