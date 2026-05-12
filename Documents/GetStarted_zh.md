# 开始使用

## 申请 API 密钥

- 我们仍处于早期阶段。请前往 [MirrorScene](https://mirrorscene.deepmirror.com) 首页了解 MirrorVerse 平台，并联系我们购买服务、获取 API 密钥。
- 拿到密钥与 Secret 后，请阅读下方 [场景设置](#场景设置) 一节，完成密钥的配置。


## Unity 版本和包依赖

- Unity **2022.3** LTS 或以上（SDK 当前在 2022.3.62f2 上构建，Unity 6000.x 同样支持）。
- URP (Universal Render Pipeline) **14.0.12** 或以上。MirrorVerse SDK 全部使用 URP 管线。Unity 6000.x 自带的 URP 17 也已支持。
- AR Foundation **5.1.5** 或以上（Unity 6000.x 上的 AR Foundation 6.x 同样可用）。

## 移动设备和操作系统

- **Android**
    - Android 10.0 或以上的手机或平板，且设备支持 ARCore。设备支持列表请参见 Google 的 [ARCore 官方支持列表](https://developers.google.com/ar/devices)。
    - 有些设备支持 ARCore，但出厂时未安装 ARCore Services 或版本较旧。请从应用商店安装或升级 ARCore Services。
- **iOS**
    - iOS 12.0 或以上的 iPhone 或 iPad，且设备 ARKit 版本相符。
- **HarmonyOS**
    - HarmonyOS 4.0 的华为或荣耀手机或平板，且设备支持 AREngine v4.0.0.5 或以上。
    - HarmonyOS NEXT 5.0 设备暂未支持。


## 安装

- **Universal RP**
    - 新工程可在 Unity Hub 中选 **3D (URP)** 模板创建。已有工程请确保安装了 URP 14.0.12 或以上版本（Unity 6000.x 使用 URP 17）。

- **AR Foundation 与 XR 插件**
    - 如果工程还未安装 AR Foundation，请在 Unity Package Manager 中安装 **AR Foundation 5.1.5 或以上**，并安装与之匹配版本的 **ARCore XR Plugin** 与/或 **ARKit XR Plugin**，以支持 Android 和 iOS 平台打包。详见 [安装 AR Foundation 官方指引](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html#installing-ar-foundation)。

    - **AREngine XR Plugin** 不在 AR Foundation 官方支持范围内。若需要在支持 AREngine 的 HarmonyOS 设备上运行,我们提供了一个修改自开源社区的 AREngine XR 插件。请在 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 下载 `com.unity.xr.arengine-[version].tgz`，然后在 Unity 工程的 `Package Manager` → `Add package from tarball...` 菜单中安装。安装后用法与 ARCore XR Plugin 类似，同一 APK 可同时跑在 ARCore 与 AREngine 设备上。

- **安装 MirrorVerse Unity SDK**
    - 在 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 下载两个 `.tgz` 安装包，依次通过 Unity 工程的 `Package Manager` → `Add package from tarball...` 菜单安装：
        1. `com.deepmirror.mirrorverse-[version].tgz` — MirrorVerse SDK 核心组件包。
        2. `com.deepmirror.mirrorverse.ui-[version].tgz` — MirrorVerse SDK UI 包，依赖核心包，必须后装。

- 安装完成后，Packages 窗口如下图所示：

    <img src="https://github.com/user-attachments/assets/157ccffe-099f-4ab3-adb9-a60097b44e9a" width="65%">

- 若发布了新版 MirrorVerse Unity SDK，请前往 [版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 下载最新的 `.tgz` 文件，并在 `Package Manager` → `Add package from tarball...` 中重新安装。


## 工程配置

- 确保目标平台对应的 XR 插件已安装并启用。
    - 在 `Project Settings` → `XR Plug-in Management` 中：
        - Android 标签：勾选 `ARCore`。
            - 若同时支持 HarmonyOS，再勾选 `AREngine`。
            - 若同一平台启用了多个提供方（例如同时勾选 `ARCore` 和 `AREngine`），请取消勾选 `Initialize XR on Startup` — SDK 会在运行时选择合适的提供方。
        - iOS 标签：勾选 `ARKit`。
- Android 平台请确认以下配置：
    - 在 `Project Settings` → `Player` → Android 标签 → `Other Settings` 页面：
        - `Rendering` → `Graphics APIs`：取消勾选 `Auto Graphics API`，移除 `Vulkan`（如有），只保留 `OpenGLES3`。
            - 若以 HarmonyOS 为目标平台,请取消勾选 `Multithreaded Rendering` — AREngine 插件暂不支持。
        - `Identification` → `Minimum API Level`：选择 `API level 29`。
        - `Configuration` → `Scripting Backend`：选择 `IL2CPP`（不是 `Mono`）。
        - `Configuration` → `Target Architectures`：仅勾选 `ARM64`。
- iOS 平台请填写相机与位置使用说明：
    - 在 `Project Settings` → `Player` → iOS 标签 → `Other Settings` 页面：
        - `Configuration` → `Camera Usage Description`：填写相机使用说明文字。
        - `Configuration` → `Location Usage Description`：填写位置使用说明文字。
- 确保工程当前使用的 URP 渲染设置中已启用 `AR Background Renderer Feature`。默认情况下,URP 渲染设置文件位于 `Assets/Settings/` 目录下。

  <img src="https://github.com/user-attachments/assets/8727c4a9-bf00-458d-9c76-49b95fb86fe7" width="70%">


## 场景设置

- `MirrorVerse SDK` 与 `MirrorVerse SDK UI` 包中提供了多种预制体，可应对从零配置默认流程到完全自定义渲染与交互的各种场景需求。

- 若直接使用默认提供的两种交互界面与流程，可从以下目录中将完整预制体之一拖入场景：

    - DefaultUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_DefaultUI.prefab`
    - ClassyUI: `Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll_ClassyUI.prefab`

  预制体已经包含构建一个简易 AR 场景所需的全部内容：
    - 核心组件：`MirrorSceneImpl`、`ArFoundationAdapter`、`ArFoundationCamera`
    - 可配置或替换的可视化组件：`MirrorSceneRenderer`
    - 可配置的交互流程组件：`MirrorSceneDefaultUI` 或 `MirrorSceneClassyUI`

- 配置 API 密钥。在 Unity 工程窗口中右键 `Create` → `MirrorVerse` → `App Auth Options`，创建一个空的 App Auth Options 资源文件，填入之前获得的 API 密钥与 Secret，然后将其拖至 `MirrorScene` GameObject 的 `App Auth Options` 属性。
- 场景中已有的常见 GameObject 可能需要调整：
    - **EventSystem**：若场景中没有，请添加一个 `EventSystem`，交互组件需要用到。

    <img src="https://github.com/user-attachments/assets/d23db633-5021-419a-ab5c-88b755a386a3" width="70%">

    - **相机**：核心组件预制体已包含 AR 相机，请关闭或移除场景中原有的主相机。
    - **光源**：默认 `Directional Light` 可直接使用；如需更好的 AR 效果，或场景中还有其他光源，可调整参数以达到最佳呈现。

- 设置完成后，场景如下图所示：

  <img src="https://github.com/user-attachments/assets/08c01eb6-d976-48ec-8bfb-209b400fac85" width="70%">

- 若选择使用 ClassyUI 交互组件，需在当前 URP 渲染设置中启用以下两个渲染器特性：
    - `Blur Background Renderer Feature`
    - `Scan Line Renderer Feature`

  <img src="https://github.com/user-attachments/assets/5fdac3f1-ffcd-45fd-b1b9-11293065da7e" width="70%">


## 代码样例

场景设置完成后，可通过脚本驱动 SDK，创建并使用 MirrorScene 系统。请阅读 [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) 接口及其相关的事件和数据结构定义。

若使用 `MirrorVerse SDK UI` 程序包提供的完整预制体，例如 `MirrorSceneAll_DefaultUI.prefab`，可以使用更简洁的代码进行驱动 — 预制体已实现大部分串接代码和交互逻辑。参考以下代码，挂在场景中一个空的 GameObject 上即可：

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

若使用 `MirrorSceneAll_ClassyUI.prefab`，将代码中的 `DefaultUI` 换成 `ClassyUI` 即可。

完成后构建 Android 或 iOS 包,安装到手机即可看到由 `MirrorScene` 驱动的简易 AR 场景。

更多用 DefaultUI 或 ClassyUI 实现的样例,请参见 [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) 仓库。


## 高级定制

### 自定义可视化组件

`MirrorVerse SDK UI` 包的 `Renderers` 目录下包含 `MirrorSceneRenderer` 预制体以及一系列渲染组件的实现。

这些组件可通过 `StaticMeshRendererOptions`、`PointCloudRendererOptions` 等选项资源进行定制。要覆盖默认配置：

- 右键 `Create` → `MirrorVerse` → `XXX Options` 创建相应的选项资源，填入新参数，并将其拖到对应渲染组件的属性处。

  <img src="https://github.com/user-attachments/assets/3fe44117-3b6a-47d5-8837-f672d387e169" width="70%">

- 如需更深度的定制，可重载 `MirrorVerse SDK UI` 包中 `Renderers/` 目录下的渲染器类，或者继承 `SceneRenderer` 接口，替换默认的渲染组件，从头实现整套可视化逻辑。

### 自定义交互样式

`MirrorVerse SDK UI` 包提供的两套交互流程 `MirrorSceneDefaultUI` 与 `MirrorSceneClassyUI` 在样例程序中被广泛使用。开发者可以修改或替换组件中的任意参数或资源，让扫描、加入场景等流程符合应用自身的风格。

### 自定义交互流程

也可以从零编写自定义的交互流程。`MirrorScene` 系统启动后，会按下方状态图在用户操作中进行状态切换：

<img src="https://github.com/user-attachments/assets/9a4a7e7e-512c-45b6-895a-e180d9346489" width="100%">

通过 `IMirrorScene` 接口可读取系统状态、驱动状态切换（例如开始扫描、退出定位等），并在事件、调用、等待之间插入自定义 UI。左图为主机端状态切换，右图为加入主机场景的客户端状态切换。

在这种完全自定义流程下,无需安装 `MirrorVerse SDK UI` 包，只需核心 `MirrorVerse SDK` 即可。

以下是一个最简代码框架,仅使用 `IMirrorScene` 接口完成完整流程，包括多用户加入场景的情形：

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
