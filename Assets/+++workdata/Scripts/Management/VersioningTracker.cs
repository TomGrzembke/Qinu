using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

/// <summary>
/// Keeps track of subversions when building
/// 0.1 = First release
/// 0.11 = Revamp of Pixelartshader
/// 0.12 = ongoing movement and balancing revamp
/// 0.13 = (hopefully) Amaze Version
/// minorVersions go from 000 to 999 and correspond to the build number for debugging
/// </summary>
public class VersioningTracker : MonoBehaviour, IPreprocessBuildWithReport
{
    [SerializeField] float majorVersion = 0.12f;
    [SerializeField] TextMeshProUGUI versionText;


    public int callbackOrder { get; }

    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_EDITOR
        int currentMinorVersion = PlayerPrefs.GetInt("MinorVersion", 0) + 1;
        PlayerPrefs.SetInt("MinorVersion", currentMinorVersion);

        versionText.text = "v: " + majorVersion + GetMinorVersionString(currentMinorVersion);
        
        EditorUtility.SetDirty(this);
        EditorSceneManager.SaveScene(gameObject.scene);
#endif
    }

    /// <summary> at least three digits, use zeros if empty.</summary>
    string GetMinorVersionString(int minorVersion) => minorVersion.ToString("000");
}