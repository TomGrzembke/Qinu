using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardWindow : MonoBehaviour
{
    public static RewardWindow Instance;

    #region serialized fields
    [SerializeField] GameObject rewardWindow;
    [SerializeField] GameObject essentialUI;

    [SerializeField] TextMeshProUGUI[] choiceButtonTexts;

    [SerializeField] float fadeTime = 2;
    [SerializeField] List<GameObject> possibleRewards;
    [SerializeField] List<GameObject> rewardsReceived;
    #endregion

    #region private fields
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

    public void OpenRewardWindow()
    {
        if (currentRewarWindowCoroutine != null)
            StopCoroutine(currentRewarWindowCoroutine);

        currentRewarWindowCoroutine = StartCoroutine(ShowCoroutine());
    }

    [ButtonMethod]
    public void GiveReward()
    {
        rewards = PickThreeRewards();
        string currentText;

        for (int i = 0; i < choiceButtonTexts.Length; i++)
        {
            if (rewards[i].TryGetComponent(out Ability ability))
                currentText = ability.AbilitySO.abilityTitel;
            else
            {
                Debug.Log(rewards[i].name + " has no Ability Script");
                currentText = "Random";
            }

            choiceButtonTexts[i].text = currentText;
        }

        OpenRewardWindow();
    }

    public GameObject[] PickThreeRewards()
    {
        GameObject[] rewards = new GameObject[3];
        rewards[0] = GetRandomReward();
        rewards[1] = GetRandomReward(rewards[0]);
        rewards[2] = GetRandomReward(rewards[0], rewards[1]);

        return rewards;
    }

    public GameObject GetRandomReward(GameObject priorChoice1 = null, GameObject priorChoice2 = null)
    {
        GameObject randomObject = possibleRewards[Random.Range(0, possibleRewards.Count)];

        if (randomObject == null)
            GetRandomReward();
        if (rewardsReceived.Contains(randomObject))
            return GetRandomReward();
        if (randomObject == priorChoice1)
            return GetRandomReward();
        if (randomObject == priorChoice2)
            return GetRandomReward();

        return randomObject;
    }

    public void RewardPicked(int buttonID)
    {
        AbilitySlotManager.Instance.AddNewAbility(rewards[buttonID]);
        rewardsReceived.Add(rewards[buttonID]);
        Close();
    }

    [ButtonMethod]
    public void Close()
    {
        //PauseManager.Instance.PauseLogic(false);

        if (currentRewarWindowCoroutine != null)
            StopCoroutine(currentRewarWindowCoroutine);

        currentRewarWindowCoroutine = StartCoroutine(HideCoroutine());
    }
    IEnumerator ShowCoroutine()
    {
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
        rewardWindowCanvasGroup.alpha = 1;
        essentialUICanvasGroup.alpha = 0;
    }

    IEnumerator HideCoroutine()
    {
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
    }
}