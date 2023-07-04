# Get Started

## Request for API Key
- We are still in early stage. Please visit our home page to learn more about MirrorVerse Platform, and reach out to us to request as early testers.
- Once you get the API key and secret, follow the [MirrorScene Setup](#mirrorscene-setup) for how to setup the API key.


## Dependencies

- Unity Version 2021.3 or above.
- URP (Universal Render Pipeline) 12.1.6 or above. `MirrorVerse SDK` uses URP for rendering. Please refer to [Installing AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/index.html#installing-ar-foundation) for more details.
- AR Foundation 4.2.6 or above with ARCore and ARKit plugins.


## Installation

- If creating a new project, start a 3D (URP) project from Unity Hub, or make sure the existing project has URP 12.1.6 or above installed.
- If the project does not have AR Foundation installed, add AR Foundation 4.2.6 or above from Unity Package Manager, and it's coresponding version of ARCore XR Plugin and/or ARKit XR Plugin.
- Install MirrorVerse Unity SDK

    - Import downloaded .unitypackage file of the [SDK releases list](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page.
        - Simply import all files from the package, and put them to your project properly.
        - The .unitypackage file includes assets and content from both `MirrorVerse SDK` and `MirrorVerse SDK UI` packages.
        - These two packages won't show up in Unity Package Manager.

    - Or add package URLs with Unity Package Manager
        - MirrorVerse SDK：`https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse`
            - Core component to access the SDK.
            - You can also add a dependency in `Packages/manifest.json`:
                ```yaml
                "com.deepmirror.mirrorverse": "https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse"
                ```

        - MirrorVerse SDK UI：`https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse.UI`
            - A set of rendering and UI components to drive the flow and visualize the results.
            - You can also add a dependency in `Packages/manifest.json`:
                ```yaml
                "com.deepmirror.mirrorverse.ui": "https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse.UI"
                ```

        - After all installed:

            <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/ec9b1345-52c2-4dda-8580-3f3a70686f2f" width="65%">



## Configurations

- Make sure ARCore and ARKit plugin are enabled.
    - Check `XR Plugin-in Management` -> `[Android -> ARCore | iOS -> ARKit ]`
- Make sure Android player has correctly configured.
    - Under  `Build Settings` -> `Player Settings` -> `Player` -> `Android` -> `Other Settings`,
        - Select `Rendering` -> `Graphics APIs`, uncheck `Auto Graphics API`, remove `Vulkan` if exists, and keep only `OpenGLES3`.
        - Select `Identification` -> `Minimum API Level` to `API level 28`.
        - Select `Configuration` -> `Scripting Backend` to `IL2CPP` instead of `Mono`.
        - Check `Configuration` -> `Target Architectures` to `ARM64` instead of others.
- Make sure `AR Background Renderer Feature` has been enabled for your active URP renderer setting.
    - Click `Add Renderer Feature` on each of `UniversalRenderPipelineAssetRenderer.asset` file's inspector panel, and select the AR backournd one from the list. By default, the URP renderer settings assets are under `Assets/Settings` folder in Unity project.

  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/00c1c0a3-73e9-4c03-84e7-ed80ad57b8b0" width="70%">


## MirrorScene Setup

In a Unity scene, arrange these necessary game objects to configure MirrorScene:
- Drag `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll.prefab` from package folder to the Unity scene as a game object.
- This prefab has everything you need to setup a simple AR game. It has 
    - Core components: `MirrorVerseImpl`, `ArFoundationAdapter`，`ArFoundationCamera`
    - Customizable visuzalization component: `MirrorSceneRenderer`
    - Customizable UI component: `MirrorSceneDefaultUI`
- In your project, right click to open `Create` -> `MirrorVerse` -> `App Auth Options` to create an empty app auth assets, and fill your API key and secret there. Drag the `appAuthOptions.asset` file to the MirrorScene game object's `App Auth Options` property.
- Some existing Unity game objects need to be configured
    - If not yet, add `EventSystem` to your Unity scene:

      <img src="https://github.com/deepmirrordev/MirrorSceneExamples/assets/53016403/67e2af8e-49c3-4c8f-8058-2b1084d22eb5" width="70%">

    - The core component prefab contains an AR camera. Remove or deactive original normal camera if there are any.
    - Adjust the lighting in the Unity scene to get the best AR effect on real device.

- After the setup, the Unity project looks like this:

  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/8024c541-f9f1-4f43-8757-0a3fbbf03922" width="70%">


## Scripting

After the setup in Unity scene, now we can use scripts to trigger SDK to create and use MirrorScene system. Please read [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) interface and related events and data structures.

If using the ``MirrorSceneAll.prefab` components provided from `MirrorVerse SDK UI` package, there is simpler way to just use the UI component, because all glue codes are implemented already. Try the following codes and add this MonoBehaviour to an empty game object in the Unity scene:

```C#
using UnityEngine;
using MirrorVerse;
using MirrorVerse.UI.MirrorSceneDefaultUI;

public class MyExampleGame : MonoBehaviour
{
    private void Start()
    {
        if (MirrorScene.IsAvailable())
        {
            DefaultUI.Instance.onMenuFinish += OnMenuFinished;
            // Called at start to trigger the MirrorScene UI Menu.
            DefaultUI.Instance.Restart();
        }
    }

    private void OnMenuFinished()
    {
        // Called scene is ready and localized.

        // Now start the game logic.
        // Access scene infomation via IMirrorScene interface.
    }
}
```
Then build android or iOS package and install on your phone. A simple AR scene powered by `MirrorScene` will be shown.

For more examples, please checkout [MirrorSceneExamples](https://github.com/deepmirrorinc/MirrorSceneExamples) repo.


## Advanced Customization

### Customize Visualization

There are several options or configurations that are configurable, like `StaticMeshRendererOptions`, `PointCloudRendererOptions` etc. If you want to override the options, you can:
- Use right click context menu `Create` -> `MirrorVerse` to create an empty options instance asset, and fill in values yourself.
- Then drag the new asset to the cooresponding property of the `MirrorSceneRenderer` game object.

  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/adb3a637-d7e4-40d8-ac95-09210a699329" width="70%">

- If you want further customization the rendering, you can also override the renderer classes in `/Renderers` folder in `MirrorVerse SDK UI` package. Just implement your own visualization and hook them up to `MirrorSceneRenderer` game object.
- Or implement entire visualization logic by inheriting `SceneRenderer` interface and replace default rendering components in the prefab.

### Customize UI Styles

In `MirrorVerse SDK UI` package, a set of default UI component is provided : `MirrorSceneDefaultUI`, which is used in many sample apps come with the SDK. Developer can modify or create a set of new UI component with custom styles.

### Customize UI Flows

Developer can also write their own flows of interactions that only uses `MirrorScene` insterface. After `MirrorScene` initialized, the system will transition its states in a graph during user's operations. 

<img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/e3dc82b2-075b-45a4-b047-7920dad51680" width="100%">

Developers can use `IMirrorScene` to access information from the system, or operate the system, e.g. start streaming or exit localiztion.  Developers can add UI between events, calls and waitings during the MirrorScene operation flows.  In the diagrams above. The left diagram illustrates the state transitions for a host device, and the right diagram illustrates the state transition graph for a guest.

Below is a simplest code reference that only uses `IMirrorVerse` interface to hook up the whole flow, including multiple users scenario.

```C#
using UnityEngine;
using MirrorVerse;

public class MyExampleGame : MonoBehaviour
{
    private void Start()
    {
        if (MirrorScene.IsAvailable())
        {
            // Register an event handler to handle scene standby event.
            MirrorScene.Get().onSceneStandby += OnSceneStandby;

            // Register an event handler to handle scene ready event.
            MirrorScene.Get().onSceneReady += OnSceneReady;

            // There are other useful events to handle.
        }
    }
    
    public void OnStartButtonClicked()
    {
        // Called by button clicks, creates a scene.
        // Once the scene is created, OnSceneStandby event will be triggered.
        MirrorScene.Get().CreateScene();
    }
    
    public void OnJoinButtonClicked()
    {
        // Called by button clicks, joins a scene by marker detection.
        MirrorScene.Get().StartMarkerDetection((marker, markerPose, localizedPose) =>
        {
            // Once the scene is joined and loaded, OnSceneStandby event will be triggered.
            MirrorScene.Get().JoinScene(marker.sceneId);
        });
    }

    private void OnSceneStandby(StatusOr<SceneInfo> sceneInfo)
    {
        switch (sceneInfo.Value.status)
        {
            case SceneStatus.Empty:
            case SceneStatus.Capturing:
                // Created a scene or joined a scene just created, start streaming.
                MirrorScene.Get().StartSceneStream();
                break;
            case SceneStatus.Completed:
                // Joined a completed scene, start downloading the mesh.
                MirrorScene.Get().DownloadSceneMesh();
                break;
        }
    }

    public void OnFinishButtonClicked()
    {
        // Called by button clickes, finishes the stream.
        // The scene starts to process in the cloud.
        MirrorScene.Get().FinishSceneStream();
    }

    private void OnSceneReady(StatusOr<SceneInfo> sceneInfo)
    {
        // Called when a scene has processed and ready to use.
        if (sceneInfo.HasValue)
        {
            // Starts localization so that the device tracking never lost.            
            MirrorScene.Get().StartLocalization();
        }
    }

    public void OnExitButtonClicked()
    {
        // Called by button clicks, exits the scene.
        MirrorScene.Get().ExitScene();
    }
}
```