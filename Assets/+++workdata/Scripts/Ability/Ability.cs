using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary> The template and function for all abilities </summary>
public abstract class Ability : MonoBehaviour
{
    #region Serialized
    [SerializeField] protected float cooldown;
    [SerializeField] protected AbilitySO abilitySO;
    #endregion

    #region Non Serialized
    public AbilitySO AbilitySO => abilitySO;
    public bool IsActive => cooldown > 0;
    GameObject numberObject;

    Image abilityImage;
    Image abilityImageBG;
    Coroutine coolDownCor;
    #endregion

    public void EnterAbility(Image _abilityImage, Image _abilityImageBG, GameObject _numberObject)
    {
        abilityImage = _abilityImage;
        abilityImageBG = _abilityImageBG;
        numberObject = _numberObject;
        OnInitialized();
    }

    public virtual void Execute(bool performed = true)
    {
        if (coolDownCor != null) return;

        if (performed)
        {
            coolDownCor = StartCoroutine(Cooldown());
            ExecuteInternal();
            numberObject.SetActive(false);
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
            abilityImageBG.fillAmount = wentByTime / cooldown;
            abilityImage.fillAmount = wentByTime / cooldown;
            yield return null;
        }

        numberObject.SetActive(true);
        coolDownCor = null;
    }
}