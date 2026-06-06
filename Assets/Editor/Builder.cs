using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Agent-callable batchmode build entrypoint for Pully.
// Run headless:
//   Unity -quit -batchmode -projectPath . -executeMethod Builder.BuildAndroid -logFile build.log
// Exits non-zero on failure so the agent's run loop can detect it.
public static class Builder
{
    private const string OutputPath = "Builds/Android/pully.apk";

    [MenuItem("Pully/Build Android (Debug)")]
    public static void BuildAndroid()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[Builder] No enabled scenes in Build Settings. Add the menu + game scenes.");
            EditorApplication.Exit(2);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Builder] BUILD OK: {summary.totalSize} bytes -> {OutputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[Builder] BUILD FAILED: result={summary.result}, errors={summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }
}
