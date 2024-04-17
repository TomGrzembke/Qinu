using UnityEngine;

[System.Serializable]
public class CharNPCSettings
{
    [field: SerializeField] public bool GoesToDefault { get; private set; } = true;
    [field: SerializeField] public bool FollowBallY { get; private set; } = true;
    [field: SerializeField] public bool InvertY { get; private set; }

    [field: SerializeField] public bool DashRandomly { get; private set; } = true;
    [field: SerializeField] public float ProbabilityPerFrame { get; private set; } = 0.001f;
}