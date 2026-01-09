#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildVersionProcessor : IPostprocessBuildWithReport
{
    // This ensures it runs before the build
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        // Find the VersionData file
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(VersionDataSO)}");
        if (guids.Length == 0)
        {
            Debug.Log(nameof(BuildVersionProcessor) + " Didnt find VersionDataSO");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        VersionDataSO data = AssetDatabase.LoadAssetAtPath<VersionDataSO>(path);

        data.minorVersion++;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        //:D3 Refers to Layout setting (000) for ex.
        PlayerSettings.bundleVersion = $"{data.majorVersion}.{data.minorVersion:D3}";

        Debug.Log($"Build Version Updated to: {PlayerSettings.bundleVersion}");
    }
}

#endif