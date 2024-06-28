using UnityEngine;

[System.Serializable]
public class CharAestheticSettings
{
    [field: SerializeField] public Color PrimaryColor { get; private set; }
    [field: SerializeField] public AudioClip Music { get; private set; }  
    
}