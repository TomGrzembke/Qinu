using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    public enum gameState
    {
        InGame,
        Over
    }

    #region serialized fields
    [field: SerializeField] public float RoundAmount { get; private set; } 
    [field: SerializeField] public gameState GameState { get; private set; }   
    #endregion

    #region private fields
    
    #endregion

    public void StartGame()
    {
        if (RoundAmount == 0)
        {

        }
        else if (RoundAmount == 1)
        {

        }
        else
        {

        }

        RoundAmount++;
    }
}