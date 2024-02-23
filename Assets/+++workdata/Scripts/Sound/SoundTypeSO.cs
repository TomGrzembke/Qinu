using System;
using UnityEngine;

[CreateAssetMenu]
public class SoundTypeSO : ScriptableObject
{
    public SoundType soundType;
    public AudioClip[] clips;

    void OnValidate()
    {
        Enum.TryParse<SoundType>(name, out var result);
        if (result != SoundType.Null)
            soundType = result;
    }
    
}
