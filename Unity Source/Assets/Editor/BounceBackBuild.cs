using System;
using System.Collections.Generic;
using System.IO;
using BounceBack;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BounceBack.Editor
{
    public static class BounceBackBuild
    {
        [MenuItem("Bounce Back/Build/Windows 64-bit")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/BounceBack.exe");
        }

        [MenuItem("Bounce Back/Build/WebGL")]
        public static void BuildWebGL()
        {
            Build(BuildTarget.WebGL, "Builds/WebGL");
        }

        private static void Build(BuildTarget target, string relativeOutputPath)
        {
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                throw new InvalidOperationException(
                    "The " + target + " build module is not installed for this Unity Editor.");
            }

            string demoScene = BounceBackSceneBuilder.EnsureDemoSceneAsset();
            AddSceneToBuildSettings(demoScene);
            AssetDatabase.SaveAssets();

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, relativeOutputPath));
            string outputDirectory = target == BuildTarget.WebGL
                ? outputPath
                : Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDirectory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { demoScene },
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    target + " build ended with status " + report.summary.result + ".");
            }

            Debug.Log(
                target + " build complete: " + outputPath +
                " (" + report.summary.totalSize + " bytes)");
        }

        private static void AddSceneToBuildSettings(string demoScene)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(demoScene, true),
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && scene.path != demoScene)
                {
                    scenes.Add(scene);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
