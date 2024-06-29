using TMPro;
using UnityEngine;

/// <summary> Manages goals, resets and the ball </summary>
public class MinigameManager : MonoBehaviour
{
    #region Serialized
    [field: SerializeField] public CharSwitch CharSwitchManager { get; private set; }
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

    #region Non Serialized
    public static MinigameManager Instance;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    public void Goal(int goalID, GoalParticles goalTracker = null)
    {
        bool goalInLeft = goalID == 0;

        if (goalInLeft)
            pointCounter.y += 1;
        else
            pointCounter.x += 1;

        UpdateCounter();
        SoundManager.Instance.PlaySound(SoundType.GoalShot);

        pukRB.velocity = Vector2.zero;
        pukRB.transform.position = goalInLeft ? ballResetRight.position : ballResetLeft.position;


        //Returns if no side won yet
        if (!(pointCounter.x == pointsTilWin || pointCounter.y == pointsTilWin)) return;

        TournamentManager.Instance.SideWon(goalID == 0 ? 1 : 0);
        goalTracker?.WonParticle();
        ResetInternal();
    }

    public void ResetArena()
    {
        ResetInternal();
        UpdateCounter();
    }

    public void ResetInternal()
    {
        pointCounter = new();
        Puk.position = pukResetPos.position;
    }

    void UpdateCounter()
    {
        rightCounterTxt.text = pointCounter.y.ToString();
        leftCounterTxt.text = pointCounter.x.ToString();
    }

}