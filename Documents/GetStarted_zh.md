# 开始使用

## 申请API密钥

- 首先，请在 [MirrorScene](https://mirrorscene.deepmirror.com) 首页联系我们申请 API 的密钥。
- 获得密钥之后，请阅读 [场景设置](#场景设置) 这一节配置密钥，以获得调用 API 的权限。


## Unity版本和包的依赖

- Unity Version 2021.3 版本或以上。如果使用2022或更新版本的Unity，请自行升级和完成必要的资源格式转换，转好后不影响使用。
- URP (Universal Render Pipeline) 12.1.6 版本或以上。 MirrorVerse SDK 全部使用 URP 管线。
- AR Foundation 4.2.6版本或以上，包括相应版本的 ARCore 和 ARKit 插件。


## 安装

- 如果是新工程，可以在Unity Hub中创建3D (URP)工程。

- 如果是已有工程，确保当前工程是URP管线的工程，并且安装了URP 12.1.6或以上版本。

- 如果工程还未安装 AR Foundation，可在 Unity Package Manager 里面安装 AR Foundation 4.2.6 或以上版本，以及相应版本的 ARCore XR Plugin 和 ARKit XR Plugin，以支持 Android 和 iOS 平台的打包。具体可以参见[Installing AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/index.html#installing-ar-foundation)。

- 安装 MirrorVerse Unity SDK

    - SDK包含两个程序包，其中：
        - `MirrorVerse SDK`: 这个包是API的核心组件，必选安装。
        - `MirrorVerse SDK UI`: 这个包含有常用的界面，渲染和可视化组建，方便快速创建应用，可选安装。

    - 通过 tarball 文件安装：
        - 在[版本发布列表](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases)中下载相应版本两个.tgz包，保存到本地。
            - `com.deepmirror.mirrorverse-#.#.#.tgz`
            - `com.deepmirror.mirrorverse.ui-#.#.#.tgz`
        - 在Unity Package Manager窗口左上角加号添加 tarball 安装方式按顺序安装以上两个文件。

    - 通过 git URL 安装：
        - 在Unity Package Manager窗口左上角加号添加 git URL 安装，粘贴以下git URL：
            - `https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse`
            - `https://github.com/deepmirrordev/MirrorVerseUnitySDK.git?path=/Project/Assets/MirrorVerse.UI`
    
    - 注意：请先安装 `MirrorVerse SDK`，再添加 `MirrorVerse SDK UI`，因为两个包之间有依赖关系。

    - 安装好后如下图所示：

        <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/08884668-6012-4019-a73d-0d164693e5b6" width="70%">


## 工程配置

- 确保ARCore或者ARKit已经安装并且启用
    - 在工程设置中前往 `XR Plugin-in Management`
        - Android 标签 -> ARCore 打勾
        - iOS 标签 -> ARKit 打勾
- 在安卓平台上，确保以下配置正确：
    - 在工程设置中前往：`Player` -> `Android标签` -> `Other Settings` 页签：
        - 在 `Rendering` -> `Graphics APIs` 中去除勾选 `Auto Graphics API`, 删除 `Vulkan` 而只保留 `OpenGLES3`
        - 在 `Identification` -> `Minimum API Level` 中选择 `API level 29`
        - 在 `Configuration` -> `Scripting Backend` 中选择 `IL2CPP` （而不是 `Mono`）
        - 在 `Configuration` -> `Target Architectures` 中选择 `ARM64` （其他不支持）
- 在iOS平台上，确保填写了使用相机和位置的描述：
    - 在工程设置中前往：`Player` -> `iOS标签` -> `Other Settings` 页签：
        - 在 `Configuration` -> `Camera Usage Description` 中填写相机使用说明
        - 在 `Configuration` -> `Location Usage Description` 中填写位置使用说明
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
    - 事件系统：`EventSystem`如果没有，添加一个到工程场景中，交互组件需要。 如下图所示:

      <img src="https://github.com/deepmirrordev/MirrorSceneExamples/assets/53016403/67e2af8e-49c3-4c8f-8058-2b1084d22eb5" width="70%">

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

更多的实机样例，请参见 [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) 仓库。


## 高级定制

### 自定义可视化控件

在`MirrorVerse SDK UI`程序包中的`Renderers`目录下，含有`MirrorSceneRenderer`预制体和一系列渲染组件的实现。

这些渲染组件都是可以通过选项定制的，比如`StaticMeshRendererOptions`， `PointCloudRendererOptions`等等。开发者可以创建新的选项文件，配置新的参数，覆盖默认的选项，比如更改材质，配置颜色等等。
- 用鼠标右键菜单 `Create` -> `MirrorVerse` -> `XXX Options` 创建相应的选项文件，填入新的参数，并将这个文件拖到相应的渲染组件对象属性处。如下图所示。
  
  <img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/adb3a637-d7e4-40d8-ac95-09210a699329" width="70%">

开发者可以选择修改重载个别渲染组件，或者重新继承`SceneRenderer`，替换默认的组件来重新开发整个可视化效果，包括Mesh材质，特效，阴影，光标，空气墙等，达到自定义的预期效果。

### 自定义交互样式

在`MirrorVerse SDK UI`程序包中包含的交互控件`MirrorSceneDefaultUI`预制体，在 SDK 自带的样例程序中被广泛使用。开发者在不改变主流程的情况下，可以自行修改或者自行开发符合自己应用风格的交互控件，包括嵌入应用或游戏中如何触发扫描，扫码，进入场景等操作和回调。

### 自定义交互流程

开发者还可以自定义整个交互流程。MirrorScene系统启动之后，会进入以下状态流程，包括单人Host流程，和多人的Host+Guest流程。

<img src="https://github.com/deepmirrordev/MirrorVerseUnitySDK/assets/61708920/e3dc82b2-075b-45a4-b047-7920dad51680" width="100%">

用户在创建，扫描，加入场景等操作过程中状态会随之改变，或者等待下一步操作。左图为单机或者主机的状态流程，右图为扫码加入者的状态流程。在这种情况下，只需要将`MirrorVerse SDK`程序包里面的核心组件预制体 `Packages/MirrorVerse SDK/Prefabs/MirrorScene.prefab`拖入 Unity 工程的场景中，开发者再通过`IMirrorScene`来获取系统相关信息，或者操作这些流程，比如开始扫描，下载场景，退出等等。

注：这个预制体和`MirrorSceneAll.prefab`不同，里面不包含`Renderers`和`DefaultUI`模块，也就是说可视化和交互都由开发者自身的应用和游戏提供。

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
