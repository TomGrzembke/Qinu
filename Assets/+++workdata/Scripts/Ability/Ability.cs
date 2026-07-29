using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

/// <summary> The template and function for all abilities </summary>
public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected float cooldown;
    [SerializeField] protected AbilitySO abilitySO;

    public AbilitySO AbilitySO => abilitySO;
    public bool IsActive => cooldown > 0;
    protected int currentRarity;
    GameObject numberObject;
    Image abilityImage;
    Image[] abilityBGImages;
    Coroutine coolDownCor;
    Animator anim;

    public void EnterAbility(Image _abilityImage, Image[] _abilityBGImages, GameObject _numberObject, Animator _anim)
    {
        anim = _anim;
        abilityImage = _abilityImage;
        abilityBGImages = _abilityBGImages;
        numberObject = _numberObject;
        OnInitialized();
    }

    public virtual bool UpgradeRarity(int maxRarity)
    {
        if(currentRarity >= maxRarity) return false;

        currentRarity++;
        return true;
    }

    public int GetCurrentRarity()
    {
        return currentRarity;
    }


    public virtual void Execute(bool performed = true)
    {
        if (!performed) return;

        if (coolDownCor != null)
        {
            SoundManager.Instance.PlaySound(SoundType.AbilityCooldown);
            anim.SetTrigger("wobble");

            return;
        }

        coolDownCor = StartCoroutine(Cooldown());
        ExecuteInternal();
        numberObject.SetActive(false);
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

            foreach (var entry in abilityBGImages)
            {
                entry.fillAmount = wentByTime / cooldown;
            }

            abilityImage.fillAmount = wentByTime / cooldown;
            yield return null;
        }

        numberObject.SetActive(true);
        coolDownCor = null;
    }

    public virtual void Cleanup()
    {
        currentRarity = 0;

        if (coolDownCor != null)
        {
            StopCoroutine(coolDownCor);
            coolDownCor = null;
        }

        foreach (var entry in abilityBGImages)
        {
            entry.fillAmount = 1;
        }

        abilityImage.fillAmount = 1;

        CleanupInternal();
    }

    protected void QueueDestroy(Coroutine coroutineToFinish)
    {
        if (coroutineToFinish == null)
        {
            Clear();
            return;
        }

        coroutineToFinish.OnComplete(Clear);
    }

    void Clear()
    {
        Destroy(gameObject);
    }

    protected int EvaluateRaritySizing(int listLength)
    {
        int raritySizing = currentRarity;

        if (raritySizing > listLength - 1)
        {
            raritySizing = listLength - 1;
        }

        if (raritySizing < 0)
        {
            raritySizing = 0;
        }

        return raritySizing;
    }
}