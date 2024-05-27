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
    [SerializeField] GameObject numberObject;
    [SerializeField] Ability currentAbility;
    #endregion

    #region private fields
    AbilitySlotManager AbilitySlotManager => AbilitySlotManager.Instance;
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
            if (currentAbilityPrefab && AbilitySlotManager)
                AbilitySlotManager.AddNewAbility(currentAbilityPrefab, slotIndex);
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

    public void ChangeAbilityPrefab(GameObject newAbilityPrefab)
    {
        if (currentAbility)
            DestroyImmediate(currentAbility.gameObject, true);

        currentAbilityPrefab = newAbilityPrefab;

        if (currentAbilityPrefab)
        {
            currentAbility = Instantiate(newAbilityPrefab, gameObject.transform).GetComponent<Ability>();
            EnterAbility();
        }
        RefreshPicture();
    }
    public void EnterAbility()
    {
        if (currentAbility)
            currentAbility.EnterAbility(abilityImage);
    }
    public void Execute(bool performed = true)
    {
        if (currentAbility)
            currentAbility.Execute(performed);
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
}