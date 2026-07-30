using MyBox;
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


    public static AbilitySlotManager Instance;


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
        if(slotIndex > AbilitySlots.Length -1) return;

        AbilitySlots[slotIndex].Execute(performed);
    }

    public void ExchangeAbility(GameObject newPrefab, int slotIndex)
    {
        if (slotIndex == -1)
        {
            AddNewAbility(newPrefab);
            return;
        }

        AbilitySlots[slotIndex].ChangeAbilityPrefab(newPrefab);
        AbilitySlots[slotIndex].RefreshRarity(RaritySO);
    }

    public GameObject RemoveRandomAbility()
    {
        int number = Random.Range(1, AbilitySlots.Length);
        return RemoveAbility(number);
    }

    GameObject RemoveAbility(int slotIndex)
    {
        GameObject prefab = AbilitySlots[slotIndex].CurrentAbilityPrefab;
        AbilitySlots[slotIndex].ChangeAbilityPrefab(null);
        return prefab;
    }

    public void AddNewAbility(GameObject newPrefab)
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (AbilitySlots[i].CurrentAbilityPrefab == newPrefab)
            {
                AbilitySlots[i].UpgradeRarity(RaritySO);

                return;
            }
        }

        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied)
            {
                AbilitySlots[i].ChangeAbilityPrefab(newPrefab);
                AbilitySlots[i].RefreshRarity(RaritySO);

                break;
            }
        }
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