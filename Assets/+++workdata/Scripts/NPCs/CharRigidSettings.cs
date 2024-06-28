using MyBox;
using UnityEngine;

[System.Serializable]
public class CharRigidSettings
{
    [field: SerializeField] public float MaxSpeed { get; private set; } = 5f;
    [field: SerializeField] public float StoppingDistance { get; private set; } = 1;
    [field: SerializeField] public float Acceleration { get; private set; } = 10f;
    [field: SerializeField] public float Decceleration { get; private set; } = 10f;
    [field: SerializeField] public bool DashEnabled { get; private set; } = true;
    [field: SerializeField] public float DashForce { get; private set; } = 10f;
    [field: SerializeField] public float DashTime { get; private set; } = 0.1f;
    [field: SerializeField] public float DashCooldown { get; private set; } = 0.1f;
    [field: SerializeField] public bool DashAutomAim { get; private set; } = true;

    [Space(20)]
    [SerializeField, ShowOnly] string mouseInputHeader;
    [field: SerializeField] public bool MouseInput { get; private set; }
    [field: SerializeField] public float MaxSpeedDistance { get; private set; } = .7f;
    [field: SerializeField] public AnimationCurve MoveCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [field: SerializeField] public float MinSpeed { get; private set; } = 1f;
}