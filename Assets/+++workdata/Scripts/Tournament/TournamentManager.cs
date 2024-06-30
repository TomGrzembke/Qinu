using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary> Manages what happens after a match in terms of clean up and next matches </summary>
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

    #region Serialized
    [field: SerializeField] public GameStateEnum GameState { get; private set; }
    [field: SerializeField] public GameMode CurrentGameMode { get; private set; }
    [field: SerializeField] public List<GameMode> FirstGameModes { get; private set; }
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> RightPlayers { get; private set; }
    [field: SerializeField] public List<GameObject> LeftPlayers { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    [field: SerializeField] public float WinPoints { get; private set; } = 5;

    #endregion

    #region Non Serialized
    public static TournamentManager Instance;
    public event Action<float> OnPlayerMatchEnd;
    GameObject lastPlayed;
    bool firstMatch = true;
    int roundAmount;
    #endregion

    void Awake() => Instance = this;

    void Start()
    {
        for (int i = 0; i < AvailableChars.Count; i++)
        {
            CharStats.Add(new(AvailableChars[i]));
        }
    }

    public void InitializeGame()
    {
        if (GameState == GameStateEnum.InGame) return;

        GameState = GameStateEnum.InGame;

        StartMode();

        roundAmount++;
    }

    void StartMode()
    {
        DetermineWhichMode();

        if (CurrentGameMode == GameMode.a1v1)
            Calc1v1();
        else if (CurrentGameMode == GameMode.a2v2)
            Calc2v2();
        else if (CurrentGameMode == GameMode.Bodi)
            BodiRound();
    }

    void DetermineWhichMode()
    {
        if (roundAmount < FirstGameModes.Count)
            CurrentGameMode = FirstGameModes[roundAmount];
        else
            CurrentGameMode = (GameMode)Random.Range(1, 3);
    }

    void Calc1v1()
    {
        ClearSideLists();
        LeftPlayerAdd();
        lastPlayed = GetLowestPlayRate(lastPlayed);

        var newChar = CharManager.Instance.InitializeChar(lastPlayed, true);
        AddToList(RightPlayers, newChar);
        SwitchChars();
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

    void BodiRound()
    {
        ClearSideLists();
        LeftPlayerAdd();

        GameObject bodi = AvailableChars[2];
        lastPlayed = bodi;

        bodi = CharManager.Instance.InitializeChar(bodi, true);
        AddToList(RightPlayers, bodi);
        SwitchChars();
    }

    void AddToList(List<GameObject> list, GameObject toAdd)
    {
        list.CleanList();
        if (!list.Contains(toAdd))
            list.Add(toAdd);
        list.CleanList();
    }

    List<GameObject> UseList(List<GameObject> list)
    {
        CleanList(list);
        return list;
    }

    void CleanList(List<GameObject> list)
    {
        list.CleanList();
    }

    void SwitchChars()
    {
        MinigameManager.Instance.CharSwitchManager.BlendColors(UseList(RightPlayers));
    }

    void ClearSideLists()
    {
        LeftPlayers.Clear();
        RightPlayers.Clear();
    }

    /// <param name="sideID">0 = left, 1 = right</param>
    public void SideWon(int sideID)
    {
        StartCoroutine(AfterGameCor(sideID));
    }

    IEnumerator AfterGameCor(int sideID)
    {
        MinigameManager.Instance.PlayStrongSlowMo();
        yield return new WaitUntil(() => MinigameManager.Instance.GetSlowMoFinished());

        SituationalDialogue.Instance.StartDialogue(UseList(RightPlayers)[0].name);

        MinigameManager.Instance.ResetInternal();
        MinigameManager.Instance.Cage.SetActive(true);

        GameState = GameStateEnum.AfterGame;

        if (!firstMatch)
            OnPlayerMatchEnd?.Invoke(UpdateCharStats(sideID).Wins);

        yield return new WaitForSeconds(.5f);
        yield return new WaitUntil(() => CheckOutOfInteraction());

        MinigameManager.Instance.ResetArena();
        GameState = GameStateEnum.OutOfGame;

        if (UseList(RightPlayers).Count > 0)
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

        if (CharStats[0].Wins != WinPoints)
            InitializeGame();
        MinigameManager.Instance.Cage.SetActive(false);
    }

    bool CheckOutOfInteraction()
    {
        return !DialogueController.Instance.InDialogue && !RewardWindow.Instance.InAbilitySelect;
    }

    CharacterStats UpdateCharStats(int sideWon)
    {
        CharacterStats left0Stats = null;
        CharacterStats right0Stats = null;

        if (LeftPlayers.Count > 0)
            left0Stats = GetCharacterStats(LeftPlayers[0]);

        if (UseList(RightPlayers).Count > 0)
            right0Stats = GetCharacterStats(UseList(RightPlayers)[0]);

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
        CleanList(LeftPlayers);
        CleanList(RightPlayers);

        if (LeftPlayers.Count > 1)
        {
            CharacterStats left1Stats = GetCharacterStats(LeftPlayers[1]);
            if (sideID == 0)
                left1Stats.Wins++;

            else if (sideID == 1)
                left1Stats.Wins--;

            left1Stats.TimesPlayed++;
        }

        if (RightPlayers.Count > 1)
        {
            CharacterStats right1Stats = GetCharacterStats(RightPlayers[1]);
            if (sideID == 0)
                right1Stats.Wins++;

            else if (sideID == 1)
                right1Stats.Wins--;

            right1Stats.TimesPlayed++;
        }

        try
        {
            LeftPlayers[1].GetComponent<NPCNav>().GoHome();
        }
        catch { print("left index 1 error"); }
        try
        {
            RightPlayers[1].GetComponent<NPCNav>().GoHome();
        }
        catch { print("right index 1 error"); }
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

    public void LeftPlayerAdd()
    {
        AddToList(LeftPlayers, AvailableChars[0]);
    }

    public void RightPlayerAdd(GameObject charGO)
    {
        AddToList(RightPlayers, charGO);
    }

    [ButtonMethod]
    public void WinGame()
    {
        SideWon(0);
    }

    [ButtonMethod]
    public void LooseGame()
    {
        SideWon(1);
    }
}

[System.Serializable]
public class CharacterStats
{
    public CharacterStats(GameObject charGO)
    {
        if (charGO == null) return;
        CharGO = charGO;
        Name = CharGO.name;
    }

    [HideInInspector] public string Name;
    public int TimesPlayed;
    public int Wins;
    [field: SerializeField] public GameObject CharGO { get; private set; }
}
