using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TournamentManager : MonoBehaviour
{
    public enum GameStateEnum
    {
        InGame,
        AfterGame,
        OutOfGame
    }
    public enum GameMode
    {
        Bodi,
        a1v1,
        a2v2
    }

    #region public fields
    public static TournamentManager Instance;
    [field: SerializeField] public int RoundAmount { get; private set; }
    [field: SerializeField] public GameStateEnum GameState { get; private set; }
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    [field: SerializeField] public GameMode CurrentGameMode { get; private set; }
    [field: SerializeField] public List<GameMode> FirstGameModes { get; private set; }
    [field: SerializeField] public List<GameObject> RightPlayers { get; private set; }
    [field: SerializeField] public List<GameObject> LeftPlayers { get; private set; }
    public event Action<float> OnPlayerMatchEnd;
    public event Action<bool> OnMatchEnd;
    #endregion

    #region [SerializeField]
    [SerializeField] float afterCombatTime = 3;
    [field: SerializeField] public float RoundsTilWin { get; private set; } = 5;
    [SerializeField] List<string> afterCombatTalk;
    #endregion

    #region private fields
    int afterCombatTalkTimes = -1;
    GameObject lastPlayed;
    bool firstMatch = true;
    #endregion
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < CharManager.Instance.CharPrefabs.Length; i++)
        {
            if (!AvailableChars.Contains(CharManager.Instance.CharPrefabs[i].gameObject))
                AvailableChars.Add(CharManager.Instance.CharPrefabs[i].gameObject);
        }

        for (int i = 0; i < AvailableChars.Count; i++)
        {
            CharStats.Add(new(AvailableChars[i]));
        }

    }

    [ButtonMethod]
    public void InitializeGame()
    {
        if (GameState == GameStateEnum.InGame) return;

        if (RoundAmount < FirstGameModes.Count)
            CustomRound(FirstGameModes[RoundAmount]);
        else
            RandomCalcRound();

        GameState = GameStateEnum.InGame;
        RoundAmount++;

        OnMatchEnd?.Invoke(GameState == GameStateEnum.InGame);
    }

    [ButtonMethod]
    public void WinGame()
    {
        SideWon(0);
    }

    void Calc1v1()
    {
        ClearSideLists();
        AddToList(LeftPlayers, AvailableChars[0]);
        lastPlayed = GetLowestPlayRate(lastPlayed);

        var newChar = CharManager.Instance.InitializeChar(lastPlayed, true);
        AddToList(RightPlayers, newChar);
        SwitchChars();
    }

    void AddToList(List<GameObject> list, GameObject toAdd)
    {
        list.CleanList();
        if (!list.Contains(toAdd))
            list.Add(toAdd);
        list.CleanList();
    }

    void SwitchChars()
    {
        MinigameManager.Instance.CharSwitchManager.Calculate(RightPlayers);
    }

    void ClearSideLists()
    {
        LeftPlayers.Clear();
        RightPlayers.Clear();
    }

    void Calc2v2()
    {
        ClearSideLists();
        LeftPlayerAdd();

        var first = GetLowestPlayRate();
        var second = GetLowestPlayRate(first);
        var third = GetLowestPlayRate(first, second);

        first = CharManager.Instance.InitializeChar(first, true);
        second = CharManager.Instance.InitializeChar(second, true);
        third = CharManager.Instance.InitializeChar(third, false);

        AddToList(RightPlayers, first);
        AddToList(RightPlayers, second);
        AddToList(LeftPlayers, third);
        SwitchChars();
    }

    #region Rounds
    void BodiRound()
    {
        GameObject bodi = AvailableChars[2];
        CurrentGameMode = GameMode.Bodi;
        lastPlayed = bodi;
        bodi = CharManager.Instance.InitializeChar(bodi, true);
        ClearSideLists();
        LeftPlayerAdd();
        AddToList(RightPlayers, bodi);
        SwitchChars();
    }

    void CustomRound(GameMode gameMode)
    {
        CurrentGameMode = gameMode;

        if (CurrentGameMode == GameMode.a1v1)
            Calc1v1();
        else if (CurrentGameMode == GameMode.a2v2)
            Calc2v2();
        else
            BodiRound();
    }

    void RandomCalcRound()
    {
        CurrentGameMode = (GameMode)Random.Range(1, 3);
        if (CurrentGameMode == GameMode.a1v1)
            Calc1v1();
        else if (CurrentGameMode == GameMode.a2v2)
            Calc2v2();
    }

    #endregion

    /// <param name="sideID">0 = left, 1 = right</param>
    public void SideWon(int sideID)
    {
        StartCoroutine(AfterGameCor(sideID));
    }

    IEnumerator AfterGameCor(int sideID)
    {
        yield return new WaitForSeconds(.3f);

        if (afterCombatTalkTimes + 1 < afterCombatTalk.Count)
            DialogueController.Instance.StartDialogue(afterCombatTalk[++afterCombatTalkTimes]);

        MinigameManager.Instance.ResetInternal();
        MinigameManager.Instance.Cage.SetActive(true);

        GameState = GameStateEnum.AfterGame;
        OnMatchEnd?.Invoke(GameState == GameStateEnum.InGame);
        OnPlayerMatchEnd?.Invoke(UpdateCharStats(sideID).Wins);

        yield return new WaitForSeconds(.5f);
        yield return new WaitUntil(() => CheckOutOfInteraction());

        MinigameManager.Instance.ResetArena();
        GameState = GameStateEnum.OutOfGame;

        if (RightPlayers.Count > 0)
            RightPlayers[0].GetComponent<NPCNav>().GoHome();

        if (CurrentGameMode == GameMode.a2v2)
            Cleanup2v2(sideID);

        yield return new WaitUntil(() => CheckOutOfInteraction());

        if (!firstMatch)
            if (sideID == 0)
                RewardWindow.Instance.GiveReward();
            else
                RewardWindow.Instance.RemoveReward();

        firstMatch = false;

        yield return new WaitUntil(() => CheckOutOfInteraction());

        InitializeGame();
        MinigameManager.Instance.Cage.SetActive(false);
    }

    bool CheckOutOfInteraction()
    {
        return !DialogueController.Instance.InDialogue && !RewardWindow.Instance.InAbilitySelect;
    }

    CharacterStats UpdateCharStats(int sideWon)
    {
        CharacterStats left0Stats = GetCharacterStats(LeftPlayers[0]);
        CharacterStats right0Stats = null;

        if (RightPlayers.Count > 0)
            right0Stats = GetCharacterStats(RightPlayers[0]);

        if (firstMatch)
            return left0Stats;

        left0Stats.Wins += sideWon == 0 ? 1 : -1;
        left0Stats.TimesPlayed++;

        firstMatch = false;

        if (right0Stats != null)
        {
            right0Stats.Wins += sideWon == 0 ? 1 : -1;
            right0Stats.TimesPlayed++;
        }

        return left0Stats;
    }

    void Cleanup2v2(int sideID)
    {
        CharacterStats left1Stats = GetCharacterStats(LeftPlayers[1]);
        CharacterStats right1Stats = GetCharacterStats(RightPlayers[1]);

        if (sideID == 0)
        {
            left1Stats.Wins++;
            right1Stats.Wins--;
        }
        else if (sideID == 1)
        {
            left1Stats.Wins--;
            right1Stats.Wins++;
        }

        left1Stats.TimesPlayed++;
        right1Stats.TimesPlayed++;

        LeftPlayers[1].GetComponent<NPCNav>().GoHome();
        RightPlayers[1].GetComponent<NPCNav>().GoHome();
    }

    /// <returns>left = 0, right = 1</returns>
    Vector3 GetRandomDefaultPos(int sideIndex)
    {
        return GetRandomDefaultTrans(sideIndex).position;
    }

    /// <returns>left = 0, right = 1</returns>
    public Transform GetRandomDefaultTrans(int sideIndex)
    {
        if (sideIndex == 0)
            return MinigameManager.Instance.DefaultPosLeft[Random.Range(0, MinigameManager.Instance.DefaultPosLeft.Length)];
        else
            return MinigameManager.Instance.DefaultPosRight[Random.Range(0, MinigameManager.Instance.DefaultPosRight.Length)];
    }

    GameObject GetLowestPlayRate(GameObject exclude = null, GameObject exclude2 = null)
    {
        GameObject lowestPlayRateChar = new();
        int lowestTimesPlayed = 100;
        for (int i = 1; i < CharStats.Count; i++)
        {
            if (CharStats[i].TimesPlayed == lowestTimesPlayed)
                if (Random.Range(0, 2) == 0) continue;

            if (CharStats[i].TimesPlayed <= lowestTimesPlayed)
            {
                if (exclude == CharStats[i].CharGO || exclude2 == CharStats[i].CharGO) continue;

                lowestTimesPlayed = CharStats[i].TimesPlayed;
                lowestPlayRateChar = CharStats[i].CharGO;
            }
        }
        return lowestPlayRateChar;
    }

    CharacterStats GetCharacterStats(GameObject goSearched)
    {
        CharacterStats characterStats = new(goSearched);
        for (int i = 0; i < CharStats.Count; i++)
        {
            if (CharStats[i].CharGO == goSearched)
                characterStats = CharStats[i];
        }

        return characterStats;
    }

    public void RegisterOnPlayerMatchEnd(Action<float> callback, bool getInstantCallback = false)
    {
        OnPlayerMatchEnd += callback;
        if (getInstantCallback)
            callback(CharStats[0].Wins);
    }
    public void RegisterOnMatchEnd(Action<bool> callback, bool getInstantCallback = false)
    {
        OnMatchEnd += callback;
        if (getInstantCallback)
            callback(GameStateEnum.InGame == GameState);
    }
    public void LeftPlayerAdd()
    {
        AddToList(LeftPlayers, AvailableChars[0]);
    }
    public void RightPlayerAdd(GameObject charGO)
    {
        AddToList(RightPlayers, charGO);
    }
}

[System.Serializable]
public class CharacterStats
{
    public CharacterStats(GameObject charGO)
    {
        CharGO = charGO;
        Name = CharGO.name;
    }

    [HideInInspector] public string Name;
    public int TimesPlayed;
    public int Wins;
    [field: SerializeField] public GameObject CharGO { get; private set; }
}
