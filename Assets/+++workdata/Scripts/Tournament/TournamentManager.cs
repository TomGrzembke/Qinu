using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    public enum GameState
    {
        InGame,
        Village
    }

    #region serialized fields
    [field: SerializeField] public float RoundAmount { get; private set; }
    [field: SerializeField] public GameState gameState { get; private set; }
    [SerializeField] float RoundsTilWin = 5;
    [field: SerializeField] public List<CharacterStats> CharStats { get; private set; }
    [field: SerializeField] public List<GameObject> AvailableChars { get; private set; }
    #endregion

    #region private fields

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


        if (RoundAmount == 0)
        {
            CharManager.Instance.PathTo(AvailableChars[2], new(8, 5));
        }
        else if (RoundAmount == 1)
        {

        }
        else
        {

        }

        gameState = GameState.InGame;
        RoundAmount++;
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
