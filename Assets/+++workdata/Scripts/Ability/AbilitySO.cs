using UnityEngine;

[CreateAssetMenu]
public class AbilitySO : ScriptableObject
{
    public Sprite abilitySprite;
    public string abilityTitel;
    [TextArea] public string abilityDescription;
}
