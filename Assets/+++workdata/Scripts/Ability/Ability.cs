using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary> The template and function for all abilities </summary>
public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected float cooldown;
    [SerializeField] protected AbilitySO abilitySO;


    public AbilitySO AbilitySO => abilitySO;
    public bool IsActive => cooldown > 0;
    GameObject numberObject;
    Image abilityImage;
    Image abilityImageBG;
    Coroutine coolDownCor;
    Animator anim;

    public void EnterAbility(Image _abilityImage, Image _abilityImageBG, GameObject _numberObject, Animator _anim)
    {
        anim = _anim;
        abilityImage = _abilityImage;
        abilityImageBG = _abilityImageBG;
        numberObject = _numberObject;
        OnInitialized();
    }

    public virtual void Execute(bool performed = true)
    {
        if (coolDownCor != null)
        {
            if (performed)
            {
                SoundManager.Instance.PlaySound(SoundType.AbilityCooldown);
                anim.SetTrigger("wobble");
            }

            return;
        }

        if (performed)
        {
            coolDownCor = StartCoroutine(Cooldown());
            ExecuteInternal();
            numberObject.SetActive(false);
        }
    }


    public void OnInitialized()
    {
        OnInitializedInternal();
    }

    protected abstract void OnInitializedInternal();
    protected abstract void ExecuteInternal();
    protected abstract void CleanupInternal();

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

    public virtual void Cleanup()
    {
        CleanupInternal();

        if (coolDownCor != null)
        {
            StopCoroutine(coolDownCor);
            coolDownCor = null;
        }

        abilityImageBG.fillAmount = 1;
        abilityImage.fillAmount = 1;
    }

    void OnDestroy()
    {
        Cleanup();
    }
}