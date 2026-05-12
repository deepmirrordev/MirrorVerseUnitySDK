# Get Started

## Request an API Key

- We are still in early stages. Please visit our [home page](https://mirrorscene.deepmirror.com) to learn more about the MirrorVerse Platform, and reach out to request access and purchase the service.
- Once you have the API key and secret, follow the [Scene Setup](#scene-setup) section below to configure them in your project.


## Unity Version and Dependencies

- Unity **2022.3** LTS or above (the SDK is built on 2022.3.62f2; Unity 6000.x is also supported).
- URP (Universal Render Pipeline) **14.0.12** or above. The MirrorVerse SDK uses URP for rendering. Unity 6000.x ships URP 17, which is also supported.
- AR Foundation **5.1.5** or above (AR Foundation 6.x works on Unity 6000.x).

## Mobile Devices and Operating Systems

- **Android**
    - Android 10.0 or above on phones or tablets that support ARCore. Please refer to Google's [official ARCore supported devices](https://developers.google.com/ar/devices) list.
    - Some devices support ARCore but don't have a recent version of ARCore Services installed out of the box. Install the latest ARCore Services from the Play Store.
- **iOS**
    - iOS 12.0 or above on iPhones or iPads with the corresponding ARKit version.
- **HarmonyOS**
    - HarmonyOS 4.0 on Huawei or Honor phones / tablets with AREngine v4.0.0.5 or above.
    - HarmonyOS NEXT 5.0 devices are currently not supported.


## Installation

- **Universal RP**
    - For a new project, start from the **3D (URP)** template in Unity Hub. For an existing project, make sure URP 14.0.12 or above is installed (URP 17 on Unity 6000.x).

- **AR Foundation and XR Plug-ins**
    - If the project doesn't have AR Foundation installed, add **AR Foundation 5.1.5 or above** from Unity Package Manager, along with the matching version of **ARCore XR Plugin** and/or **ARKit XR Plugin**. See the [Installing AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html#installing-ar-foundation) instructions.

    - **AREngine XR Plugin** is not officially shipped by Unity. To run on HarmonyOS devices that support AREngine, our SDK provides an AREngine XR Plug-in ported from the open-source community. Download `com.unity.xr.arengine-[version].tgz` from the [SDK Releases](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page and install via `Package Manager` → `Add package from tarball...`. Once installed, it behaves like ARCore XR Plug-in — one APK can run on both ARCore devices and AREngine devices.

- **Install MirrorVerse Unity SDK**
    - From the [SDK Releases](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page, download the two `.tgz` files and install both via `Package Manager` → `Add package from tarball...` in order:
        1. `com.deepmirror.mirrorverse-[version].tgz` — the core MirrorVerse SDK.
        2. `com.deepmirror.mirrorverse.ui-[version].tgz` — reference UI components, depends on the core package, so install second.

- After installation, the Packages window looks like this:

    <img src="https://github.com/user-attachments/assets/157ccffe-099f-4ab3-adb9-a60097b44e9a" width="65%">

- When a newer SDK version is released, download the latest `.tgz` files from the [SDK Releases](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page and reinstall via `Package Manager` → `Add package from tarball...`.


## Configurations

- Make sure the XR plug-ins are installed and enabled for your target platforms.
    - In `Project Settings` → `XR Plug-in Management`:
        - Android tab: enable `ARCore`.
            - Also enable `AREngine` if HarmonyOS is a target.
            - If more than one provider is enabled on the same platform (e.g. both `ARCore` and `AREngine`), uncheck `Initialize XR on Startup` — the SDK will pick the right one at runtime.
        - iOS tab: enable `ARKit`.
- Make sure the Android player is correctly configured.
    - Under `Project Settings` → `Player` → Android tab → `Other Settings`:
        - `Rendering` → `Graphics APIs`: uncheck `Auto Graphics API`, remove `Vulkan` if present, keep only `OpenGLES3`.
            - If HarmonyOS is a target, also uncheck `Multithreaded Rendering` — the AREngine plug-in doesn't support it.
        - `Identification` → `Minimum API Level`: select `API level 29`.
        - `Configuration` → `Scripting Backend`: select `IL2CPP` (not `Mono`).
        - `Configuration` → `Target Architectures`: check `ARM64` only.
- Make sure the iOS player has camera and location usage descriptions filled in.
    - Under `Project Settings` → `Player` → iOS tab → `Other Settings`:
        - Fill in `Configuration` → `Camera Usage Description`.
        - Fill in `Configuration` → `Location Usage Description`.
- Make sure `AR Background Renderer Feature` is enabled on the active URP renderer asset (under `Assets/Settings/` by default).

    <img src="https://github.com/user-attachments/assets/8727c4a9-bf00-458d-9c76-49b95fb86fe7" width="70%">


## Scene Setup

- The `MirrorVerse SDK` and `MirrorVerse SDK UI` packages ship prefabs that cover a wide range of scenarios — from zero-config defaults to fully customizable rendering and interactions.

- If your app uses the default UI or ClassyUI provided by `MirrorVerse SDK UI`, drag one of these prefabs into your scene:

    - DefaultUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_DefaultUI.prefab`
    - ClassyUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_ClassyUI.prefab`

  Each prefab provides everything needed to bootstrap a simple AR scene:
    - Core components: `MirrorSceneImpl`, `ArFoundationAdapter`, `ArFoundationCamera`
    - Customizable visualization component: `MirrorSceneRenderer`
    - Customizable UI component: `MirrorSceneDefaultUI` or `MirrorSceneClassyUI`

- Configure the API key. In the Unity Project view, right-click and choose `Create` → `MirrorVerse` → `App Auth Options` to create an empty App Auth Options asset. Fill in your API key and secret, then drag the asset onto the `MirrorScene` GameObject's `App Auth Options` property in the inspector.
- A few existing scene GameObjects may need adjustment:
    - **EventSystem**: add an `EventSystem` to the scene if there isn't one already.

    <img src="https://github.com/user-attachments/assets/d23db633-5021-419a-ab5c-88b755a386a3" width="70%">

    - **Camera**: the prefab includes an AR camera. Remove or disable the existing main camera.
    - **Light**: tune the `Directional Light` in the scene for the best AR effect on a real device.

- After setup, the scene looks like this:

  <img src="https://github.com/user-attachments/assets/08c01eb6-d976-48ec-8bfb-209b400fac85" width="70%">

- If you're using the ClassyUI interaction components, two URP renderer features need to be enabled on the active renderer asset:
    - `Blur Background Renderer Feature`
    - `Scan Line Renderer Feature`

  <img src="https://github.com/user-attachments/assets/5fdac3f1-ffcd-45fd-b1b9-11293065da7e" width="70%">


## Scripting

With the scene set up, scripts can drive the SDK to create and use a MirrorScene system. Read [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) and its associated events and data structures for the full API surface.

If you're using the `MirrorSceneAll_DefaultUI.prefab` from the `MirrorVerse SDK UI` package, there's a simpler entry point — the prefab already wires up all the glue code. Try the following MonoBehaviour on an empty GameObject in your scene:

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
            // Triggers the MirrorScene UI menu at start.
            DefaultUI.Instance.Restart();
        }
    }

    private void OnMenuFinished()
    {
        // Called once the scene is ready and localized.
        // Now start the game logic.
        // Access scene information via the IMirrorScene interface.
    }
}
```

If you're using `MirrorSceneAll_ClassyUI.prefab` instead, replace `DefaultUI` with `ClassyUI` in the code above.

Then build an Android or iOS package, install on your phone, and you'll have a simple AR scene powered by `MirrorScene`.

Check out the [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) repo for additional sample applications and demos using DefaultUI or ClassyUI.


## Advanced Customization

### Customize Visualization

Several renderer options are exposed for customization, e.g. `StaticMeshRendererOptions`, `PointCloudRendererOptions`. To override defaults:

- Right-click in the Project view and choose `Create` → `MirrorVerse` to create an empty options asset, then fill in your values.
- Drag the new asset onto the corresponding property of the `MirrorSceneRenderer` GameObject.

  <img src="https://github.com/user-attachments/assets/3fe44117-3b6a-47d5-8837-f672d387e169" width="70%">

- For deeper customization, override the renderer classes in the `Renderers/` folder of the `MirrorVerse SDK UI` package — implement your own visualization and hook it up to the `MirrorSceneRenderer` GameObject.
- Or replace the entire visualization stack by inheriting from `SceneRenderer` and swapping the default renderer components in the prefab.

### Customize UI Styles

`MirrorVerse SDK UI` ships two reference interaction flows: `MirrorSceneDefaultUI` and `MirrorSceneClassyUI`. Both are used in the sample apps that come with the SDK. Modify any of the parameters or assets in the components to fit scan, join, or other AR-related flows to your app's style.

### Customize UI Flows

You can also write your own interaction flow from scratch. Once `MirrorScene` is initialized, it transitions through the following state graph during user operations:

<img src="https://github.com/user-attachments/assets/9a4a7e7e-512c-45b6-895a-e180d9346489" width="100%">

Use the `IMirrorScene` interface to read system state, drive the system, e.g. start streaming or exit localization, and inject your own UI between events, calls, and waiting periods. The left diagram shows state transitions for a host device; the right shows the transitions for a guest joining a host's scene.

In this fully-customized flow you don't need the UI components — only the core SDK package.

Below is the minimal code reference that hooks the whole flow up using only `IMirrorScene`, including the multi-user case:

```C#
using UnityEngine;
using MirrorVerse;

public class MyExampleGame : MonoBehaviour
{
    private void Start()
    {
        if (MirrorScene.IsAvailable())
        {
            // Register a handler for the scene-standby event.
            MirrorScene.Get().onSceneStandby += OnSceneStandby;

            // Register a handler for the scene-ready event.
            MirrorScene.Get().onSceneReady += OnSceneReady;

            // There are other useful events to subscribe to.
        }
    }

    public void OnStartButtonClicked()
    {
        // Creates a scene. Once the scene is created, OnSceneStandby fires.
        MirrorScene.Get().CreateScene();
    }

    public void OnJoinButtonClicked()
    {
        // Joins a scene by marker detection.
        MirrorScene.Get().StartMarkerDetection((marker, markerPose, localizedPose) =>
        {
            // Once the scene is joined and loaded, OnSceneStandby fires.
            MirrorScene.Get().JoinScene(marker.sceneId);
        });
    }

    private void OnSceneStandby(StatusOr<SceneInfo> sceneInfo)
    {
        switch (sceneInfo.Value.status)
        {
            case SceneStatus.Empty:
            case SceneStatus.Capturing:
                // Created a scene or joined one just created — start streaming.
                MirrorScene.Get().StartSceneStream();
                break;
            case SceneStatus.Completed:
                // Joined a completed scene — start downloading the mesh.
                MirrorScene.Get().DownloadSceneMesh();
                break;
        }
    }

    public void OnFinishButtonClicked()
    {
        // Finishes streaming. The scene starts processing in the cloud.
        MirrorScene.Get().FinishSceneStream();
    }

    private void OnSceneReady(StatusOr<SceneInfo> sceneInfo)
    {
        // Called when a scene has finished processing and is ready to use.
        if (sceneInfo.HasValue)
        {
            // Start localization so device tracking stays consistent.
            MirrorScene.Get().StartLocalization();
        }
    }

    public void OnExitButtonClicked()
    {
        // Exits the scene.
        MirrorScene.Get().ExitScene();
    }
}
```
