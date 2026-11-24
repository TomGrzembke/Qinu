using UnityEngine;

[CreateAssetMenu]
public class AbilitySO : ScriptableObject
{
    public Sprite abilitySprite;
    public string abilityTitel;
    [TextArea] public string abilityDescription;
    [field: SerializeField] public AbilitiesSettings AbilitiesSettings { get; private set; } 

}
