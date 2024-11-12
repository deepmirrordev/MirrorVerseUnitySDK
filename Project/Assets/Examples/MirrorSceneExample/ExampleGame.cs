using MirrorVerse;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static MirrorVerse.Options.NavigationOptions;

public class ExampleGame : MonoBehaviour
{
    public Canvas gameUI;
    public GameObject gameButtonPanel;
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
        gameButtonPanel.SetActive(false);
    }

    public void ShowGameButtons()
    {
        gameUI.gameObject.SetActive(true);
        gameButtonPanel.SetActive(true);
    }

    public void ShowMirrorSceneButtons()
    {
        gameUI.gameObject.SetActive(true);
        gameButtonPanel.SetActive(false);
    }

    public void OnDropButtonClicked()
    {
        if (!MirrorScene.IsAvailable() || !MirrorScene.Get().GetCurrentCursorPose().HasValue)
        {
            Debug.Log($"No cursor.");
            return;
        }
        Vector3 pos = MirrorScene.Get().GetCurrentCursorPose().Value.position;
        
        // Just to preserve collider classes.
        Collider _; _ = new SphereCollider(); _ = new BoxCollider();

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
        if (!MirrorScene.IsAvailable() || MirrorScene.Get().GetNavigationOptions().navigationMode != NavigationMode.Unity)
        {
            Debug.Log($"Navigation not active.");
            return;
        }
        var pose = MirrorScene.Get().GetCurrentCursorPose();
        if (pose.HasValue)
        {
            var bot = Instantiate(botPrefab, pose.Value.position, Quaternion.identity);
            _bots.Add(bot);
        }
    }

    public void OnNavButtonToggled(Toggle toggle)
    {
        if (!MirrorScene.IsAvailable())
        {
            return;
        }
        // Toggle navigation.
        var navOptions = MirrorScene.Get().GetNavigationOptions();
        if (toggle.isOn)
        {
            navOptions.navigationMode = NavigationMode.Unity;
        }
        else
        {
            navOptions.navigationMode = NavigationMode.None;
        }
    }

    private void Update()
    {
        if (!MirrorScene.IsAvailable())
        {
            return;
        }
        var pose = MirrorScene.Get().GetCurrentCursorPose();
        if (pose.HasValue && MirrorScene.Get().GetNavigationOptions().navigationMode == NavigationMode.Unity)
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
