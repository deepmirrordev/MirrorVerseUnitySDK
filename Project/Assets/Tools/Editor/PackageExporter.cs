using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MirrorVerse.EditorTools
{
    public static class PackageExporter
    {
        [MenuItem("MirrorVerse/Package Tarball")]
        public static void Pack()
        {
            string tempUPMPath = $"{Application.dataPath}/../Temp/ExportPackages";
            Client.Pack($"{Application.dataPath}/MirrorVerse", tempUPMPath);
            Client.Pack($"{Application.dataPath}/MirrorVerse.UI", tempUPMPath);
        }
    }
}
