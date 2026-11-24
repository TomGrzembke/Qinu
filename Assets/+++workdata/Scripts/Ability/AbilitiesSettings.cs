using UnityEngine;

[CreateAssetMenu]
public class AbilitiesSettings : ScriptableObject
{
    [field: SerializeField] public AnimationCurve CooldownCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);
}