using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuilder
{
    public static void Build()
    {
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "WebGL"));
        Directory.CreateDirectory(output);

        PlayerSettings.companyName = "DaoStudio";
        PlayerSettings.productName = "Dao Endless Ascension";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.WebGL, "com.daostudio.daoascension");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        Debug.Log("[WebGL Build] 开始构建到：" + output);
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[WebGL Build] 成功！大小=" + (summary.totalSize / 1024 / 1024) + "MB，耗时=" + summary.totalTime);
        }
        else
        {
            Debug.LogError("[WebGL Build] 失败：" + summary.result + " 错误数=" + summary.totalErrors);
        }
    }
}
