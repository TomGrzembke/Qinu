using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary> Responsible for the UI and distrebution of abilities</summary>
public class RewardWindow : MonoBehaviour
{
    [SerializeField] GameObject rewardWindow;
    [SerializeField] GameObject essentialUI;

    [SerializeField] TextMeshProUGUI[] choiceButtonTexts;
    [SerializeField] UIButton[] choiceButtonAnimation;
    [SerializeField] TextMeshProUGUI keySlotDescription;

    [SerializeField] float fadeTime = 2;
    [SerializeField] List<GameObject> possibleRewards;
    [SerializeField] AbilityExchange abilityExchange;
    [field: SerializeField] public bool InAbilitySelect { get; private set; }

    public static RewardWindow Instance;
    GameObject[] rewards;
    CanvasGroup rewardWindowCanvasGroup;
    Coroutine currentRewarWindowCoroutine;
    GameObject rewardSelected;

    void Awake()
    {
        Instance = this;
        rewardWindowCanvasGroup = rewardWindow.GetComponent<CanvasGroup>();
    }

    public void OpenRewardWindow(bool showAll = true)
    {
        if (showAll)
        {
            for (int i = 0; i < choiceButtonTexts.Length; i++)
            {
                choiceButtonTexts[i].gameObject.SetActive(true);
            }
        }

        if (currentRewarWindowCoroutine != null)
        {
            StopCoroutine(currentRewarWindowCoroutine);
        }

        MinigameManager.Instance.CageBall();
        SoundManager.Instance.PlaySound(SoundType.AbilityPopup);
        StopChoicePressedAnimation();

        currentRewarWindowCoroutine = StartCoroutine(ShowCoroutine());
    }

    [ButtonMethod]
    public void GiveReward()
    {
        rewards = PickThreeRewards();
        string currentText = "";

        for (int i = 0; i < choiceButtonTexts.Length; i++)
        {
            if (rewards[i] != null && rewards[i].TryGetComponent(out Ability ability))
            {
                currentText = ability.AbilitySO.abilityTitel;
            }
            else
            {
                Debug.Log(i + " has no Ability Script");
                choiceButtonTexts[i].gameObject.SetActive(false);
            }

            choiceButtonTexts[i].text = currentText;
        }
        OpenRewardWindow();

        keySlotDescription.text = AbilitySlotManager.Instance.GetAvailableSlotKey() + " Key Slot";
    }

    public void GiveSingleReward(GameObject specified)
    {
        string currentText;
        GameObject[] _rewards = new GameObject[3];


        if (specified.TryGetComponent(out Ability ability))
        {
            currentText = ability.AbilitySO.abilityTitel;
        }
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
        AbilitySlotManager.Instance.RemoveRandomAbility();
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
        if (priorChoice1 != null && priorChoice2 && possibleRewards.Count < 3) return priorChoice1;

        GameObject randomObject = possibleRewards[Random.Range(0, possibleRewards.Count)];

        bool exists = randomObject != null;
        bool isDifferentToChoice1 = randomObject != priorChoice1;
        bool isDifferentToChoice2 = randomObject != priorChoice2;
        bool hasUpgradeRoom = AbilitySlotManager.Instance.CheckIfSlotHasUpgradeRoom(randomObject);

        bool applicableAbility = exists && isDifferentToChoice1 && isDifferentToChoice2 && hasUpgradeRoom;

        if (!applicableAbility) return GetRandomReward(priorChoice1, priorChoice2);

        return randomObject;
    }

    public void RewardPicked(int buttonID)
    {
        var abilityPrefab = rewards[buttonID];
        if (abilityPrefab == null) return;

        var slotAvailable = AbilitySlotManager.Instance.CheckIfSlotAvailable();
        var upgradeAvailable = AbilitySlotManager.Instance.CheckIfAbilityCanUpgradeSomething(abilityPrefab);

        if (!slotAvailable && !upgradeAvailable)
        {
            abilityExchange.enabled = true;
            abilityExchange.OnSlotSelected -= SelectedSlot;
            abilityExchange.OnSlotSelected += SelectedSlot;

            rewardSelected = abilityPrefab;
            keySlotDescription.text = "Press the Ability you want to exchange";

            StopChoicePressedAnimation();
            choiceButtonAnimation[buttonID].SetPressed(gameObject, true);

            if (!AbilitySlotManager.Instance.CheckIfSlotAvailable())
            {
                AbilitySlotManager.Instance.SetSelectable(true);
            }

            return;
        }

        ReceiveReward(buttonID);
    }

    void ReceiveReward(int buttonID)
    {
        AbilitySlotManager.Instance.AddNewAbility(rewards[buttonID]);
        Close();
    }

    void ExchangeReward(GameObject reward, int atIndex)
    {
        if (reward == null)
        {
            Debug.LogError("Reward received is null", gameObject);
            return;
        }

        AbilitySlotManager.Instance.ExchangeAbility(reward, atIndex);

        Close();
    }

    [ButtonMethod]
    public void Close()
    {
        MinigameManager.Instance.ReleaseBall();
        AbilitySlotManager.Instance.SetSelectable(false);

        StopChoicePressedAnimation();

        if (currentRewarWindowCoroutine != null)
        {
            StopCoroutine(currentRewarWindowCoroutine);
        }

        currentRewarWindowCoroutine = StartCoroutine(HideCoroutine());
    }

    void StopChoicePressedAnimation()
    {
        foreach (var entry in choiceButtonAnimation)
        {
            if (entry == null) continue;

            entry.SetPressed(gameObject, false);
        }
    }

    IEnumerator ShowCoroutine()
    {
        InputManager.Instance.ShowCursor();


        InAbilitySelect = true;

        essentialUI.SetActive(true);
        rewardWindow.SetActive(true);

        rewardWindowCanvasGroup.alpha = 0;

        float time = 0;

        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            rewardWindowCanvasGroup.alpha = Mathf.Clamp01(time / fadeTime);
        }

        rewardWindowCanvasGroup.interactable = true;
        rewardWindowCanvasGroup.alpha = 1;
    }

    IEnumerator HideCoroutine()
    {
        InputManager.Instance.HideCursor();

        rewardWindowCanvasGroup.interactable = false;
        essentialUI.SetActive(true);
        rewardWindowCanvasGroup.alpha = 0;
        rewardWindow.SetActive(false);
        float time = 0;

        while (time < fadeTime)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
        }
        InAbilitySelect = false;
    }

    public void SelectedSlot(int i)
    {
        if (rewardSelected == null) return;

        StopChoicePressedAnimation();

        abilityExchange.enabled = false;
        abilityExchange.OnSlotSelected -= SelectedSlot;

        ExchangeReward(rewardSelected, i);
        rewardSelected = null;
    }
}