using MirrorVerse;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ExampleGame : MonoBehaviour
{
    public Canvas gameUI;
    public GameObject spawnBotButton;
    public GameObject dropObjectsButton;
    public GameObject botPrefab;
    public Material objMaterial;

    private List<GameObject> _bots = new();
    private List<GameObject> _objs = new();

    private void OnDestroy()
    {
        ClearAll();
    }

    public void ClearAll()
    {

        foreach (GameObject bot in _bots)
        {
            Destroy(bot);
        }
        _bots.Clear(); 
        
        foreach (GameObject obj in _objs)
        {
            Destroy(obj);
        }
        _objs.Clear();
    }

    public void HideButtons()
    {
        gameUI.gameObject.SetActive(false);
        spawnBotButton.SetActive(false);
        dropObjectsButton.SetActive(false);
    }

    public void ShowGameButtons()
    {
        gameUI.gameObject.SetActive(true);
        spawnBotButton.SetActive(true);
        dropObjectsButton.SetActive(true);
    }

    public void ShowMirrorSceneButtons()
    {
        gameUI.gameObject.SetActive(true);
        spawnBotButton.SetActive(false);
        dropObjectsButton.SetActive(false);
    }

    public void OnDropButtonClicked()
    {
        if (!MirrorScene.Get().GetCurrentCursorPose().HasValue)
        {
            Debug.Log($"No cursor.");
            return;
        }
        Vector3 pos = MirrorScene.Get().GetCurrentCursorPose().Value.position;
        for (int i = 0; i < 30; i++)
        {
            PrimitiveType primitiveType = (PrimitiveType)(i % 4);
            GameObject obj = GameObject.CreatePrimitive(primitiveType);
            _objs.Add(obj);
            obj.transform.SetPositionAndRotation(
                new Vector3(Random.Range(pos.x - 0.2f, pos.x + 0.2f), Random.Range(2.0f, 4.0f), Random.Range(pos.z - 0.2f, pos.z + 0.2f)),
                Random.rotation);
            float size = Random.Range(0.01f, 0.1f);
            obj.transform.localScale = new Vector3(size, size, size);
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = objMaterial;
            Rigidbody rigid = obj.AddComponent<Rigidbody>();
            rigid.useGravity = true;
            rigid.isKinematic = false;
        }
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
