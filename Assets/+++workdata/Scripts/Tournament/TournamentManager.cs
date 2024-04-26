using MyBox;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TournamentManager : MonoBehaviour
{
    public enum GameState
    {
        InGame,
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
    [field: SerializeField] public float RoundAmount { get; private set; }
    [field: SerializeField] public GameState gameState { get; private set; }
    [SerializeField] float RoundsTilWin = 5;
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    [field: SerializeField] public GameMode CurrentGameMode { get; private set; }
    [field: SerializeField] public List<GameObject> rightPlayers { get; private set; }
    [field: SerializeField] public List<GameObject> leftPlayers { get; private set; }
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
        for (int i = 0; i < CharManager.Instance.CharNavs.Length; i++)
        {
            if (!AvailableChars.Contains(CharManager.Instance.CharNavs[i].gameObject))
                AvailableChars.Add(CharManager.Instance.CharNavs[i].gameObject);
        }

        for (int i = 0; i < AvailableChars.Count; i++)
        {
            CharStats.Add(new(AvailableChars[i]));
        }
    }

    [ButtonMethod]
    public void InitializeGame()
    {
        if (gameState == GameState.InGame) return;

        int a = RoundAmount switch
        {
            0 => RoundZero(),
            1 => FirstRound(),
            2 => SecondRound(),
            3 => ThirdRound(),
            _ => RandomCalcRound()
        };

        gameState = GameState.InGame;
        RoundAmount++;
    }

    void Calc1v1()
    {
        ClearSideLists();
        leftPlayers.Add(AvailableChars[0]);
        lastPlayed = GetLowestPlayRate(lastPlayed);
        rightPlayers.Add(lastPlayed);

        CharManager.Instance.PathGOTo(lastPlayed, GetRandomDefaultPos(1));
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

        leftPlayers.Add(third);
        rightPlayers.Add(second);
        rightPlayers.Add(first);

        CharManager.Instance.PathGOTo(third, GetRandomDefaultPos(0));
        CharManager.Instance.PathGOTo(second, GetRandomDefaultPos(1));
        CharManager.Instance.PathGOTo(first, GetRandomDefaultPos(1));
    }

    #region Rounds
    int RoundZero()
    {
        GameObject bodi = AvailableChars[2];
        CharManager.Instance.PathGOTo(bodi, GetRandomDefaultPos(1));
        CurrentGameMode = GameMode.Bodi;
        lastPlayed = bodi;
        leftPlayers.Add(AvailableChars[0]);
        rightPlayers.Add(bodi);
        return 0;
    }
    int FirstRound()
    {
        CurrentGameMode = GameMode.a1v1;

        Calc1v1();
        return 0;
    }


    int SecondRound()
    {
        CurrentGameMode = GameMode.a1v1;
        Calc1v1();

        return 0;
    }
    int ThirdRound()
    {
        CurrentGameMode = GameMode.a2v2;
        Calc2v2();

        return 0;
    }
    int RandomCalcRound()
    {
        CurrentGameMode = (GameMode)Random.Range(1, 3);
        if (CurrentGameMode == GameMode.a1v1)
            Calc1v1();
        else if (CurrentGameMode == GameMode.a2v2)
            Calc2v2();

        return 0;
    }
    #endregion

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
    [field: SerializeField] public int TimesPlayed { get; private set; }
    [field: SerializeField] public int Wins { get; private set; }
    [field: SerializeField] public GameObject CharGO { get; private set; }
}
