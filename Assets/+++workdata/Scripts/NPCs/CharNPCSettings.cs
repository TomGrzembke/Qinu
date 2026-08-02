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
}
