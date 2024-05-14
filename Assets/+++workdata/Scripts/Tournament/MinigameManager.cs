using TMPro;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    #region serialized fields
    public static MinigameManager Instance;
    [SerializeField] float pointsTilWin = 5;
    [SerializeField] Vector2 pointCounter;
    [field: SerializeField] public Transform Puk { get; private set; }
    [SerializeField] Rigidbody2D pukRB;
    [field: SerializeField] public Transform ArenaMiddle { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosLeft { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosRight { get; private set; }

    [SerializeField] Transform ballResetLeft;
    [SerializeField] Transform ballResetRight;
    [SerializeField] TextMeshProUGUI leftCounterTxt;
    [SerializeField] TextMeshProUGUI rightCounterTxt;
    [SerializeField] Transform pukResetPos;
    #endregion

    #region private fields

    #endregion

    void Awake()
    {
        Instance = this;
    }

    public void Goal(int goalID, GoalTracker goalTracker = null)
    {
        if (goalID == 0)
        {
            pointCounter.y += 1;
            rightCounterTxt.text = pointCounter.y.ToString();
        }
        else
        {
            pointCounter.x += 1;
            leftCounterTxt.text = pointCounter.x.ToString();
        }

        pukRB.velocity = Vector2.zero;
        pukRB.transform.position = goalID == 0 ? ballResetLeft.position : ballResetRight.position;

        if (pointCounter.x == pointsTilWin)
        {
            TournamentManager.Instance.SideWon(1);
            ResetArena();
            if (goalTracker != null)
                goalTracker.WonParticle();
        }
        else if (pointCounter.y == pointsTilWin)
        {
            TournamentManager.Instance.SideWon(0);
            ResetArena();
            if (goalTracker != null)
                goalTracker.WonParticle();
        }
    }

    void ResetArena()
    {
        pointCounter = new();
        Puk.position = pukResetPos.position;
        rightCounterTxt.text = pointCounter.y.ToString();
        leftCounterTxt.text = pointCounter.x.ToString();
    }
}