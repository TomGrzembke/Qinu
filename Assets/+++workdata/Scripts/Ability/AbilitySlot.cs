using MyBox;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Handles the image and execution of abilities </summary>
public class AbilitySlot : MonoBehaviour
{
    [field: SerializeField] public bool Performed { get; private set; }
    public GameObject CurrentAbilityPrefab => currentAbilityPrefab;
    [SerializeField] GameObject currentAbilityPrefab;
    [field: SerializeField] public Ability CurrentAbility { get; private set; }
    [SerializeField] int slotIndex;
    [SerializeField] Image abilityImage;
    [SerializeField] Image abilityImageBG;
    [SerializeField] GameObject numberObject;
    [SerializeField] ParticleSystem lostVFX;

    public bool Occupied { get; private set; }
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnValidate()
    {
        RefreshPicture();
    }

    void RefreshPicture()
    {
        Ability tempAbility = null;
        var hasAbilityPrefab = currentAbilityPrefab != null;

        if (hasAbilityPrefab)
        {
            currentAbilityPrefab.TryGetComponent(out tempAbility);
        }

        abilityImage.sprite = hasAbilityPrefab ? tempAbility.AbilitySO.abilitySprite : null;

        Occupied = hasAbilityPrefab;

        abilityImage.SetAlpha(Occupied ? 255 : 0);

        if (tempAbility)
        {
            numberObject.SetActive(tempAbility.IsActive);
        }
        else
        {
            numberObject.SetActive(false);
        }
    }

    public void ChangeAbilityPrefab(GameObject newAbilityPrefab)
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.Cleanup();
            DestroyImmediate(CurrentAbility.gameObject, true);
        }

        if (newAbilityPrefab == null)
        {
            abilityImageBG.color = Color.red;
            lostVFX.Play();
            numberObject.SetActive(false);
        }
        else
        {
            abilityImageBG.color = Color.gray;
        }

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
        if (CurrentAbility == null) return;

        CurrentAbility.EnterAbility(abilityImage, abilityImageBG, numberObject, anim);
    }

    public void Execute(bool performed = true)
    {
        Performed = performed;

        if (CurrentAbility == null)
        {
            SoundManager.Instance.PlaySound(SoundType.AbilityCooldown);
            anim.SetTrigger("wobble");
            return;
        }

        CurrentAbility.Execute(performed);
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
}