using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TournamentManager : MonoBehaviour
{
    public enum GameState
    {
        InGame,
        AfterGame,
        Village
    }
    public enum GameMode
    {
        Bodi,
        a1v1,
        a2v2
    }

    #region serialized fields
    public static TournamentManager Instance;
    [field: SerializeField] public int RoundAmount { get; private set; }
    [SerializeField] float RoundsTilWin = 5;
    [field: SerializeField] public GameState gameState { get; private set; }
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    [field: SerializeField] public GameMode CurrentGameMode { get; private set; }
    [field: SerializeField] public List<GameMode> FirstGameModes { get; private set; }
    [field: SerializeField] public List<GameObject> rightPlayers { get; private set; }
    [field: SerializeField] public List<GameObject> leftPlayers { get; private set; }
    [SerializeField] float afterCombatTime = 3;
    [SerializeField] GameObject cage;
    #endregion

    #region private fields
    GameObject lastPlayed;
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

        InitializeGame();
    }

    [ButtonMethod]
    public void InitializeGame()
    {
        if (gameState == GameState.InGame) return;

        if (RoundAmount < FirstGameModes.Count)
            CustomRound(FirstGameModes[RoundAmount]);
        else
            RandomCalcRound();

        gameState = GameState.InGame;
        RoundAmount++;
    }

    void Calc1v1()
    {
        ClearSideLists();
        leftPlayers.Add(AvailableChars[0]);
        lastPlayed = GetLowestPlayRate(lastPlayed);

        GameObject newChar = CharManager.Instance.InitializeChar(lastPlayed, true);
        rightPlayers.Add(newChar);
    }

    void ClearSideLists()
    {
        leftPlayers.Clear();
        rightPlayers.Clear();
    }

    void Calc2v2()
    {
        ClearSideLists();
        leftPlayers.Add(AvailableChars[0]);

        GameObject first = GetLowestPlayRate();
        GameObject second = GetLowestPlayRate(first);
        GameObject third = GetLowestPlayRate(first, second);

        first = CharManager.Instance.InitializeChar(first, true);
        second = CharManager.Instance.InitializeChar(second, true);
        third = CharManager.Instance.InitializeChar(third, false);

        leftPlayers.Add(third);
        rightPlayers.Add(second);
        rightPlayers.Add(first);
    }

    #region Rounds
    void BodiRound()
    {
        GameObject bodi = AvailableChars[2];
        CurrentGameMode = GameMode.Bodi;
        lastPlayed = bodi;
        bodi = CharManager.Instance.InitializeChar(bodi, true);
        leftPlayers.Add(AvailableChars[0]);
        rightPlayers.Add(bodi);
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
    public void SideWon(int sideID)
    {
        StartCoroutine(AfterGameCor(sideID));
    }

    IEnumerator AfterGameCor(int sideID)
    {
        yield return new WaitForSeconds(.3f);
        cage.SetActive(true);
        gameState = GameState.AfterGame;
        yield return new WaitForSeconds(afterCombatTime);
        gameState = GameState.Village;

        CharacterStats left0Stats = GetCharacterStats(leftPlayers[0]);
        CharacterStats right0Stats = GetCharacterStats(rightPlayers[0]);

        if (sideID == 0)
            left0Stats.Wins += 1;

        else if (sideID == 1)
            right0Stats.Wins += 1;

        left0Stats.TimesPlayed += 1;
        right0Stats.TimesPlayed += 1;

        rightPlayers[0].GetComponent<NPCNav>().GoHome();

        if (CurrentGameMode == GameMode.a2v2)
            Cleanup2v2(sideID);

        yield return new WaitForSeconds(afterCombatTime);
        InitializeGame();
        cage.SetActive(false);
    }

    void Cleanup2v2(int sideID)
    {
        CharacterStats left1Stats = GetCharacterStats(leftPlayers[1]);
        CharacterStats right1Stats = GetCharacterStats(rightPlayers[1]);

        if (sideID == 0)
            left1Stats.Wins += 1;

        else if (sideID == 1)
            right1Stats.Wins += 1;

        left1Stats.TimesPlayed += 1;
        left1Stats.TimesPlayed += 1;

        leftPlayers[1].GetComponent<NPCNav>().GoHome();
        rightPlayers[1].GetComponent<NPCNav>().GoHome();
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
