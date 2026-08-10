using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

/// <summary> The template and function for all abilities </summary>
public abstract class Ability : MonoBehaviour
{

    [SerializeField] protected float[] cooldownPerRarity;
    [SerializeField] protected AbilitySO abilitySO;

    public AbilitySO AbilitySO => abilitySO;
    public bool IsActive => currentabilityTime == 0;
    protected int currentRarity;
    GameObject numberObject;
    Image abilityImage;
    Image[] abilityBGImages;
    Coroutine coolDownCor;
    Animator anim;
    float currentCooldown;
    float currentabilityTime;
    bool isCleaningUp;

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
        if (currentRarity >= maxRarity) return false;

        currentRarity++;
        return true;
    }

    public bool ReduceRarity(int amount)
    {
        currentRarity -= amount;
        return currentRarity >= 0;
    }

    public int GetCurrentRarity()
    {
        return currentRarity;
    }


    public virtual void Execute(bool performed = true)
    {
        if (!performed) return;
        if (isCleaningUp) return;

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
        currentabilityTime = 0;

        currentCooldown = cooldownPerRarity[EvaluateRaritySizing(cooldownPerRarity.Length)];

        while (currentabilityTime < currentCooldown)
        {
            currentabilityTime += Time.deltaTime;

            foreach (var entry in abilityBGImages)
            {
                entry.fillAmount = currentabilityTime / currentCooldown;
            }

            abilityImage.fillAmount = currentabilityTime / currentCooldown;
            yield return null;
        }

        numberObject.SetActive(true);
        coolDownCor = null;
        currentabilityTime = 0;
    }

    public virtual void Cleanup()
    {
        isCleaningUp = true;
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

    protected void Clear()
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
