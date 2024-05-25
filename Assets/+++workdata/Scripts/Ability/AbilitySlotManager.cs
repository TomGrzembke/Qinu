using UnityEngine;

public class AbilitySlotManager : MonoBehaviour
{
    #region serialized fields
    [SerializeField] AbilitySlot[] abilitySlots;
    [Header("Ability getter")]
    [SerializeField] Transform playerObj;
    public Transform PlayerObj => playerObj;
    [field: SerializeField] public Transform Puk { get; private set; }  
    

    #endregion

    #region private fields
    public static AbilitySlotManager Instance;
    #endregion

    void Awake() => Instance = this;

    void Start()
    {
        for (int i = 0; i < abilitySlots.Length; i++)
        {
            abilitySlots[i].SetSlotIndex(i);
            if (abilitySlots[i].CurrentAbility)
                AddNewAbility(abilitySlots[i].CurrentAbilityPrefab, i);
        }
    }

    public void ActivateSlot(int slotIndex, bool performed = true)
    {
        abilitySlots[slotIndex].Execute(performed);
    }

    public void AddNewAbility(GameObject newPrefab, int slotIndex)
    {
        abilitySlots[slotIndex].ChangeAbilityPrefab(newPrefab, this);
    }

    public void AddNewAbility(GameObject newPrefab)
    {
        for (int i = 0; i < abilitySlots.Length; i++)
        {
            if (abilitySlots[i].CurrentAbilityPrefab == newPrefab)
                break;
            else if (!abilitySlots[i].occupied)
            {
                abilitySlots[i].ChangeAbilityPrefab(newPrefab, this);
               // RewardWindow.Instance.GiveReward(newPrefab);
                break;
            }
        }
    }
}