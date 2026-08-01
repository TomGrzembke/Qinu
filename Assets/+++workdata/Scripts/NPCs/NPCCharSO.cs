using UnityEngine;

[CreateAssetMenu(menuName = "Characters/NPC Character")]
public class NPCCharSO : CharSO
{
    [field: SerializeField] public NPCCharSettings CharSettings { get; private set; }

    public override float MaxSpeed => CharSettings.CharRigidSettings.MaxSpeed;
    public override float StoppingDistance => CharSettings.CharRigidSettings.StoppingDistance;
}
