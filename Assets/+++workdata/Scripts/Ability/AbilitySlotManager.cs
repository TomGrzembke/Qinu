using MyBox;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySlotManager : MonoBehaviour
{
    [field: SerializeField] public AbilitySlot[] AbilitySlots { get; private set; }

    [SerializeField] string[] abilityKeys = new string[4] { "Left", "Middle", "Right", "None" };

    [Header("Ability getter")]
    [SerializeField]
    Transform playerObj;

    public Transform PlayerObj => playerObj;
    [field: SerializeField] public Transform Puk { get; private set; }
    [field: SerializeField] public Transform Middle { get; private set; }
    [field: SerializeField] public Vector2 MiddleStartPosition { get; private set; }
    [field: SerializeField] public AbilityRaritiesSO RaritySO { get; private set; }

    [SerializeField] AbilityExchange abilityExchange;


    public static AbilitySlotManager Instance;
    const int RARITIES_LOST = 2;


    void Awake() => Instance = this;

    void OnValidate()
    {
        foreach (var entry in AbilitySlots)
        {
            entry.RefreshPicture();
            entry.RefreshRarity(RaritySO);
        }
    }

    void Start()
    {
        MiddleStartPosition = Middle.position;

        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            AbilitySlots[i].SetSlotIndex(i);

            if (AbilitySlots[i].CurrentAbilityPrefab == null) continue;

            ExchangeAbility(AbilitySlots[i].CurrentAbilityPrefab, i);
        }
    }

    public void ActivateSlot(int slotIndex, bool performed = true)
    {
        if (slotIndex > AbilitySlots.Length - 1) return;
        if (IsAbilityExchangeCall()) return;

        AbilitySlots[slotIndex].Execute(performed);
    }

    bool IsAbilityExchangeCall()
    {
        if (abilityExchange == null) return false;

        return abilityExchange.enabled;
    }

    public void ExchangeAbility(GameObject newPrefab, int slotIndex, int rewardRarity = 0)
    {
        if (slotIndex == -1)
        {
            AddNewAbility(newPrefab, rewardRarity);
            return;
        }

        AbilitySlots[slotIndex].ChangeAbilityPrefab(newPrefab);
        ApplyRarityUpgrades(AbilitySlots[slotIndex], rewardRarity);
    }

    public GameObject RemoveRandomAbility()
    {
        List<int> candidateSlotIndices = new();

        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (i == 0)
            {
                if (AbilitySlots[i].Occupied && AbilitySlots[i].GetRarity() <= 0) continue;
            }

            candidateSlotIndices.Add(i);
        }

        if (candidateSlotIndices.Count == 0) return null;

        int randomSlotIndex = candidateSlotIndices[Random.Range(0, candidateSlotIndices.Count)];
        return RemoveAbility(randomSlotIndex);
    }

    GameObject RemoveAbility(int slotIndex)
    {
        if (!AbilitySlots[slotIndex].Occupied)
        {
            AbilitySlots[slotIndex].ShowEmptySlotLossFeedback();
            return null;
        }

        GameObject prefab = AbilitySlots[slotIndex].CurrentAbilityPrefab;

        if (slotIndex == 0)
        {
            int rarityLoss = Mathf.Min(RARITIES_LOST, AbilitySlots[slotIndex].GetRarity());
            AbilitySlots[slotIndex].ReduceRarity(rarityLoss, RaritySO);
            return prefab;
        }

        if (!AbilitySlots[slotIndex].ReduceRarity(RARITIES_LOST, RaritySO))
        {
            AbilitySlots[slotIndex].ChangeAbilityPrefab(null);
        }

        return prefab;
    }

    public void AddNewAbility(GameObject newPrefab, int rewardRarity)
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (AbilitySlots[i].CurrentAbilityPrefab == newPrefab)
            {
                ApplyRarityUpgrades(AbilitySlots[i], rewardRarity + 1);

                return;
            }
        }

        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied)
            {
                AbilitySlots[i].ChangeAbilityPrefab(newPrefab);
                ApplyRarityUpgrades(AbilitySlots[i], rewardRarity);

                break;
            }
        }
    }

    void ApplyRarityUpgrades(AbilitySlot slot, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (!slot.UpgradeRarity(RaritySO)) break;
        }

        slot.RefreshRarity(RaritySO);
    }

    public int GetAbilityRarityValue(GameObject abilityPrefab)
    {
        int slotID = GetAbilitySlot(abilityPrefab);
        return slotID < 0 ? 0 : AbilitySlots[slotID].GetRarity() + 1;
    }

    public int GetResultingRarityIndex(GameObject abilityPrefab, int rewardRarity)
    {
        int currentRarityValue = GetAbilityRarityValue(abilityPrefab);
        int rewardRarityValue = rewardRarity + 1;
        int resultingRarityValue = currentRarityValue > 0 ? currentRarityValue + rewardRarityValue : rewardRarityValue;
        return Mathf.Clamp(resultingRarityValue - 1, 0, RaritySO.MaxRarity);
    }

    public bool CheckIfSlotAvailable()
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied) return true;
        }

        return false;
    }

    public bool CheckIfAbilityCanUpgradeSomething(GameObject abilityPrefab)
    {
        bool equalsCurrentPrefab = false;
        bool hasUpgradeRoom = false;

        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied) continue;
            if (AbilitySlots[i].CurrentAbility == null) continue;

            equalsCurrentPrefab = AbilitySlots[i].CurrentAbilityPrefab.Equals(abilityPrefab);
            hasUpgradeRoom = AbilitySlots[i].GetRarity() < RaritySO.MaxRarity;

            if (equalsCurrentPrefab && hasUpgradeRoom) return true;
        }

        return false;
    }

    public bool CheckIfSlotHasUpgradeRoom(GameObject abilityPrefab)
    {
        var slotID = GetAbilitySlot(abilityPrefab);

        if (slotID == -1) return true;

        bool hasUpgradeRoom = AbilitySlots[slotID].GetRarity() < RaritySO.MaxRarity;

        if (hasUpgradeRoom) return true;

        return false;
    }

    public bool CheckIfSlotHasUpgradeRoom(int slotID)
    {
        if (AbilitySlots[slotID] == null) return true;

        bool hasUpgradeRoom = AbilitySlots[slotID].GetRarity() < RaritySO.MaxRarity;

        if (hasUpgradeRoom) return true;

        return false;
    }


    public string GetAvailableSlotKey()
    {
        int slotID = -1;
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (AbilitySlots[i].Occupied) continue;

            slotID = i;
            break;
        }

        if (slotID == -1) return "Upgrade or Exchange";
        if (abilityKeys.Length - 1 >= slotID) return abilityKeys[slotID];

        return "Apparently No";
    }

    public bool GetAbilitySlotPerformed(int index)
    {
        return AbilitySlots[index].Performed;
    }

    [ButtonMethod]
    public void Upgrade0()
    {
        UpgradeRarity(0);
    }

    public bool UpgradeRarity(int index)
    {
        if (index < 0 || index >= AbilitySlots.Length) return false;

        return AbilitySlots[index].UpgradeRarity(RaritySO);
    }

    public void SetSelectable(bool condition)
    {
        foreach (var entry in AbilitySlots)
        {
            entry.SetSelectable(condition);
        }
    }

    public int GetAbilitySlot(GameObject prefab)
    {
        foreach (var entry in AbilitySlots)
        {
            if (entry == null) continue;
            if (entry.CurrentAbilityPrefab == null) continue;
            if (!entry.CurrentAbilityPrefab.Equals(prefab)) continue;

            return entry.GetSlotIndex();
        }

        return -1;
    }
}
