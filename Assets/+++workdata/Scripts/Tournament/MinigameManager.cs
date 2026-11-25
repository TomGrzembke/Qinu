using System;
using TMPro;
using UnityEngine;

/// <summary> Manages goals, resets and the ball </summary>
public class MinigameManager : MonoBehaviour
{
    [field: SerializeField] public CharSwitch CharSwitchManager { get; private set; }
    [SerializeField] float pointsTilWin = 5;
    [SerializeField] Vector2 pointCounter;
    [field: SerializeField] public Transform Puk { get; private set; }
    [field: SerializeField] public Collider2D PukCollider { get; private set; }
    [field: SerializeField] public Transform ArenaMiddle { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosLeft { get; private set; }
    [field: SerializeField] public Transform[] DefaultPosRight { get; private set; }
    [field: SerializeField] public Transform DespawnPos { get; private set; }
    [field: SerializeField] public GameObject Cage { get; private set; }
    [field: SerializeField] public Transform LeftGoalStart { get; private set; }
    [field: SerializeField] public Transform LeftGoalEnd { get; private set; }
    [field: SerializeField] public Transform RightGoalStart { get; private set; }
    [field: SerializeField] public Transform RightGoalEnd { get; private set; }

    public Rigidbody2D PukRB => pukRB;

    [SerializeField] Transform ballResetLeft;
    [SerializeField] Transform ballResetRight;
    [SerializeField] TextMeshProUGUI leftCounterTxt;
    [SerializeField] TextMeshProUGUI rightCounterTxt;
    [SerializeField] Rigidbody2D pukRB;
    [SerializeField] Transform pukResetPos;
    [SerializeField] float[] ballSFXSpeed;

    [SerializeField] SlowMoDataSo goalSlowMo;
    [SerializeField] SlowMoDataSo matchEndSlowMo;

    public static MinigameManager Instance;

    public static event Action<Vector2> OnGoalShot;

    public bool TryGetGoalMiddle(bool isRight, out Vector2 goalMiddle)
    {
        Transform goalStart = isRight ? RightGoalStart : LeftGoalStart;
        Transform goalEnd = isRight ? RightGoalEnd : LeftGoalEnd;
        goalMiddle = Vector2.zero;

        if (!goalStart || !goalEnd) return false;

        goalMiddle = Vector2.Lerp(goalStart.position, goalEnd.position, 0.5f);
        return true;
    }

    void Awake()
    {
        Instance = this;
    }

    public void Goal(int goalID, GoalParticles goalTracker = null)
    {
        bool goalInLeft = goalID == 0;

        if (goalInLeft)
        {
            pointCounter.y += 1;
        }
        else
        {
            pointCounter.x += 1;
        }

        OnGoalShot?.Invoke(pointCounter);

        UpdateCounter();

        GoalSound();

        pukRB.linearVelocity = Vector2.zero;
        pukRB.transform.position = goalInLeft ? ballResetRight.position : ballResetLeft.position;

        PlaySlowMo();

        //Returns if no side won yet
        if (!(pointCounter.x == pointsTilWin || pointCounter.y == pointsTilWin)) return;

        TournamentManager.Instance.SideWon(goalID == 0 ? 1 : 0);
        goalTracker?.WonParticle();
        ResetInternal();
        AbilitySlotManager.Instance.ResetCooldowns();
    }

    public void SetPointsTilWin(int value)
    {
        pointsTilWin = value;
    }

    /// <summary> Depends on rb velocity</summary>
    void GoalSound()
    {
        float ballSpeed = pukRB.linearVelocity.magnitude;

        if (ballSpeed < ballSFXSpeed[0])
        {
            SoundManager.Instance.PlaySound(SoundType.GoalShotSoft);
        }
        else if (ballSpeed < ballSFXSpeed[1])
        {
            SoundManager.Instance.PlaySound(SoundType.GoalShotMiddle);
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundType.GoalShotHard);
        }
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

    public void CageBall()
    {
        Puk.position = pukResetPos.position;
        Cage.SetActive(true);
    }

    public void ReleaseBall()
    {
        Cage.SetActive(false);
    }

    void UpdateCounter()
    {
        rightCounterTxt.text = pointCounter.y.ToString();
        leftCounterTxt.text = pointCounter.x.ToString();
    }

    public void PlaySlowMo()
    {
        TimeScaler.Instance.PlaySlowMo(goalSlowMo);
    }

    public void PlayStrongSlowMo()
    {
        TimeScaler.Instance.PlaySlowMo(matchEndSlowMo);
    }


    public int GetLeftSideGoals()
    {
        return (int)pointCounter.x;
    }

    public int GetRightSideGoals()
    {
        return (int)pointCounter.y;
    }
}
