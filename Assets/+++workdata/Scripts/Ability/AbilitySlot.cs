using MyBox;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    #region serialized fields
    public GameObject CurrentAbilityPrefab => currentAbilityPrefab;
    [SerializeField] GameObject currentAbilityPrefab;
    [SerializeField] int slotIndex;
    [SerializeField] Image abilityImage;
    [SerializeField] AbilitySlotManager abilitySlotManager;
    [SerializeField] GameObject numberObject;
    [SerializeField] Ability currentAbility;
    #endregion

    #region private fields
    public bool occupied { get; private set; }
    public Ability CurrentAbility => currentAbility;
    #endregion


    void Start()
    {
        OnValidateCall();
    }

    void OnValidateCall()
    {
        if (Application.isPlaying)
            if (currentAbilityPrefab && abilitySlotManager)
                abilitySlotManager.AddNewAbility(currentAbilityPrefab, slotIndex);
        RefreshPicture();
    }

    void RefreshPicture()
    {
        Ability tempAbility = null;
        if (currentAbilityPrefab)
            currentAbilityPrefab.TryGetComponent(out tempAbility);

        abilityImage.sprite = currentAbilityPrefab ? tempAbility.AbilitySO.abilitySprite : null;

        if (!abilityImage.sprite)
        {
            occupied = false;
        }
        else
        {
            occupied = true;
        }

        abilityImage.SetAlpha(occupied ? 255 : 0);
        if (tempAbility)
            numberObject.SetActive(tempAbility.IsActive);
    }

    public void ChangeAbilityPrefab(GameObject newAbilityPrefab, AbilitySlotManager _abilitySlotManager)
    {
        if (currentAbility && newAbilityPrefab != currentAbilityPrefab)
            DestroyImmediate(currentAbility.gameObject, true);
        currentAbilityPrefab = newAbilityPrefab;

        if (currentAbilityPrefab)
        {
            currentAbility = Instantiate(newAbilityPrefab, gameObject.transform).GetComponent<Ability>();
            abilitySlotManager = _abilitySlotManager;
            EnterAbility();
        }
        RefreshPicture();
    }
    public void EnterAbility()
    {
        if (currentAbility)
            currentAbility.EnterAbility(abilitySlotManager, abilityImage);
    }
    public void Execute(bool performed = true)
    {
        if (currentAbility)
            currentAbility.Execute(abilitySlotManager, performed);
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
}