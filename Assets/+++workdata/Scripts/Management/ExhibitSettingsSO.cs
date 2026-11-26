using UnityEngine;

[CreateAssetMenu]
public class ExhibitSettingsSO : ScriptableObject
{
    [field: SerializeField] public bool ExhibitMode { get; private set; }

    [field: SerializeField, Min(0f)] public float TimeUntilWarning { get; private set; } = 60f;
    [field: SerializeField, Min(0f)] public float TimeUntilReset { get; private set; } = 75f;

    void OnValidate()
    {
        TimeUntilWarning = Mathf.Max(0f, TimeUntilWarning);
        TimeUntilWarning = Mathf.Max(TimeUntilWarning, TimeUntilReset);
    }

    public void SetExhibitMode(bool newState)
    {
        ExhibitMode = newState;
    }
}
