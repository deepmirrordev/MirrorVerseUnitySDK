#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MirrorVerse.Data.Editor
{
    /// <summary>
    /// Editor menu (visible in consumer applications) that copies MNN models and edge
    /// map zips bundled inside the MirrorVerse.Data package's Data~ folder into the
    /// consumer's Assets/StreamingAssets/Data/ directory. The runtime side
    /// (MnnModelManager, EdgeMapManager) reads from StreamingAssets/Data/ -- this menu
    /// stages the bundled assets there so the SDK works out of the box.
    ///
    /// Mirrors Kappa's internal "Copy MNN Models" / "Copy Edge Maps" tools, which copy
    /// from Kappa/Data/ into Kappa/Assets/StreamingAssets/. Here the source is the
    /// Data~ folder shipped inside the SDK data package; the trailing tilde keeps the
    /// bundled binaries hidden from Unity's importer until this menu is run.
    /// </summary>
    public static class BundledDataCopyTool
    {
        // Destination paths under StreamingAssets. Must stay in lockstep with
        // MnnModelManager.PATH_PREFIX and EdgeMapManager.PATH_PREFIX -- the runtime
        // managers read from `Application.streamingAssetsPath/<PATH_PREFIX>/...`.
        private const string ModelsStreamingPath = "Data/Models";
        private const string EdgeMapsStreamingPath = "Data/EdgeMaps";

        // Source paths inside the SDK data package, relative to PackageInfo.resolvedPath.
        // The Data~ wrapper hides the bundled binaries from Unity's asset importer in
        // consumer projects until this menu stages them into StreamingAssets.
        private const string ModelsBundledPath = "Data~/Models";
        private const string EdgeMapsBundledPath = "Data~/EdgeMaps";

        // Priorities place these below the Kappa-only "Copy MNN Models" / "Copy Edge Maps"
        // (50/51) and grouped with "Package Tarball" (default ~1000) under the same
        // menu separator, so the Bundled/SDK-shipping tools cluster visually.
        [MenuItem("MirrorVerse/Copy Bundled MNN Models", priority = 990)]
        public static void CopyBundledMnnModels()
        {
            CopyBundledData(ModelsBundledPath, ModelsStreamingPath, "MNN model");
        }

        [MenuItem("MirrorVerse/Copy Bundled Edge Maps", priority = 991)]
        public static void CopyBundledEdgeMaps()
        {
            CopyBundledData(EdgeMapsBundledPath, EdgeMapsStreamingPath, "edge map");
        }

        private static void CopyBundledData(string bundledPath, string streamingPath, string label)
        {
            if (!TryResolveDataPackageRoot(out string packageRoot))
            {
                Debug.LogError(
                    "Could not locate the MirrorVerse.Data package on disk. Make sure the " +
                    "com.deepmirror.mirrorverse.data package is installed via Package Manager " +
                    "(git URL or tarball), or that an Assets/MirrorVerse.Data folder exists in " +
                    "this project.");
                return;
            }

            string sourcePath = Path.Combine(packageRoot, bundledPath);
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogWarning(
                    $"No bundled {label} data found at '{sourcePath}'. The installed " +
                    "MirrorVerse.Data package may not include this data. Contact the SDK " +
                    "provider for an updated package.");
                return;
            }

            string targetPath = Path.Combine(Application.streamingAssetsPath, streamingPath);
            Directory.CreateDirectory(targetPath);
            FileUtil.ReplaceDirectory(sourcePath, targetPath);

            AssetDatabase.Refresh();
            Debug.Log($"Bundled {label} data copied from '{sourcePath}' to '{targetPath}'.");
        }

        // Resolves the on-disk root of the MirrorVerse.Data package. Two layouts are supported:
        //   1. Installed via Package Manager (git URL or tarball) -- PackageInfo.resolvedPath
        //      points at Library/PackageCache/<name>@<hash>/.
        //   2. Embedded directly under Assets/MirrorVerse.Data -- the SDK project's own layout,
        //      where MirrorVerse.Data is a regular folder rather than a registered package.
        private static bool TryResolveDataPackageRoot(out string packageRoot)
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BundledDataCopyTool).Assembly);
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                packageRoot = packageInfo.resolvedPath;
                return true;
            }

            string embeddedPath = Path.Combine(Application.dataPath, "MirrorVerse.Data");
            if (Directory.Exists(embeddedPath))
            {
                packageRoot = embeddedPath;
                return true;
            }

            packageRoot = null;
            return false;
        }
    }
}

#endif
