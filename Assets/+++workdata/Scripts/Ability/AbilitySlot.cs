using MyBox;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    #region serialized fields
    public GameObject CurrentAbilityPrefab => currentAbilityPrefab;
    [SerializeField] GameObject currentAbilityPrefab;
    [field: SerializeField] public Ability CurrentAbility { get; private set; }
    [SerializeField] int slotIndex;
    [SerializeField] Image abilityImage;
    [SerializeField] GameObject numberObject;
    #endregion

    #region private fields
    public bool Occupied { get; private set; }
    #endregion

    void OnValidate()
    {
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
            Occupied = false;
        }
        else
        {
            Occupied = true;
        }

        abilityImage.SetAlpha(Occupied ? 255 : 0);
        if (tempAbility)
            numberObject.SetActive(tempAbility.IsActive);
    }

    public void ChangeAbilityPrefab(GameObject newAbilityPrefab)
    {
        if (CurrentAbility)
            DestroyImmediate(CurrentAbility.gameObject, true);

        currentAbilityPrefab = newAbilityPrefab;

        if (currentAbilityPrefab)
        {
            CurrentAbility = Instantiate(newAbilityPrefab, gameObject.transform).GetComponent<Ability>();
            EnterAbility();
        }
        RefreshPicture();
    }
    public void EnterAbility()
    {
        if (CurrentAbility)
            CurrentAbility.EnterAbility(abilityImage);
    }
    public void Execute(bool performed = true)
    {
        if (CurrentAbility)
            CurrentAbility.Execute(performed);
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
}