# 开始使用

## 申请API密钥

- 首先，请在 [MirrorScene](https://mirrorscene.deepmirror.com) 首页联系我们申请 API 的密钥。
- 获得密钥之后，请阅读 [场景设置](#场景设置) 这一节配置密钥，以获得调用 API 的权限。


## Unity版本和包的依赖

- Unity Version 2021.3 版本或以上。
- URP (Universal Render Pipeline) 12.1.6 版本或以上。 MirrorVerse SDK 全部使用 URP 管线。
- AR Foundation 4.2.6版本或以上，包括相应版本的 ARCore 和 ARKit 插件。


## 安装

- 如果是新工程，可以在Unity Hub中创建3D(URP)工程。或者确保当前工程安装了URP 12.1.6或以上版本

- 如果工程还未安装 AR Foundation，可在 Unity Package Manager 里面安装 AR Foundation 4.2.6 或以上版本，以及相应版本的 ARCore XR Plugin 和 ARKit XR Plugin，以支持 Android 和 iOS 平台的打包。

- 安装 MirrorVerse Unity SDK

    - 在[版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases)中直接下载 .unitypackage 资产包导入Unity工程
        - 在工程中导入所有文件即可。
        - 这个资产包含有`MirrorVerse SDK`和`MirrorVerse SDK UI`两个程序包，是SDK的所有内容。
        - 用导入方式安装，这两个程序包不会出现在 Unity Package Manager中，由开发者自行在工程中管理。

    - 或者使用 Unity Package Manager 通过 Git URL 安装：
        - MirrorVerse SDK：`https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse`
            - 这个包是API的核心组件
            - 也可以在 `Packages/manifest.json`文件中添加一行依赖项：
                ```yaml
                "com.deepmirror.mirrorverse": "https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse"
                ```

        - MirrorVerse SDK UI：`https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse.UI`
            - 这个包含有常用的界面，渲染和可视化组建，方便快速创建应用
            - 也可以在 `Packages/manifest.json`文件中添加一行依赖项：
                ```yaml
                "com.deepmirror.mirrorverse.ui": "https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse.UI"
                ```

        - 安装好后如下图所示：

            <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/ec9b1345-52c2-4dda-8580-3f3a70686f2f" width="65%">


## 工程配置

- 确保ARCore或者ARKit已经安装并且启用
    - 在工程设置中前往 `XR Plugin-in Management`
        - Android 标签 -> ARCore 打勾
        - iOS 标签 -> ARKit 打勾
- 在安卓平台上，确保以下配置正确：
    - 在工程设置中前往：`Player` -> `Android标签` -> `Other Settings` 页签：
        - 在 `Rendering` -> `Graphics APIs` 中去除勾选 `Auto Graphics API`, 删除 `Vulkan` 而只保留 `OpenGLES3`
        - 在 `Identification` -> `Minimum API Level` 中选择 `API level 28`
        - 在 `Configuration` -> `Scripting Backend` 中选择 `IL2CPP` （而不是 `Mono`）
        - 在 `Configuration` -> `Target Architectures` 中选择 `ARM64` （其他不支持）
        
- 在工程的 URP 设置中，确保运行或打包时使用的 URP 设置里的 `AR Background Renderer Feature` 渲染特性已被选中启用。默认工程里面，URP 渲染设置文件在`Assets/Settings`目录里面：
      
  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/00c1c0a3-73e9-4c03-84e7-ed80ad57b8b0" width="70%">


## 场景设置

在 Unity 工程中的场景里，配置以下预制体，启用 MirrorScene API：
- 将`Packages/MirrorVerse SDK UI/Prefabs/MirrorSceneAll.prefab`这个包含系统库，可视化控件和交互控件的预制体直接拖入工程场景中，成为一个对象。
- 这个预制体包含了创建一个简单AR场景的所有内容，其中主要有：
    - 系统核心组件：`MirrorVerseImpl`, `ArFoundationAdapter`，`ArFoundationCamera`
    - 可替换的可视化组件：`MirrorSceneRenderer`
    - 可替换的交互组件：`MirrorSceneDefaultUI`
- 配置 API 密钥。用鼠标右键菜单 `Create` -> `MirrorVerse` -> `App Auth Options`来创建一个空的 `appAuthOptions.asset` 资源文件在工程中，填入之前获得的 API 密钥，并将这个资源文件拖到`MirrorScene`预制体对象的`App Auth Options`属性。
- 工程场景中已有的Unity常用对象看情况需要调整：
    - 事件系统：`EventSystem` 如果没有，添加一个到工程场景中，交互组件需要。
    - 相机：核心组件预制体中含有 AR 相机，如果以 AR 相机为主相机，可以关掉场景原本主相机。
    - 光源：默认的光源`Directional Light`参数可以直接使用，但效果如果不够好，或者工程场景中还有其他光源，可以调整光源参数以达到最好的AR效果。

- 拖拽完成后，如下图所示：

  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/8024c541-f9f1-4f43-8757-0a3fbbf03922" width="70%">


## 代码样例

设置完成后，接下来用代码驱动 MirrorScene API 来开启制作和使用场景。请阅读 [`IMirrorScene`](../Project/Assets/MirrorVerse/Scripts/MirrorScene/IMirrorScene.cs) 接口，以及相应的事件和数据结构定义。

如果使用 `MirrorVerse SDK UI`程序包所提供的`MirrorSceneAll.prefab`预制体和默认可视化组件以及交互流程，可以使用更为简单的代码来驱动，因为大部分串接代码和交互代码，组件包已经提供。参考以下代码，挂在工程场景中的一个空的对象上即可：
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
打 Android 包或打 iOS 包后，就可以在相应手机上看到一个简单的 MirrorScene 驱动的 AR 场景。

更多的实机样例，请参见 [MirrorSceneExamples](https://github.com/deepmirrorinc/MirrorSceneExamples) 仓库。


## 高级定制

### 自定义可视化控件

在`MirrorVerse SDK UI`程序包中的`Renderers`目录下，含有`MirrorSceneRenderer`预制体和一系列渲染组件的实现。

这些渲染组件都是可以通过选项定制的，比如`StaticMeshRendererOptions`， `PointCloudRendererOptions`等等。开发者可以创建新的选项文件，配置新的参数，覆盖默认的选项。
- 用鼠标右键菜单 `Create` -> `MirrorVerse` -> `XXX Options` 创建相应的选项文件，填入新的参数，并将这个文件拖到相应的渲染组件对象属性处。如下图所示。
  
  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/adb3a637-d7e4-40d8-ac95-09210a699329" width="70%">

开发者可以选择修改重载个别渲染组件，或者重新继承`SceneRenderer`，替换默认的组件来重新开发整个可视化效果。

### 自定义交互样式

在`MirrorVerse SDK UI`程序包中包含的交互控件 `MirrorSceneDefaultUI`预制体，在 SDK 自带的样例程序中被广泛使用。开发者可以自行修改或者自行开发符合自己应用风格的交互控件，在 MirrorScene 一系列扫描，扫码，进入场景等操作中指定符合开发者工程的风格样式。

### 自定义交互流程

开发者还可以自定义整个交互流程。MirrorScene系统启动之后，会进入以下状态流程。

<img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/e3dc82b2-075b-45a4-b047-7920dad51680" width="100%">

用户在创建，扫描，加入场景等操作过程中状态会随之改变，或者等待下一步操作。左图为单机或者主机的状态流程，右图为扫码加入者的状态流程。在这种情况下，只需要将`MirrorVerse SDK`程序包里面的核心组件预制体 `Packages/MirrorVerse SDK/Prefabs/MirrorScene.prefab`拖入 Unity 工程的场景中，开发者再通过`IMirrorScene`来获取系统相关信息，或者操作这些流程，比如开始扫描，下载场景，退出等等。

以下是一个简单的调用顺序框架，仅使用核心组件`IMirrorVerse`的接口来完成整个流程逻辑，包括多人情形下的加入他人场景流程，而开发者可以自行实现所需的交互流程：

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
