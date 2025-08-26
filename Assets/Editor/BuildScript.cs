using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    // All enabled scenes from Build Settings
    private static string[] EnabledScenes() =>
        EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

    // Resolve platform-specific output path (folder/file as required)
    private static string ResolveOutputPath(BuildTarget target, string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            productName = "Build";

        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                {
                    // Windows requires a FILE path (.exe)
                    var outDir = "build-folder/Windows/output";
                    Directory.CreateDirectory(outDir);
                    return Path.Combine(outDir, productName + ".exe");
                }

            case BuildTarget.WebGL:
                {
                    // WebGL requires a folder path
                    var outDir = "build-folder/WebGL/output";
                    Directory.CreateDirectory(outDir);
                    return outDir;
                }

            case BuildTarget.Android:
                {
                    // Choose APK (or switch to AAB by toggling this flag + extension)
                    var outDir = "build-folder/Android/output";
                    Directory.CreateDirectory(outDir);
                    EditorUserBuildSettings.buildAppBundle = false; // true → AAB, false → APK
                    var ext = EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                    return Path.Combine(outDir, productName + ext);
                }

            case BuildTarget.StandaloneOSX:
                {
                    // macOS builds to a .app bundle path
                    var outDir = "build-folder/macOS/output";
                    Directory.CreateDirectory(outDir);
                    return Path.Combine(outDir, productName + ".app");
                }

            case BuildTarget.StandaloneLinux64:
                {
                    // Linux is a file path (no extension required)
                    var outDir = "build-folder/Linux/output";
                    Directory.CreateDirectory(outDir);
                    return Path.Combine(outDir, productName);
                }

            default:
                {
                    // Fallback: Windows x64 exe
                    var outDir = "build-folder/Windows/output";
                    Directory.CreateDirectory(outDir);
                    return Path.Combine(outDir, productName + ".exe");
                }
        }
    }

    // Build the project's CURRENT active platform (whatever the project is set to)
    public static void PerformCurrentPlatformBuild()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);

        var scenes = EnabledScenes();
        var product = PlayerSettings.productName;
        var location = ResolveOutputPath(target, product);

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = location,
            target = target,
            targetGroup = group,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception(
                $"Build failed: {report.summary.result} " +
                $"(Errors: {report.summary.totalErrors}, Warnings: {report.summary.totalWarnings})");
        }

       
    }
}
