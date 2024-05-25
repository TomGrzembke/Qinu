using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{
    #region serialized fields
    [SerializeField] protected float cooldown;
    [SerializeField] protected AbilitySO abilitySO;
    public AbilitySO AbilitySO => abilitySO;
    public bool IsActive => cooldown > 0;
    #endregion

    #region private fields
    protected AbilitySlotManager abilitySlotManager;
    Image abilityImage;
    Coroutine coolDownCor;
    #endregion

    public void EnterAbility(AbilitySlotManager _abilitySlotManager, Image _abilityImage)
    {
        abilitySlotManager = _abilitySlotManager;
        abilityImage = _abilityImage;
        OnInitialized(_abilitySlotManager);
    }

    public virtual void Execute(AbilitySlotManager _abilitySlotManager, bool performed = true)
    {
        if (coolDownCor != null) return;

        abilitySlotManager = _abilitySlotManager;
        if (performed)
        {
            coolDownCor = StartCoroutine(Cooldown());
            ExecuteInternal();
        }
        else
            DeExecuteInternal();
    }
    public void OnInitialized(AbilitySlotManager _abilitySlotManager)
    {
        OnInitializedInternal(_abilitySlotManager);
    }
    protected abstract void ExecuteInternal();
    protected abstract void DeExecuteInternal();
    protected abstract void OnInitializedInternal(AbilitySlotManager _abilitySlotManager);

    IEnumerator Cooldown()
    {
        float wentByTime = 0;
        while (wentByTime < cooldown)
        {
            wentByTime += Time.deltaTime;
            abilityImage.fillAmount = wentByTime / cooldown;
            yield return null;
        }

        coolDownCor = null;
    }
}