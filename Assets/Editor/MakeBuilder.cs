using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public class MakeBuilder
{
    public static void Make()
    {
        // 現在の設定を控える
        BuildTarget prevPlatform = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup prevGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

        // 設定値
        var outputPathKey = "-output-path";
        var outputPath = GetParameterFrom(key: outputPathKey);
        Debug.Assert(!string.IsNullOrEmpty(outputPath), $"'{outputPathKey}'の取得に失敗しましました！");

        var targetKey = "-build-target";
        var targetValue = GetParameterFrom(key: targetKey);
        Debug.Assert(!string.IsNullOrEmpty(targetValue), $"'{targetKey}'の取得に失敗しましました！");
        var buildTarget = BuildTarget.Android;
        if (targetValue.Equals("android"))
        {
            buildTarget = BuildTarget.Android;
        }
        else if (targetValue.Equals("ios"))
        {
            buildTarget = BuildTarget.iOS;
        }
        var buildOptions = BuildOptions.Development;

        // 実行
        var report = BuildPipeline.BuildPlayer(
            GetBuildScenePaths(),
            outputPath,
            buildTarget,
            buildOptions);

        // 元に戻す
        EditorUserBuildSettings.SwitchActiveBuildTarget(prevGroup, prevPlatform);

        // 結果
        if (report.summary.result == BuildResult.Succeeded)
        {
            const int kSuccessCode = 0;
            EditorApplication.Exit(kSuccessCode);
        }
        else
        {
            const int kErrorCode = 1;
            EditorApplication.Exit(kErrorCode);
        }
    }

    public static void Android()
    {
        // 現在の設定を控える
        BuildTarget prevPlatform = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup prevGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

        // 設定値
        var fileName = "uitest.apk";
        var outputPath = $"~/Downloads/{fileName}";
        var buildTarget = BuildTarget.Android;
        var buildOptions = BuildOptions.Development;

        // 実行
        var report = BuildPipeline.BuildPlayer(
            GetBuildScenePaths(),
            outputPath,
            buildTarget,
            buildOptions);

        // 元に戻す
        EditorUserBuildSettings.SwitchActiveBuildTarget(prevGroup, prevPlatform);

        // 結果
        if (report.summary.result == BuildResult.Succeeded)
        {
            const int kSuccessCode = 0;
            EditorApplication.Exit(kSuccessCode);
        }
        else
        {
            const int kErrorCode = 1;
            EditorApplication.Exit(kErrorCode);
        }
    }

    public static void iOS()
    {
        // 現在の設定を控える
        BuildTarget prevPlatform = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup prevGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

        // var outputDirKey = "-output-dir";
        // var outputDir = GetParameterFrom(key: outputDirKey);
        // 設定値
        var dirName = "uitestproject";
        var outputPath = $"~/Downloads/{dirName}";
        string[] sceneList = {"Assets/Scenes/UITestScene.unity"};
        var buildTarget = BuildTarget.iOS;
        var buildOptions = BuildOptions.Development;

        // Debug.Assert(!string.IsNullOrEmpty(outputDir), $"'{outputDirKey}'の取得に失敗しましました！");

        // 実行
        var report = BuildPipeline.BuildPlayer(
            GetBuildScenePaths(),
            outputPath,
            buildTarget,
            buildOptions);

        // 元に戻す
        EditorUserBuildSettings.SwitchActiveBuildTarget(prevGroup, prevPlatform);

        // 結果
        if (report.summary.result == BuildResult.Succeeded)
        {
            const int kSuccessCode = 0;
            EditorApplication.Exit(kSuccessCode);
        }
        else
        {
            const int kErrorCode = 1;
            EditorApplication.Exit(kErrorCode);
        }
    }

    /// <summary>
    /// 対象シーンを返す
    /// </summary>
    /// <returns>シーン名リスト</returns>
    private static string[] GetBuildScenePaths()
    {
        // "Scenes In Build"に登録されているシーンリストを取得
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        return scenes
            .Where((arg) => arg.enabled)
            .Select((arg) => arg.path)
            .ToArray();
    }

    /// <summary>
    /// コマンド引数を取得
    /// </summary>
    /// <param name="key">スイッチキー</param>
    /// <returns>値</returns>
    private static string GetParameterFrom(string key)
    {
        var args = System.Environment.GetCommandLineArgs();
        var index = args.ToList().FindIndex((arg) => arg == key);
        var paramIndex = index + 1;

        if (index < 0 || args.Count() <= paramIndex)
        {
            return null;
        }

        return args[paramIndex];
    }
}
