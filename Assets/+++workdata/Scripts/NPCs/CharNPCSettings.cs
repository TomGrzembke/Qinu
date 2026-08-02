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
    [field: Separator("Safe Puck Approach - routes behind the puck before striking")]
    [field: Tooltip("Distance the NPC keeps behind the puck while lining up a safe shot.")]
    [field: SerializeField] public float PukApproachDistance { get; private set; } = 4f;
    [field: Tooltip("Required alignment toward the opponent goal before the NPC may strike or dash into the puck.")]
    [field: Range(-1f, 1f)]
    [field: SerializeField] public float RequiredShotAlignment { get; private set; } = 0.7f;
    [field: Tooltip("Seconds of puck movement used to predict its approach position.")]
    [field: Min(0f)]
    [field: SerializeField] public float PukPredictionTime { get; private set; } = 0.1f;
}
