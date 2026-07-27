
using UnityEngine;


[CreateAssetMenu]
public class AbilityRaritiesSO : ScriptableObject
{
    [field: SerializeField] public Color[] RarityColors { get; private set; } 
    
    /// <summary> Accounts for 0 indexing</summary>
    public int MaxRarity => RarityColors.Length - 1;

    //ToDo: Add Colors for ability lost and empty slot!!
}