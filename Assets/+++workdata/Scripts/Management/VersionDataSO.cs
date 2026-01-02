using UnityEngine;

[CreateAssetMenu(fileName = "VersionData", menuName = "Tools/Version Data")]
public class VersionDataSO : ScriptableObject
{
    public float majorVersion = 1;
    public int minorVersion = 0;
}
