using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;



public class BuildHandler : Editor
{
    [MenuItem("Özel/Build Run Android")]
    public static void BuildSpecial()
    {
        switch (EditorUtility.DisplayDialogComplex("Confirm Build", "Are you sure that you want to build and run player?", "Build", "Cancel", "Build, Version++"))
        {
            case 0:
                break;
            case 1:
                return;
            case 2:
                int version;
                if (!int.TryParse(PlayerSettings.bundleVersion, out version))
                {
                    Debug.LogError("Version number not set correctly!");
                    return;
                }
                version++;
                PlayerSettings.bundleVersion = version.ToString();
                PlayerSettings.Android.bundleVersionCode = version;
                break;
        }

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = Application.dataPath.Replace("/Assets", "");
        options.locationPathName = options.locationPathName + "/Builds/" + PlayerSettings.productName + "_v" + PlayerSettings.bundleVersion + ".apk";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.AutoRunPlayer;
        options.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        var report = BuildPipeline.BuildPlayer(options);

    }
}
