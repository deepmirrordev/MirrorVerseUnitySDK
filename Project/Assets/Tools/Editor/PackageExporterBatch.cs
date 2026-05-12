// Batchmode-friendly entry point for `MirrorVerse → Package Tarball`.
// Client.Pack is async (returns a request; completion is driven by the
// editor's update loop), so the existing PackageExporter.Pack would race
// with -quit. PackAll fires the three Pack requests and polls each one
// until it completes, then exits with a status code reflecting success.
//
// Usage:
//   Unity.exe -batchmode -nographics -quit -projectPath <project> \
//             -executeMethod MirrorVerse.EditorTools.PackageExporterBatch.PackAll \
//             -logFile <path>

using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MirrorVerse.EditorTools
{
    public static class PackageExporterBatch
    {
        public static void PackAll()
        {
            // Don't write to Temp/ -- Unity wipes it on -quit shutdown, so the
            // tarballs would vanish before the calling shell can pick them up.
            // Build/ is just an arbitrary repo-level folder Unity won't touch.
            string outDir = $"{Application.dataPath}/../Build/Tarballs";
            Directory.CreateDirectory(outDir);

            string[] candidates = {
                $"{Application.dataPath}/MirrorVerse",
                $"{Application.dataPath}/MirrorVerse.Data",
                $"{Application.dataPath}/MirrorVerse.UI",
            };

            // Skip missing sources so this script also works on older revisions
            // (v0.3.0 predates the MirrorVerse.Data split).
            var present = new System.Collections.Generic.List<string>();
            foreach (string s in candidates)
            {
                if (Directory.Exists(s)) present.Add(s);
                else Debug.LogWarning($"[PackageExporterBatch] Skipping (not present): {s}");
            }
            string[] sources = present.ToArray();

            var requests = new PackRequest[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                Debug.Log($"[PackageExporterBatch] Packing {sources[i]} -> {outDir}");
                requests[i] = Client.Pack(sources[i], outDir);
            }

            int failures = 0;
            for (int i = 0; i < requests.Length; i++)
            {
                while (!requests[i].IsCompleted)
                {
                    Thread.Sleep(100);
                }
                // StatusCode in Unity 2022 has only InProgress + Failure;
                // success is "completed without error".
                if (requests[i].Error == null && requests[i].Result != null)
                {
                    Debug.Log($"[PackageExporterBatch] OK: {requests[i].Result.tarballPath}");
                }
                else
                {
                    Debug.LogError($"[PackageExporterBatch] FAILED ({sources[i]}): " +
                                   $"{requests[i].Error?.message}");
                    failures++;
                }
            }

            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
