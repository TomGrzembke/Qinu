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
    protected AbilitySlotManager abilitySlotManager => AbilitySlotManager.Instance;
    Image abilityImage;
    Coroutine coolDownCor;
    #endregion

    public void EnterAbility(Image _abilityImage)
    {
        abilityImage = _abilityImage;
        OnInitialized();
    }

    public virtual void Execute(bool performed = true)
    {
        if (coolDownCor != null) return;

        if (performed)
        {
            coolDownCor = StartCoroutine(Cooldown());
            ExecuteInternal();
        }
        else
            DeExecuteInternal();
    }
    public void OnInitialized()
    {
        OnInitializedInternal();
    }
    protected abstract void ExecuteInternal();
    protected abstract void DeExecuteInternal();
    protected abstract void OnInitializedInternal();

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