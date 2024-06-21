using UnityEngine;

[CreateAssetMenu]
public class CharSO : ScriptableObject
{
    [field: SerializeField] public CharSettings CharSettings { get; private set; } 
    [field: SerializeField] public GameObject VisualPrefab { get; private set; }
    [field: SerializeField] public CharAestheticSettings charAestheticSettings { get; private set; } 
}