using UnityEngine;
using MyBox;

[System.Serializable]
public class CharNPCSettings
{
    [field: SerializeField] public bool GoesToDefault { get; private set; } = true;
    [field: SerializeField] public float DefaultSwitchTime { get; private set; } 
    [field: SerializeField] public bool FollowBallY { get; private set; } = true;
    [field: SerializeField] public bool InvertY { get; private set; }
    [field: SerializeField] public bool DashRandomly { get; private set; } = true;
    [field: SerializeField] public float ProbabilityPerFrame { get; private set; } = 0.001f;
    [field: Separator("Approach Distance - higher lines up farther behind the puck")]
    [field: Tooltip("Distance behind the predicted puck position used for approach candidates. Lower values produce shorter, more direct approaches; higher values create wider and safer run-ups.")]
    [field: SerializeField] public float PukApproachDistance { get; private set; } = 4f;
    [field: Separator("Shot Alignment - higher requires a straighter shot")]
    [field: Tooltip("Minimum dot-product alignment toward the opponent goal before the NPC may chase or dash into the puck. -1 allows any angle, 0 allows a 90-degree approach, and 1 requires perfect alignment.")]
    [field: Range(-1f, 1f)]
    [field: SerializeField] public float RequiredShotAlignment { get; private set; } = 0.7f;
    [field: Separator("Puck Prediction - higher aims farther ahead")]
    [field: Tooltip("Seconds of current puck velocity added to its position when planning. Zero targets its current position; higher values lead fast-moving pucks farther ahead.")]
    [field: Min(0f)]
    [field: SerializeField] public float PukPredictionTime { get; private set; } = 0.1f;
    [field: Separator("Goal Danger Distance - defense overrides attacking inside this range")]
    [field: Tooltip("Distance from the own goal within which a puck moving toward it is treated as a defensive threat.")]
    [field: Min(0f)]
    [field: SerializeField] public float OwnGoalDangerDistance { get; private set; } = 15f;
    [field: Separator("Threat Speed - lower reacts to slower incoming pucks")]
    [field: Tooltip("Minimum puck speed required to trigger a defensive clear before it enters the emergency distance.")]
    [field: Min(0f)]
    [field: SerializeField] public float MinimumThreatSpeed { get; private set; } = 2f;
    [field: Separator("Threat Alignment - higher requires movement more directly at the goal")]
    [field: Tooltip("Minimum dot-product alignment between puck velocity and the direction toward the own goal. -1 accepts any direction, 0 accepts sideways movement, and 1 requires movement directly at the goal.")]
    [field: Range(-1f, 1f)]
    [field: SerializeField] public float OwnGoalThreatAlignment { get; private set; } = 0.6f;
    [field: Separator("Emergency Distance - backdash behind the puck before clearing")]
    [field: Tooltip("Inside this distance from the middle of the own goal, the NPC first dashes to a clear goal-side setup position without touching the puck, then approaches normally to clear it out of the goal.")]
    [field: Min(0f)]
    [field: SerializeField] public float EmergencyGoalDistance { get; private set; } = 5f;
    [field: Separator("Emergency Alignment - higher moves farther behind the puck before clearing")]
    [field: Tooltip("Minimum alignment required before an emergency backdash changes into a normal outward clear. Higher values reduce risky side contact; lower values start the clear sooner.")]
    [field: Range(-1f, 1f)]
    [field: SerializeField] public float EmergencyClearAlignment { get; private set; } = 0.75f;
    [field: Separator("Emergency Clearance Margin - higher routes farther around the puck")]
    [field: Tooltip("Additional safety margin added to the character and puck collider radii when routing behind the puck. Higher values avoid early contact more reliably but require more free space.")]
    [field: Min(0f)]
    [field: SerializeField] public float EmergencyDashPukClearance { get; private set; } = 1.25f;
    [field: Separator("Defensive Dash - enabled backdashes during emergencies")]
    [field: Tooltip("Allows deterministic emergency backdashes. Normal attacking dashes continue to use Dash Randomly and Probability Per Frame.")]
    [field: SerializeField] public bool DashDefensively { get; private set; } = true;
}
