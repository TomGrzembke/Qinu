using MyBox;
using System;
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
        a2v2,
        watch1v1
    }

    #region serialized fields
    [field: SerializeField] public float RoundAmount { get; private set; }
    [field: SerializeField] public GameState gameState { get; private set; }
    [SerializeField] float RoundsTilWin = 5;
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    #endregion

    #region private fields
    [field: SerializeField] public GameMode CurrentGameMode { get; private set; }

    #endregion
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
    public void StartGame()
    {
        if (gameState == GameState.InGame) return;

        int round = RoundAmount switch
        {
            0 => RoundZero(),
            1 => proc1(),
            2 => proc2(),
            3 => proc3(),
            _ => false
        };

        switch (RoundAmount)
        {
            case 0:
                RoundZero();
                break;
            case 1:
                FirstRound();

                break;
            case 2:
                CurrentGameMode = GameMode.a1v1;

                break;
            case 3:
                CurrentGameMode = GameMode.a2v2;

                break;
            case 4:
                CurrentGameMode = GameMode.watch1v1;
                break;
            default:
                int gameModeLength = Enum.GetValues(typeof(GameMode)).Length;
                CurrentGameMode = (GameMode)Random.Range(0, gameModeLength + 1);
                break;
        }

        gameState = GameState.InGame;
        RoundAmount++;
    }

    #region Rounds
    void RoundZero()
    {
        CharManager.Instance.PathGOTo(AvailableChars[1], MinigameManager.Instance.DefaultPosLeft
            [Random.Range(0, MinigameManager.Instance.DefaultPosLeft.Length)].position);
        CurrentGameMode = GameMode.Bodi;
    }
    void FirstRound()
    {
        CurrentGameMode = GameMode.a1v1;
    }
    #endregion

    /// <param name="sideIndex">left = 0, right = 1</param>
    Vector3 GetRandomDefaultPos(int sideIndex)
    {
        if (sideIndex == 0)
            return MinigameManager.Instance.DefaultPosLeft[Random.Range(0, MinigameManager.Instance.DefaultPosLeft.Length)].position;
        else
            return MinigameManager.Instance.DefaultPosRight[Random.Range(0, MinigameManager.Instance.DefaultPosRight.Length)].position;
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
