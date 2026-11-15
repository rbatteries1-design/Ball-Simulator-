using System.Linq;
using UnityEditor;
using UnityEditor.Build;
public static class BallSimulatorBuild
{
    public static void Windows()
    {
        var result = BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            "Builds/Windows/Ball Simulator 3.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new BuildFailedException(result.summary.result.ToString());
    }
}
