using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BlockDiggerz.Build
{
    /// <summary>
    /// Release Web build for itch.io: Gzip compression with decompression fallback (itch serves files without
    /// Content-Encoding headers, so the loader decompresses them itself), the enabled scenes from Build Settings, and a
    /// zip with index.html at its root, ready to upload as an HTML game. The compression settings are set only for the
    /// build and put back afterwards. Menu: Tools > BlockDiggerz > Build > Web (itch.io). CLI with the Editor closed:
    /// <c>unity build . --target WebGL --execute-method BlockDiggerz.Build.WebBuild.BuildForItchFromCommandLine</c>.
    /// With the Editor open: call <see cref="BuildForItch"/> from a detached <c>script eval</c> job; the outcome is also
    /// written to <see cref="ResultPath"/>.
    /// </summary>
    public static class WebBuild
    {
        /// <summary>Menu path of the itch.io build.</summary>
        public const string MenuPath = "Tools/BlockDiggerz/Build/Web (itch.io)";

        /// <summary>Folder (project relative) the player is built into.</summary>
        public const string OutputFolder = "Builds/Web/BlockDiggerz";

        /// <summary>Zip (project relative) to upload to itch.io.</summary>
        public const string ZipPath = "Builds/BlockDiggerz-web-itch.zip";

        /// <summary>
        /// Text file (project relative) with the outcome of the last build: first line "succeeded" or "failed",
        /// then details. Lets the CLI wait for a build started from the menu.
        /// </summary>
        public const string ResultPath = "Builds/web-build-result.txt";

        private const string Succeeded = "succeeded";
        private const string Failed = "failed";
        private const int FailureExitCode = 1;
        private const double BytesPerMegabyte = 1024d * 1024d;

        /// <summary>Builds and zips from the menu, one Editor update later so the menu closes first.</summary>
        [MenuItem(MenuPath)]
        public static void BuildForItchFromMenu()
        {
            DeleteResult();
            EditorApplication.delayCall += () => BuildForItch();
        }

        /// <summary>Batch mode entry point: builds and zips, exits with 1 on failure.</summary>
        public static void BuildForItchFromCommandLine()
        {
            if (!BuildForItch())
            {
                EditorApplication.Exit(FailureExitCode);
            }
        }

        /// <summary>Builds the Web player for itch.io and zips it. Returns true on success.</summary>
        public static bool BuildForItch()
        {
            WebGLCompressionFormat savedFormat = PlayerSettings.WebGL.compressionFormat;
            bool savedFallback = PlayerSettings.WebGL.decompressionFallback;
            try
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;

                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string output = Path.Combine(projectRoot, OutputFolder);
                string zip = Path.Combine(projectRoot, ZipPath);
                if (Directory.Exists(output))
                {
                    Directory.Delete(output, true);
                }

                var options = new BuildPlayerOptions
                {
                    scenes = EnabledScenes(),
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    targetGroup = BuildTargetGroup.WebGL,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    string message = $"Build {report.summary.result} with {report.summary.totalErrors} errors.";
                    WriteResult(projectRoot, Failed, message);
                    Debug.LogError("[WebBuild] " + message);
                    return false;
                }

                if (File.Exists(zip))
                {
                    File.Delete(zip);
                }

                ZipFile.CreateFromDirectory(output, zip, System.IO.Compression.CompressionLevel.Optimal, false);
                double megabytes = new FileInfo(zip).Length / BytesPerMegabyte;
                string details = $"{zip}\n{megabytes:0.0} MB zip, scenes: {string.Join(", ", options.scenes)}";
                WriteResult(projectRoot, Succeeded, details);
                Debug.Log("[WebBuild] Built " + details);
                return true;
            }
            catch (Exception exception)
            {
                WriteResult(Directory.GetParent(Application.dataPath).FullName, Failed, exception.ToString());
                Debug.LogException(exception);
                return false;
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat = savedFormat;
                PlayerSettings.WebGL.decompressionFallback = savedFallback;
                AssetDatabase.SaveAssets();
            }
        }

        private static string[] EnabledScenes()
        {
            var scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }

        private static void DeleteResult()
        {
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ResultPath);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void WriteResult(string projectRoot, string outcome, string details)
        {
            string path = Path.Combine(projectRoot, ResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, outcome + "\n" + details + "\n");
        }
    }
}
