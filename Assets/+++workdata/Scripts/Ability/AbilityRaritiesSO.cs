using System.Collections.Generic;
using MyBox;
using UnityEngine;


[CreateAssetMenu]
public class AbilityRaritiesSO : ScriptableObject
{
    [field: SerializeField] public Color[] RarityColors { get; private set; }
    [field: Header("Reward Chances")]
    [field: Tooltip("Relative chance for each offered rarity: index 0 is grey, 1 blue, 2 purple, and 3 pink. Values are weights and do not need to total 100.")]
    [field: SerializeField] public float[] RewardRarityWeights { get; private set; } = new float[] { 55f, 30f, 12f, 3f };
    [SerializeField, ReadOnly, Tooltip("Calculated percentage for each reward rarity value, using the same indices as Reward Rarity Weights.")]
    float[] rewardRarityPercentages;

    public IReadOnlyList<float> RewardRarityPercentages => rewardRarityPercentages;
    
    /// <summary> Accounts for 0 indexing</summary>
    public int MaxRarity => RarityColors.Length - 1;

    void OnEnable() => RefreshRewardRarityPercentages();

    void OnValidate() => RefreshRewardRarityPercentages();

    void RefreshRewardRarityPercentages()
    {
        if (RewardRarityWeights == null)
        {
            rewardRarityPercentages = new float[0];
            return;
        }

        rewardRarityPercentages = new float[RewardRarityWeights.Length];
        float totalWeight = GetTotalRewardRarityWeight();
        if (totalWeight <= Mathf.Epsilon) return;

        for (int i = 0; i < RewardRarityWeights.Length; i++)
        {
            rewardRarityPercentages[i] = Mathf.Max(0f, RewardRarityWeights[i]) / totalWeight * 100f;
        }
    }

    public int RollRewardRarityValue()
    {
        float totalWeight = GetTotalRewardRarityWeight();
        if (totalWeight <= Mathf.Epsilon) return 0;

        float roll = Random.value * totalWeight;

        for (int i = 0; i < RewardRarityWeights.Length; i++)
        {
            roll -= Mathf.Max(0f, RewardRarityWeights[i]);
            if (roll <= 0f) return i;
        }

        return RewardRarityWeights.Length - 1;
    }

    float GetTotalRewardRarityWeight()
    {
        if (RewardRarityWeights == null) return 0f;

        float totalWeight = 0f;

        foreach (float weight in RewardRarityWeights)
        {
            totalWeight += Mathf.Max(0f, weight);
        }

        return totalWeight;
    }

    //ToDo: Add Colors for ability lost and empty slot!!
}
