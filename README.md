# MirrorVerse Unity SDK

[English](#english) | [中文](#中文)

<a id="english"></a>

## English

Welcome to the MirrorVerse Developer Platform. The SDK lets AR apps and AR games blend virtual and physical reality with realistic, shareable experiences.

### Packages

The SDK ships three Unity packages:

| Package | Required | Purpose |
|---|---|---|
| [`com.deepmirror.mirrorverse`](Project/Assets/MirrorVerse/) | yes | Core API. Runtime support for MirrorScene, MirrorSpace, image tracking, and the AR Foundation adapter. |
| [`com.deepmirror.mirrorverse.ui`](Project/Assets/MirrorVerse.UI/) | recommended | Reference UI components (DefaultUI, ClassyUI) for the most common flows. |
| [`com.deepmirror.mirrorverse.data`](Project/Assets/MirrorVerse.Data/) | optional | Bundled MNN models and edge maps. Install if your app needs the bundled data staged into StreamingAssets; skip if your app downloads localization data from the server. |

### MirrorScene API

The `MirrorScene` API provides AR functionality suitable for small-scale scenes — rooms, table tops, offices. Combined with the MirrorScene UI components, you can create a shared scene with one or more users in real time, make virtual worlds in games or apps interact with surrounding physical objects, and build AR experiences limited only by imagination.

- Start with the [Get Started](Documents/GetStarted_en.md) guide.
- See the example projects in [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples) for full reference apps.
- Visit the [MirrorScene SDK home page](https://mirrorscene.deepmirror.com) for video demos and more.

### Releases

Latest tarballs and release notes are at the [SDK Releases](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) page. Each release attaches:

- `com.deepmirror.mirrorverse-X.Y.Z.tgz` — core SDK
- `com.deepmirror.mirrorverse.ui-X.Y.Z.tgz` — reference UI
- `com.unity.xr.arengine-X.Y.Z.tgz` — Huawei AREngine XR plugin (for HarmonyOS / Honor / Huawei devices; versioned independently from the SDK)

If you need the optional `com.deepmirror.mirrorverse.data` package, please reach out via the support contact below.

### Support

For early-access requests, API keys, or support, visit [mirrorscene.deepmirror.com](https://mirrorscene.deepmirror.com).

---

<a id="中文"></a>

## 中文

欢迎来到 `MirrorVerse` 开发者平台。本 SDK 为 AR 应用和 AR 游戏提供更贴近真实的空间共享工具，帮助应用与游戏打造更好的体验。

### 包说明

SDK 包含三个 Unity 包：

| 包 | 是否必需 | 用途 |
|---|---|---|
| [`com.deepmirror.mirrorverse`](Project/Assets/MirrorVerse/) | 必需 | 核心 API。提供 MirrorScene、MirrorSpace、图像跟踪以及 AR Foundation 适配器的运行时支持。 |
| [`com.deepmirror.mirrorverse.ui`](Project/Assets/MirrorVerse.UI/) | 推荐 | 参考 UI 组件（DefaultUI、ClassyUI），覆盖常见流程。 |
| [`com.deepmirror.mirrorverse.data`](Project/Assets/MirrorVerse.Data/) | 可选 | 内置 MNN 模型与边缘地图。如需将内置数据部署到 StreamingAssets 则安装此包；若应用从服务端下载定位数据可跳过。 |

### MirrorScene API

`MirrorScene` API 是 MirrorVerse SDK 套件中提供的一个功能，适用于任意小型场景，比如房间、桌面、办公室等。开发者可以通过接入 MirrorScene API 以及附带的 MirrorScene UI 组件，让应用或游戏快速将一个或多个用户所在的真实空间与真实物体，与应用或游戏内的虚拟世界联动，给用户带来创造性的实时虚实交互体验。

- 请阅读[开始使用](Documents/GetStarted_zh.md)文档，开始使用 MirrorScene API。
- 请查阅样例仓库 [MirrorSceneExamples](https://github.com/deepmirrordev/MirrorSceneExamples)，获取完整的调用参考。
- 更多内容（包括视频范例），请前往 [MirrorScene](https://mirrorscene.deepmirror.com) 主页。

### 版本发布

最新 tarball 与发布说明请见 [SDK Releases](https://github.com/deepmirrordev/MirrorVerseUnitySDK/releases) 页面。每个版本附带：

- `com.deepmirror.mirrorverse-X.Y.Z.tgz` — 核心 SDK
- `com.deepmirror.mirrorverse.ui-X.Y.Z.tgz` — 参考 UI
- `com.unity.xr.arengine-X.Y.Z.tgz` — 华为 AREngine XR 插件（HarmonyOS / 荣耀 / 华为设备；版本号独立于 SDK）

如需可选的 `com.deepmirror.mirrorverse.data` 包，请通过下方支持渠道联系我们。

### 支持

如需申请早期访问、API 密钥或技术支持，请访问 [mirrorscene.deepmirror.com](https://mirrorscene.deepmirror.com)。
