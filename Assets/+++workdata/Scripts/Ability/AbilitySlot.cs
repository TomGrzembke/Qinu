using MyBox;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary> Handles the image and execution of abilities </summary>
public class AbilitySlot : MonoBehaviour
{
    [field: SerializeField] public bool Performed { get; private set; }
    [SerializeField] bool blockedByUI = false;
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

    float lastExecutedTime;
    const float EXECUTE_COOLDOWN = 0.2f;

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
            //DestroyImmediate(CurrentAbility.gameObject, true);
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
        if (performed == false)
        {
            Performed = false;
        }

        if (blockedByUI && IsPointerOverUI()) return;

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

    bool IsPointerOverUI()
    {
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
}