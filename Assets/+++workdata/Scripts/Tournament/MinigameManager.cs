using TMPro;
using UnityEngine;

/// <summary> Manages goals, resets and the ball </summary>
public class MinigameManager : MonoBehaviour
{
    #region Serialized
    [field: SerializeField] public CharSwitch CharSwitchManager { get; private set; } 
    public static MinigameManager Instance;
    [SerializeField] float pointsTilWin = 5;
    [SerializeField] Vector2 pointCounter;
    [field: SerializeField] public Transform Puk { get; private set; }
    [field: SerializeField] public Transform ArenaMiddle { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosLeft { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosRight { get; private set; }
    [field: SerializeField] public Transform DespawnPos { get; private set; }
    [field: SerializeField] public GameObject Cage { get; private set; } 

    [SerializeField] Transform ballResetLeft;
    [SerializeField] Transform ballResetRight;
    [SerializeField] TextMeshProUGUI leftCounterTxt;
    [SerializeField] TextMeshProUGUI rightCounterTxt;
    [SerializeField] Rigidbody2D pukRB;
    [SerializeField] Transform pukResetPos;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    public void Goal(int goalID, GoalParticles goalTracker = null)
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
            ResetInternal();
            if (goalTracker != null)
                goalTracker.WonParticle();
        }
        else if (pointCounter.y == pointsTilWin)
        {
            TournamentManager.Instance.SideWon(0);
            ResetInternal();
            if (goalTracker != null)
                goalTracker.WonParticle();
        }
    }

    public void ResetArena()
    {
        ResetInternal();
        rightCounterTxt.text = pointCounter.y.ToString();
        leftCounterTxt.text = pointCounter.x.ToString();
    }

    public void ResetInternal()
    {
        pointCounter = new();
        Puk.position = pukResetPos.position;
    }
}