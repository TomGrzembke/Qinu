using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary> Responsible for the UI and distrebution of abilities</summary>
public class RewardWindow : MonoBehaviour
{
    #region Serialized
    [SerializeField] GameObject rewardWindow;
    [SerializeField] GameObject essentialUI;

    [SerializeField] TextMeshProUGUI[] choiceButtonTexts;
    [SerializeField] TextMeshProUGUI keySlotDescription;

    [SerializeField] float fadeTime = 2;
    [SerializeField] List<GameObject> possibleRewards;
    [SerializeField] List<GameObject> rewardsReceived;
    [field: SerializeField] public bool InAbilitySelect { get; private set; }

    #endregion

    #region Non Serialized
    public static RewardWindow Instance;
    GameObject[] rewards;
    CanvasGroup rewardWindowCanvasGroup;
    CanvasGroup essentialUICanvasGroup;
    Coroutine currentRewarWindowCoroutine;
    #endregion

    void Awake()
    {
        Instance = this;
        rewardWindowCanvasGroup = rewardWindow.GetComponent<CanvasGroup>();
        essentialUICanvasGroup = essentialUI.GetComponent<CanvasGroup>();
    }

    IEnumerator Start()
    {
        yield return null;

        AbilitySlot[] abilitySlots = AbilitySlotManager.Instance.AbilitySlots;

        for (int i = 0; i < abilitySlots.Length; i++)
        {
            if (abilitySlots[i].CurrentAbilityPrefab)
                rewardsReceived.Add(abilitySlots[i].CurrentAbilityPrefab);
        }
    }

    public void OpenRewardWindow(bool showAll = true)
    {
        if (showAll)
            for (int i = 0; i < choiceButtonTexts.Length; i++)
            {
                choiceButtonTexts[i].gameObject.SetActive(true);
            }

        if (currentRewarWindowCoroutine != null)
            StopCoroutine(currentRewarWindowCoroutine);

        SoundManager.Instance.PlaySound(SoundType.AbilityPopup);
        currentRewarWindowCoroutine = StartCoroutine(ShowCoroutine());
    }

    [ButtonMethod]
    public void GiveReward()
    {
        if (!AbilitySlotManager.Instance.CheckIfSlotAvailable()) return;

        rewards = PickThreeRewards();
        string currentText = "";

        for (int i = 0; i < choiceButtonTexts.Length; i++)
        {
            if (rewards[i] != null && rewards[i].TryGetComponent(out Ability ability))
                currentText = ability.AbilitySO.abilityTitel;
            else
            {
                Debug.Log(i + " has no Ability Script");
                choiceButtonTexts[i].gameObject.SetActive(false);
            }

            choiceButtonTexts[i].text = currentText;
        }
        keySlotDescription.text = AbilitySlotManager.Instance.GetAvailableSlotKey() + " Key Slot";

        OpenRewardWindow();
    }
    public void GiveSingleReward(GameObject specified)
    {
        string currentText;
        GameObject[] _rewards = new GameObject[3];


        if (specified.TryGetComponent(out Ability ability))
            currentText = ability.AbilitySO.abilityTitel;
        else
        {
            Debug.Log(specified.name + " has no Ability Script");
            currentText = "Random";
        }

        choiceButtonTexts[0].gameObject.SetActive(false);
        choiceButtonTexts[1].text = currentText;
        choiceButtonTexts[2].gameObject.SetActive(false);
        _rewards[1] = specified;

        rewards = _rewards;
        keySlotDescription.text = AbilitySlotManager.Instance.GetAvailableSlotKey() + " Key Slot";

        OpenRewardWindow(false);
    }

    [ButtonMethod]
    public void RemoveReward()
    {
        rewardsReceived.Remove(AbilitySlotManager.Instance.RemoveRandomAbility());
    }

    public GameObject[] PickThreeRewards()
    {
        GameObject[] rewards = new GameObject[3];
        rewards[0] = GetRandomReward();
        rewards[1] = GetRandomReward(rewards[0]);
        rewards[2] = GetRandomReward(rewards[0], rewards[1]);

        return rewards;
    }

    /// <summary> Gets a random reward and repeats if it isn't applicable </summary>
    public GameObject GetRandomReward(GameObject priorChoice1 = null, GameObject priorChoice2 = null)
    {

        GameObject randomObject = possibleRewards[Random.Range(0, possibleRewards.Count)];

        bool applicableAbility = randomObject != null && !rewardsReceived.Contains(randomObject)
            && randomObject != priorChoice1 && randomObject != priorChoice2;

        if (!applicableAbility)
            return GetRandomReward(priorChoice1, priorChoice2);

        return randomObject;
    }

    public void RewardPicked(int buttonID)
    {
        if (!rewards[buttonID]) return;

        AbilitySlotManager.Instance.AddNewAbility(rewards[buttonID]);
        rewardsReceived.Add(rewards[buttonID]);
        Close();
    }

    [ButtonMethod]
    public void Close()
    {
        if (currentRewarWindowCoroutine != null)
            StopCoroutine(currentRewarWindowCoroutine);

        currentRewarWindowCoroutine = StartCoroutine(HideCoroutine());
    }

    IEnumerator ShowCoroutine()
    {
        Cursor.visible = true;
        InAbilitySelect = true;

        essentialUI.SetActive(true);
        rewardWindow.SetActive(true);

        essentialUICanvasGroup.alpha = 1;
        rewardWindowCanvasGroup.alpha = 0;

        float time = 0;

        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            essentialUICanvasGroup.alpha = 1 - Mathf.Clamp01(time / fadeTime);
            rewardWindowCanvasGroup.alpha = Mathf.Clamp01(time / fadeTime);
        }
        rewardWindowCanvasGroup.interactable = true;
        rewardWindowCanvasGroup.alpha = 1;
        essentialUICanvasGroup.alpha = 0;
    }

    IEnumerator HideCoroutine()
    {
        Cursor.visible = false;
        rewardWindowCanvasGroup.interactable = false;
        essentialUICanvasGroup.alpha = 0;
        essentialUI.SetActive(true);
        rewardWindowCanvasGroup.alpha = 0;
        rewardWindow.SetActive(false);
        float time = 0;

        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            essentialUICanvasGroup.alpha = Mathf.Clamp01(time / fadeTime);
        }
        essentialUICanvasGroup.alpha = 1;
        InAbilitySelect = false;
    }
}