using MyBox;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary> Handles the image and execution of abilities </summary>
public class AbilitySlot : MonoBehaviour
{
    [Header("Runtime")]
    [field: SerializeField] public bool Performed { get; private set; }
    [SerializeField] int slotIndex;

    [Header("Settings")]
    [SerializeField] bool blockedByUI = false;

    [Header("References")]
    public GameObject CurrentAbilityPrefab => currentAbilityPrefab;
    [SerializeField] GameObject currentAbilityPrefab;

    [field: SerializeField] public Ability CurrentAbility { get; private set; }
    [SerializeField] Image abilityImage;
    [SerializeField] Image[] abilityBGImages;
    [SerializeField] GameObject numberObject;
    [SerializeField] ParticleSystem lostVFX;

    public bool Occupied { get; private set; }
    Animator anim;
    float lastExecutedTime;
    const float EXECUTE_COOLDOWN = 0.2f;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void RefreshPicture()
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

    public void RefreshRarity(AbilityRaritiesSO abilityRaritiesSO)
    {
        var currentRarity = 0;

        if (CurrentAbility != null)
        {
            currentRarity = CurrentAbility.GetCurrentRarity();
        }

        var currentRarityColor = abilityRaritiesSO.RarityColors[currentRarity];

        foreach (var entry in abilityBGImages)
        {
            entry.color = currentRarityColor;
        }
    }

    public bool UpgradeRarity(AbilityRaritiesSO abilityRaritiesSO)
    {
        if (CurrentAbility == null) return false;

        bool succeeded = CurrentAbility.UpgradeRarity(abilityRaritiesSO.MaxRarity);
        RefreshRarity(abilityRaritiesSO);

        return succeeded;
    }

    public bool ReduceRarity(int amount, AbilityRaritiesSO abilityRaritiesSO)
    {
        if (CurrentAbility == null) return false;
        if (!CurrentAbility.ReduceRarity(amount)) return false;

        PlayLostVFX();
        RefreshRarity(abilityRaritiesSO);
        return true;
    }

    public void PlayLostVFX() => lostVFX.Play();

    public void ShowEmptySlotLossFeedback()
    {
        PlayLostVFX();

        foreach (var entry in abilityBGImages)
        {
            entry.color = Color.red;
        }
    }

    public void ChangeAbilityPrefab(GameObject newAbilityPrefab)
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.Cleanup();
        }

        bool lostAbility = newAbilityPrefab == null;
        currentAbilityPrefab = newAbilityPrefab;

        if (lostAbility)
        {
            PlayLostVFX();
            numberObject.SetActive(false);
        }
        else
        {
            CurrentAbility = Instantiate(newAbilityPrefab, gameObject.transform).GetComponent<Ability>();
            numberObject.SetActive(true);

            EnterAbility();
        }

        RefreshPicture();

        if (lostAbility)
        {
            foreach (var entry in abilityBGImages)
            {
                entry.color = Color.red;
            }
        }
    }

    public void EnterAbility()
    {
        if (CurrentAbility == null) return;

        CurrentAbility.EnterAbility(abilityImage, abilityBGImages, numberObject, anim);
    }

    public void Execute(bool performed = true)
    {
        if (performed == false)
        {
            Performed = false;
        }

        if (IsBlocked()) return;

        if (Time.time - lastExecutedTime < EXECUTE_COOLDOWN) return;

        lastExecutedTime = Time.time;

        Performed = performed;

        if (CurrentAbility == null)
        {
            SoundManager.Instance.PlaySound(SoundType.AbilityCooldown);
            anim.SetTrigger("wobble");
            return;
        }

        CurrentAbility.Execute(performed);
    }

    bool IsBlocked()
    {
        if (!blockedByUI) return false;

        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = Mouse.current != null ? Mouse.current.position.ReadValue() : Touchscreen.current.primaryTouch.position.ReadValue()
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Selectable>() == null) continue;

            return true;
        }

        return false;
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }

    public void SetSelectable(bool condition)
    {
        anim.SetBool("selectable", condition);
    }

    public int GetRarity()
    {
        if (CurrentAbility == null) return -1;

        return CurrentAbility.GetCurrentRarity();
    }
}
