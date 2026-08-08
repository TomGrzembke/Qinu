using MyBox;
using UnityEngine;

[System.Serializable]
public class DashSettings
{
    [field: Separator("Dash")]
    [field: SerializeField] public bool Enabled { get; private set; } = true;
    [field: Tooltip("Total dash strength, equivalent to the previous impulse value. It is distributed across the dash duration to allow steering.")]
    [field: SerializeField] public float Force { get; private set; } = 10f;
    [field: Min(0f)]
    [field: SerializeField] public float Duration { get; private set; } = 0.1f;
    [field: Min(0f)]
    [field: SerializeField] public float Cooldown { get; private set; } = 0.1f;
    [field: Tooltip("Cumulative percentage of the total dash velocity applied over the dash. X is normalized dash time and Y is the applied percentage. Keep the final value at one to preserve the configured total force.")]
    [field: SerializeField] public AnimationCurve VelocityApplication { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [field: Tooltip("When enabled, the default player dash targets the puck. When disabled, it follows the mouse throughout the dash.")]
    [field: SerializeField] public bool AutoAim { get; private set; } = true;
    [field: Separator("Target Tracking - percentage of the dash that corrects toward a moving target")]
    [field: Tooltip("Zero locks the target's initial position. One tracks it for the entire dash. Fixed-position dashes ignore this value.")]
    [field: Range(0f, 1f)]
    [field: SerializeField] public float TargetTrackingPercentage { get; private set; }
}
