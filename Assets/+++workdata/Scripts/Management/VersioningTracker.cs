using TMPro;
using UnityEngine;

/// <summary>
/// Keeps track of subversions when building
/// 0.1 = First release
/// 0.11 = Revamp of Pixelartshader
/// 0.12 = ongoing movement and balancing revamp
/// 0.13 = (hopefully) Amaze Version
/// minorVersions go from 000 to 999 and correspond to the build number for debugging
/// </summary>
public class VersioningTracker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI versionText;

    void Start()
    {
        versionText.text = $"v{Application.version}";
    }
}