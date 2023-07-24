#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace MirrorVerse.Editor
{
    public static class XcodeProjectBuildPostprocess
    {
        [PostProcessBuild(9999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
#if UNITY_IOS
            if (buildTarget == BuildTarget.iOS)
            {
                // Original vlcframework.a library file is actually a zipped version. 
                // Extract it first and delete original zip.
                string librariesPath = Path.Combine(new string[] { path, "Libraries" });
                string[] foundFiles = Directory.GetFiles(librariesPath, "vlcframework.a", SearchOption.AllDirectories);
                if (foundFiles.Length == 0)
                {
                    Debug.LogError("The core iOS library vlcframework.a is not found. The xcode project may not compile.");
                    return;
                }
                else if (foundFiles.Length > 1)
                {
                    Debug.LogWarning("The core iOS library vlcframework.a has more than one copy. Use the first occurance.");
                }
                string originalFile = foundFiles[0];
                string pluginPath = originalFile[..originalFile.LastIndexOf("vlcframework.a")];
                string tmpPath = Path.Combine(pluginPath, "tmp");
                string tmpFile = Path.Combine(pluginPath, "tmp", "vlcframework.a");
                Directory.CreateDirectory(tmpPath);
                ZipFile.ExtractToDirectory(originalFile, tmpPath, true);
                File.Delete(originalFile);
                File.Move(tmpFile, originalFile);
                Directory.Delete(tmpPath);

                // Re-order vlcframework.a dependency to top in order to pass the xcode framework build.
                string projectPath = PBXProject.GetPBXProjectPath(path);
                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);
                string targetGuid = pbxProject.GetUnityFrameworkTargetGuid();
                string pbxString = pbxProject.WriteToString();
                // Move "vlcframework.a" to top of the files list of "Frameworks" PBXFrameworksBuildPhase build phase.
                string frameworksGuid = pbxProject.GetFrameworksBuildPhaseByTarget(targetGuid);
                string frameworksString = frameworksGuid + " /* Frameworks */ = {";
                int frameworksIndex = pbxString.IndexOf(frameworksString);
                int listStartIndex = pbxString.IndexOf("files = (", frameworksIndex) + 9;
                int listEndIndex = pbxString.IndexOf(");", listStartIndex);
                string originalString = pbxString.Substring(listStartIndex, listEndIndex - listStartIndex);
                List<string> lines = new(originalString.Split(","));
                void moveToTop(string libName)
                {
                    int i = lines.FindIndex(s => s.IndexOf(libName) != -1);
                    if (i != -1)
                    {
                        string l = lines[i];
                        lines.RemoveAt(i);
                        lines.Insert(0, l);
                    }
                }
                moveToTop("vlcframework.a");
                string replacementString = string.Join(",", lines);
                pbxString = pbxString.Substring(0, listStartIndex) + replacementString + pbxString.Substring(listEndIndex);

                pbxProject.ReadFromString(pbxString);
                pbxProject.WriteToFile(projectPath);
                Debug.Log("Postprocess finished.");
            }
#endif
        }
    }
}
#endif
