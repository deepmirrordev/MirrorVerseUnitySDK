# 开始使用

## 申请API密钥

- 首先，请在 [MirrorScene](https://mirrorscene.deepmirror.com) 首页联系我们购买服务，获得 API 的密钥。
- 获得密钥之后，请阅读 [场景设置](#场景设置) 这一节配置密钥，以获得调用 API 的权限。


## Unity版本和包的依赖

- Unity Version 2021.3 版本或以上。
- URP (Universal Render Pipeline) 12.1.11 版本或以上。 MirrorVerse SDK 全部使用 URP 管线。
- AR Foundation 5.1.5版本或以上。

## 移动设备和操作系统
- **Android**
    - Android 10.0 或以上的安卓手机或平板移动设备，并且设备支持 ARCore v1.42.X 版本或以上。详细设备支持列表请参见此 [官方列表](https://developers.google.com/ar/devices) 。
    - 有些设备支持ARCore，但出厂操作系统没有安装ARCore Service或者版本太旧，需要自行从商店安装或升级。
- **iOS**
    - iOS 12.0 或以上的苹果手机或平板移动设备，并且设备支持 ARKit 相应最新版本。

- **HarmonyOS**
    - HarmonyOS 4.0 的华为或荣耀手机或平板移动设备，并且设备支持 AREngine v4.0.0.5 版本。
    - HarmonyOS Next 5.0 的设备暂未支持


## 安装

- **Universal RP**
    - 如果是新工程，可以在 Unity Hub 中创建 3D (URP) 工程。或者确保当前工程安装了 URP 12.1.11 或以上版本。

- **AR Foundation and XR Plugins**
    - 如果工程还未安装 AR Foundation，可在 Unity Package Manager 里面安装 AR Foundation 5.1.5 或以上版本，以及与 AR Foundation 相同版本的 ARCore XR Plugin 和 ARKit XR Plugin，以支持 Android 和 iOS 平台的打包。具体可以参见 [安装 AR Foundation 官方指引](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html#installing-ar-foundation) 。

    - AREngine XR Plugin 没有 AR Foundation 官方的支持。如果需要在支持 AREngine 的 HarmonyOS 鸿蒙系统的设备上运行，我们提供了一个修改自开源社区的非官方版本的 AREngine XR Plugin。请在 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 中下载 `com.unity.xr.arengine-[version].tgz` 安装包，然后在Unity工程里面的 `Package Manager` -> `Add Package from tarball...` 菜单里安装，装好后和 ARCore 插件使用方式类似，可以支持打 APK 包 同时支持 ARCore 或 AREngine 的设备。

- **MirrorVerse Unity SDK**
    - 在 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 中下载 MirrorVerse 的两个 .tgz 文件包，然后在Unity工程里面的 `Package Manager` -> `Add Package from tarball...` 菜单里安装：
    - 先安装 `com.deepmirror.mirrorverse-[version].tgz` ，这是 MirrorVerse SDK 核心组件包
    - 再安装 `com.deepmirror.mirrorverse.ui-[version].tgz` ，这是 MirrorVerse SDK UI 包，依赖核心组件包，因此必须后装。

- 安装好后如下图所示：

    <img src="https://github.com/user-attachments/assets/157ccffe-099f-4ab3-adb9-a60097b44e9a" width="70%">

- 如果 MirrorVerse Unity SDK 有新版本发布，请在 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 中下载对应的新版本的 .tgz 文件包，重新在 Unity 工程里面的 `Package Manager` -> `Add Package from tarball...` 菜单里选择新版本的安装包安装即可。


## 工程配置

- 确保需要支持平台相应的 XR Plugin 已经安装并且启用
    - 在工程设置中前往 `XR Plugin-in Management`
        - Android 标签 -> `ARCore` 打勾
            - 如果想同时支持鸿蒙系统 -> `AREngine` 打勾
            - 如果打勾支持超过一个平台（比如 `ARCore` 和 `AREngine` 都支持），`Initialize XR on Startup` 的勾要去掉
        - iOS 标签 -> `ARKit` 打勾
- 在 Android 平台上，确保以下配置正确：
    - 在工程设置中前往：`Player` -> `Android标签` -> `Other Settings` 页签：
        - 在 `Rendering` -> `Graphics APIs` 中去除勾选 `Auto Graphics API`, 删除 `Vulkan` 而只保留 `OpenGLES3`
            - 如果需要支持鸿蒙系统，去掉 `Multithreaded Rendering` 的勾，目前 AREngine 插件不支持此项。
        - 在 `Identification` -> `Minimum API Level` 中选择 `API level 29`
        - 在 `Configuration` -> `Scripting Backend` 中选择 `IL2CPP` （而不是 `Mono`）
        - 在 `Configuration` -> `Target Architectures` 中选择 `ARM64` （其他不支持）
- 在 iOS 平台上，确保填写了使用相机和位置的描述：
    - 在工程设置中前往：`Player` -> `iOS标签` -> `Other Settings` 页签：
        - 在 `Configuration` -> `Camera Usage Description` 中填写相机使用说明
        - 在 `Configuration` -> `Location Usage Description` 中填写位置使用说明
- 在工程的 URP 设置中，确保运行或打包时使用的 URP 设置里的 `AR Background Renderer Feature` 渲染特性已被选中启用。默认工程里面，URP 渲染设置文件在 `Assets/Settings` 目录里面。
      
  <img src="https://github.com/user-attachments/assets/8727c4a9-bf00-458d-9c76-49b95fb86fe7" width="70%">


## 场景设置

- `MirrorVerse SDK` 和 `MirrorVerse SDK UI` 包中提供了已经配置好的预制体，方便开发者组装到各种不同需求的 Unity 工程的场景中，包括极简的配置直接用默认流程就能运行，到复杂的自定义流程或者自定义视觉效果。

- 如果直接使用默认提供的两种交互界面和流程，可以直接从以下目录中将包含所有模块的预制体中的一个拖入场景：
    
    - DefaultUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_DefaultUI.prefab`
    - ClassyUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_ClassyUI.prefab`

  预制体包含了创建一个简单 AR 场景的所有内容，其中主要有：
    - 系统核心组件：`MirrorVerseImpl`, `ArFoundationAdapter`，`ArFoundationCamera`
    - 可配置或替换的可视化组件：`MirrorSceneRenderer`
    - 两套可配置的交互流程组件：`MirrorSceneDefaultUI` 或 `MirrorSceneClassyUI`

- 配置 API 密钥。用鼠标右键菜单 `Create` -> `MirrorVerse` -> `App Auth Options`来创建一个空的 `appAuthOptions.asset` 资源文件在工程中，填入之前获得的 API 密钥，并将这个资源文件拖到 `MirrorSceneImpl`对象的` App Auth Options`属性。
- 工程场景中已有的 Unity 常用对象看情况需要调整：
    - 事件系统：`EventSystem` 如果没有，添加一个到工程场景中，交互组件需要。 如下图所示:

    <img src="https://github.com/user-attachments/assets/d23db633-5021-419a-ab5c-88b755a386a3" width="70%">

    - 相机：核心组件预制体中含有 AR 相机，如果以 AR 相机为主相机，可以关掉场景原本主相机。
    - 光源：默认的光源 `Directional Light` 参数可以直接使用，但效果如果不够好，或者工程场景中还有其他光源，可以调整光源参数以达到最好的 AR 效果。

- 场景设置完成后，如下图所示：

  <img src="https://github.com/user-attachments/assets/08c01eb6-d976-48ec-8bfb-209b400fac85" width="70%">

- 如果选择使用 ClassyUI 交互组件，需要打开 URP 设置中的添加以下两个渲染器特性，如下图所示：
    - `Blur Background Renderer Feature`
    - `Scan Line Renderer Feature`

  <img src="https://github.com/user-attachments/assets/5fdac3f1-ffcd-45fd-b1b9-11293065da7e" width="70%">


## 代码样例

设置完成后，接下来用代码驱动 MirrorScene API 来开启制作和使用场景。请阅读 [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) 接口，以及相应的事件和数据结构定义。

如果使用 `MirrorVerse SDK UI` 程序包所提供的完整预制体，比如 `MirrorSceneAll_DefaultUI.prefab` 所提供的可视化组件以及交互流程，可以使用更为简单的代码来驱动，因为大部分串接代码和交互代码，组件包已经提供。参考以下代码，挂在工程场景中的一个空的对象上即可。

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

如果使用 `MirrorSceneAll_ClassyUI.prefab` 所提供的交互组件，将代码中的 `DefaultUI` 换成 `ClassyUI` 。

生成 Android 包或 iOS 包后，就可以运行在相应手机或平板上看到一个简单的 MirrorScene 驱动的 AR 场景。

请参见 [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) 仓库，里面含有用 DefaultUI 或 ClassyUI 实现的几个简单示例。


## 高级定制

### 自定义可视化控件

在 `MirrorVerse SDK UI` 程序包中的 `Renderers` 目录下，含有 `MirrorSceneRenderer` 预制体和一系列渲染组件的实现。

这些渲染组件都是可以通过选项定制的，比如 `StaticMeshRendererOptions` ， `PointCloudRendererOptions` 等等。开发者可以创建新的选项文件，配置新的参数，覆盖默认的选项。
- 用鼠标右键菜单 `Create` -> `MirrorVerse` -> `XXX Options` 创建相应的选项文件，填入新的参数，并将这个文件拖到相应的渲染组件对象属性处。如下图所示。
  
  <img src="https://github.com/user-attachments/assets/3fe44117-3b6a-47d5-8837-f672d387e169" width="70%">

开发者可以选择修改重载个别渲染组件，或者重新继承 `SceneRenderer` ，替换默认的组件来重新开发整个可视化效果。

### 自定义交互样式

在 `MirrorVerse SDK UI` 程序包中包含的交互控件 `MirrorSceneDefaultUI` 或 `MirrorSceneClassyUI` 预制体，在 SDK 自带的样例程序中被广泛使用。开发者可以自行修改或覆盖组件各处的代码或参数，或者自行开发符合自己应用风格的交互控件进行替换，改变扫描，扫码，进入场景等操作流程，使之符合开发者工程的风格样式。

### 自定义交互流程

开发者还可以自定义整个交互流程。MirrorScene 系统启动之后，会进入以下状态流程：

<img src="https://github.com/user-attachments/assets/9a4a7e7e-512c-45b6-895a-e180d9346489" width="100%">

用户在创建，扫描，加入场景等操作过程中状态会随之改变，或者等待下一步操作。左图为单机或者主机的状态流程，右图为扫码加入者的状态流程。在这种情况下，只需要将 `MirrorVerse SDK` 程序包里面的核心组件预制体 `Packages/MirrorVerse SDK/Prefabs/MirrorScene.prefab` 拖入 Unity 工程的场景中，开发者再通过 `IMirrorScene` 来获取系统相关信息，或者操作这些流程，比如开始扫描，下载场景，退出等等。

因此，完全自定义的交互流程不需要安装 `MirrorVerse SDK UI` 包，而只需要核心包 `MirrorVerse SDK` 即可。

以下是一个简单的调用顺序框架，仅使用核心组件 `IMirrorVerse` 的接口来完成整个流程逻辑，包括多人情形下的加入他人场景流程，而开发者可以自行实现所需的交互流程：

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
