using UnityEngine;

public class AbilitySlotManager : MonoBehaviour
{
    #region serialized fields
    public GameObject PlayerObj => playerObj;
    [SerializeField] GameObject playerObj;
    public GameObject PlayerGFX => playerGFX;
    [SerializeField] GameObject playerGFX;
    public Transform MousePos => mousePos;
    [SerializeField] Transform mousePos;
    //public StatusManager StatusManager => statusManager;
    //[SerializeField] StatusManager statusManager;

    [SerializeField] AbilitySlot[] abilitySlots;
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

    public void ActivateSlot(int slotIndex, bool deactivate = false)
    {
        abilitySlots[slotIndex].Execute(deactivate);
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