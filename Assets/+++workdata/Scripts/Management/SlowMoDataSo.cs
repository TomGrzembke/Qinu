using UnityEngine;

[CreateAssetMenu]
public class SlowMoDataSo : ScriptableObject
{
    [field: SerializeField] public SlowMoData SlowMoData { get; private set; } 
}