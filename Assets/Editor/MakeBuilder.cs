/*
 * MakeBuilder.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public class MakeBuilder
{
    /// <summary>
    /// 作成
    /// </summary>
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
        var buildTarget = BuildTarget.NoTarget;
        if (targetValue.Equals("android"))
        {
            buildTarget = BuildTarget.Android;
        }
        else if (targetValue.Equals("ios"))
        {
            buildTarget = BuildTarget.iOS;
        }
        Debug.Assert(!buildTarget.Equals(BuildTarget.NoTarget), $"'{targetValue}'はサポート外です！");

        var buildOptions = BuildOptions.ShowBuiltPlayer;
        var variantKey = "-development";
        if (IsParameterExist(variantKey))
        {
            buildOptions |= BuildOptions.Development;
        }
        if (buildTarget.Equals(BuildTarget.iOS))
        {
            buildOptions |= (BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.SymlinkLibraries | BuildOptions.ConnectToHost);
        }

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
    /// 引数の存在を確認
    /// </summary>
    /// <param name="key">スイッチキー</param>
    /// <returns>存在するならtrue</returns>
    private static bool IsParameterExist(string key)
    {
        foreach (var command in System.Environment.GetCommandLineArgs())
        {
            if (key.Equals(command))
            {
                return true;
            }
        }
        return false;
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
