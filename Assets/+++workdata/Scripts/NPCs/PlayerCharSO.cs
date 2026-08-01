using UnityEngine;

[CreateAssetMenu(menuName = "Characters/Player Character")]
public class PlayerCharSO : CharSO
{
    [field: SerializeField] public PlayerCharSettings CharSettings { get; private set; }

    public override float MaxSpeed => CharSettings.CharRigidSettings.MaxSpeed;
    public override float StoppingDistance => CharSettings.CharRigidSettings.StoppingDistance;
}
