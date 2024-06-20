using UnityEngine;

public class AbilitySlotManager : MonoBehaviour
{
    #region serialized fields
    [field: SerializeField] public AbilitySlot[] AbilitySlots { get; private set; }
    [Header("Ability getter")]
    [SerializeField] Transform playerObj;
    public Transform PlayerObj => playerObj;
    [field: SerializeField] public Transform Puk { get; private set; }
    [field: SerializeField] public Transform Middle { get; private set; }


    #endregion

    #region private fields
    public static AbilitySlotManager Instance;
    #endregion

    void Awake() => Instance = this;

    void Start()
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            AbilitySlots[i].SetSlotIndex(i);
            if (AbilitySlots[i].CurrentAbilityPrefab)
                AddNewAbility(AbilitySlots[i].CurrentAbilityPrefab, i);
        }
    }

    public void ActivateSlot(int slotIndex, bool performed = true)
    {
        AbilitySlots[slotIndex].Execute(performed);
    }

    public void AddNewAbility(GameObject newPrefab, int slotIndex)
    {
        AbilitySlots[slotIndex].ChangeAbilityPrefab(newPrefab);
    }

    public GameObject RemoveRandomAbility()
    {
        int number = Random.Range(0, AbilitySlots.Length);
        GameObject prefab = AbilitySlots[number].CurrentAbilityPrefab;
        AbilitySlots[number].ChangeAbilityPrefab(null);
        return prefab;
    }

    public void AddNewAbility(GameObject newPrefab)
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (AbilitySlots[i].CurrentAbilityPrefab == newPrefab)
                break;
            else if (!AbilitySlots[i].Occupied)
            {
                AbilitySlots[i].ChangeAbilityPrefab(newPrefab);
                break;
            }
        }
    }

    public bool CheckIfSlotAvailable()
    {
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied)
                return true;
        }

        return false;
    }

    public string GetAvailableSlotKey()
    {
        int slotID = -1;
        for (int i = 0; i < AbilitySlots.Length; i++)
        {
            if (!AbilitySlots[i].Occupied)
                slotID = i;
        }

        if (slotID == 0)
            return "Q";
        else if (slotID == 1)
            return "W";
        else if (slotID == 2)
            return "E";
        else if (slotID == 3)
            return "R";

        return "Apparently No";
    }
}