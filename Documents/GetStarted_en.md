# Get Started

## Request for API Key
- We are still in early stage. Please visit our [home page](https://mirrorscene.deepmirror.com) to learn more about MirrorVerse Platform, and reach out to us to request access and purchase our service.
- Once you get the API key and secret, follow the [Scene Setup](#scene-setup) for how to setup the API key.


## Unity Version and Dependencies

- Unity Version 2021.3 or above.
- URP (Universal Render Pipeline) 12.1.11 or above. MirrorVerse SDK uses URP for rendering.
- AR Foundation 5.1.5 or above.

## Mobile Devices and Operating System
- **Android**
    - Android 10.0 or above for Android mobile phones or tablets which support ARCore v1.42.X or above。Please refer to the Google ARCore's [officla supported devices](https://developers.google.com/ar/devices) list.
    - Some devices support ARCore but does not have proper version of ARCore installed out of factory. Please install latest ARCore service from app store.
- **iOS**
    - iOS 12.0 or above for iPhones or iPads and with highest version of ARKit on the devices.

- **HarmonyOS**
    - HarmonyOS 4.0 for Huawei or Honor phones or tablets with AREngine v4.0.0.5 or above.
    - HarmonyOS Next 5.0 devices are currently not supported.



## Installation

- **Universal RP**
    - If creating a new project, start a 3D (URP) project from Unity Hub, or make sure the existing project has URP 12.1.11 or above installed.

- **AR Foundation and XR Plugins**
    - If the project does not have AR Foundation installed, add AR Foundation 5.1.5 or above from Unity Package Manager, and it's coresponding version of ARCore XR Plugin and/or ARKit XR Plugin. Please refer to [Installing AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html#installing-ar-foundation) instruction. 

    - AREngine XR Plugin is not officially supported by Unity. In order to run on HarmonyOS devices that support AREngine, our SDK provides an AREngine Unity XR Plugin ported from open source community. Please download the  `com.unity.xr.arengine-[version].tgz` file from [SDK Releases List](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page, and use  `Package Manager` -> `Add Package from tarball...` menu to install. After installation, this plugin works similar to ARCore XR Plugin, and supports one APK binary running on both ARCore devices and AREngine devices. 

- **Install MirrorVerse Unity SDK**
    - From the [SDK Releases List](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page, download the two .tgz files of MirrorVerse SDK packages, in use  `Package Manager` -> `Add Package from tarball...` menu to install both:
        - First install `com.deepmirror.mirrorverse-[version].tgz` which is the core library of MirrorVerse SDK.
        - Then install `com.deepmirror.mirrorverse.ui-[version].tgz` which contains the UI and visualization components of MirrorVerse SDK.

- After all installed, it looks as follows:

    <img src="https://github.com/user-attachments/assets/157ccffe-099f-4ab3-adb9-a60097b44e9a" width="70%">

- If a newer version of MirrorVerse Unity SDK has released, please visit [SDK Releases List](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page to download the latest version of .tgz files.  And re-install the newer version from `Package Manager` -> `Add Package from tarball...` menu in the Unity project.


## Configurations

- Make sure the XR plugins are installed and enabled for your target platforms.
    - Under `XR Plugin-in Management` in settings
        - Check `ARCore` under Android tab.
            - Check `AREngine` if HarmonyOS is target platform.
            - If more than one platform are checked (e.g. both `ARCore` and `AREngine` are checked), uncheck `Initialize XR on Startup`.
        - Check `ARKit` under iOS tab.
- Make sure Android player has correctly configured.
    - Under  `Build Settings` -> `Player Settings` -> `Player` -> `Android` -> `Other Settings`,
        - Select `Rendering` -> `Graphics APIs`, uncheck `Auto Graphics API`, remove `Vulkan` if exists, and keep only `OpenGLES3`.
            - If HarmonyOS is target platform, uncheck `Multithreaded Rendering`. AREngine plugin does not support this for now.
        - Select `Identification` -> `Minimum API Level` to `API level 29`.
        - Select `Configuration` -> `Scripting Backend` to `IL2CPP` instead of `Mono`.
        - Check `Configuration` -> `Target Architectures` to `ARM64` instead of others.
- Make sure iOS player has correctly configured camera and location usage description.
    - Under  `Build Settings` -> `Player Settings` -> `Player` -> `iOS` -> `Other Settings`,
        - Add descripion text for `Configuration` -> `Camera Usage Description` field.
        - Add descripion text for `Configuration` -> `Location Usage Description` field.
- Make sure `AR Background Renderer Feature` has been enabled for your active URP renderer setting. By default, the URP renderer settings assets are under `Assets/Settings` folder in Unity project.

    <img src="https://github.com/user-attachments/assets/8727c4a9-bf00-458d-9c76-49b95fb86fe7" width="70%">


## Scene Setup

- `MirrorVerse SDK` and `MirrorVerse SDK UI` contain various prefabs to fulfill Unity scenes with different requirement, from using default UI and visuals with almost zero configuration, to fully configurable rendering and interactions.

- If your application directly uses DefaultUI or CLassyUI provided by `MirrorVerse SDK UI` package, you can directly drag one of the prefabs to your scene in Unity from the following package folders:
    
    - DefaultUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_DefaultUI.prefab`
    - ClassyUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_ClassyUI.prefab`

  Each of these prefabs provides all necessary components needed to setup a simple AR environment in your Unity scene.
    - Core components: `MirrorVerseImpl`, `ArFoundationAdapter`，`ArFoundationCamera`
    - Customizable visuzalization component: `MirrorSceneRenderer`
    - Customizable UI component: `MirrorSceneDefaultUI` or `MirrorSceneClassyUI`

- Setup the API key. In your Unity project, right click to open `Create` -> `MirrorVerse` -> `App Auth Options` to create an empty app auth assets, and fill your API key and secret there. Drag the `appAuthOptions.asset` file to the `MirrorSceneImpl` game object's `App Auth Options` property.
- Some existing Unity game objects need to be configured
    - EventSystem: If not yet, add `EventSystem` to your Unity scene:

    <img src="https://github.com/user-attachments/assets/d23db633-5021-419a-ab5c-88b755a386a3" width="70%">

    - Camera: The core component prefab contains an AR camera. Remove or deactive original normal camera if there are any.
    - Light: Adjust the `Directional Light` in the Unity scene to get the best AR effect on real device.

- After the setup, the Unity project looks like this:

  <img src="https://github.com/user-attachments/assets/08c01eb6-d976-48ec-8bfb-209b400fac85" width="70%">

- If your application chooses to use ClassyUI interaction components, there are two URP renderer features needs to be enabled in the project, as following picture shows:
    - `Blur Background Renderer Feature`
    - `Scan Line Renderer Feature`

  <img src="https://github.com/user-attachments/assets/5fdac3f1-ffcd-45fd-b1b9-11293065da7e" width="70%">


## Scripting

After the setup in Unity scene, now we can use scripts to trigger SDK to create and use MirrorScene system. Please read [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) interface and related events and data structures.

If using the ``MirrorSceneAll_DefaultUI.prefab` components provided from `MirrorVerse SDK UI` package, there is simpler way to just use the UI component, because all glue codes are implemented already. Try the following codes and add this MonoBehaviour to an empty game object in the Unity scene:

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
If using the `MirrorSceneAll_ClassyUI.prefab` instead, then simply replace `DefaultUI` to `ClassyUI` in the codes above.

Then build Android or iOS package and install on your phone to run a simple AR scene powered by `MirrorScene`.

Please checkout [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) repo for other sample applications and demos uses DefaultUI or ClassyUI.


## Advanced Customization

### Customize Visualization

There are several options or configurations that are configurable, like `StaticMeshRendererOptions`, `PointCloudRendererOptions` etc. If you want to override the options, you can:
- Use right click context menu `Create` -> `MirrorVerse` to create an empty options instance asset, and fill in values yourself.
- Then drag the new asset to the cooresponding property of the `MirrorSceneRenderer` game object.

  <img src="https://github.com/user-attachments/assets/3fe44117-3b6a-47d5-8837-f672d387e169" width="70%">

- If you want further customization the rendering, you can also override the renderer classes in `/Renderers` folder in `MirrorVerse SDK UI` package. Just implement your own visualization and hook them up to `MirrorSceneRenderer` game object.
- Or implement entire visualization logic by inheriting `SceneRenderer` interface and replace default rendering components in the prefab.

### Customize UI Styles

`MirrorVerse SDK UI` package provides two sets of interaction flows: `MirrorSceneDefaultUI` and `MirrorSceneClassyUI`, which are used in sample apps come with the SDK. You can modify or replace any of the parameters or assets in the components to make scan, join or AR related operations with your customimized styles.

### Customize UI Flows

You can also write your own flows of interactions. After `MirrorScene` initialized, the system will transition its states in a graph during user's operations. 

<img src="https://github.com/user-attachments/assets/9a4a7e7e-512c-45b6-895a-e180d9346489" width="100%">

You can use `IMirrorScene` interface to access information from the system, or operate the system, e.g. start streaming or exit localiztion.  You can also can add UI between events, calls and waitings during the MirrorScene operation flows. See the diagrams above, the left diagram illustrates the state transitions for a host device, and the right diagram illustrates the state transition graph for a guest.

Thu, in this senario the fully customized flow does not use the SDK provided UI components, but only use the core SDK package.

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