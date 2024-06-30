using System.Collections;
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
    [SerializeField] float[] ballSFXSpeed;

    [Header("StrongSlowMo")]
    [SerializeField] AnimationCurve strongSlowMoCurve;
    [SerializeField] float strongSlowMoBlendTime = .85f;
    [SerializeField] float strongSlowMoBlendTimeScale = 0.05f;

    [Header("NormalSlowMo")]
    [SerializeField] AnimationCurve slowMoCurve;
    [SerializeField] float slowMoBlendTime = 0.3f;
    [SerializeField] float slowMoBlendTimeScale = .8f;
    #endregion

    #region Non Serialized
    public static MinigameManager Instance;
    Coroutine blendRoutine;
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

        GoalSound();

        pukRB.velocity = Vector2.zero;
        pukRB.transform.position = goalInLeft ? ballResetRight.position : ballResetLeft.position;

        PlaySlowMo();

        //Returns if no side won yet
        if (!(pointCounter.x == pointsTilWin || pointCounter.y == pointsTilWin)) return;

        TournamentManager.Instance.SideWon(goalID == 0 ? 1 : 0);
        goalTracker?.WonParticle();
        ResetInternal();
    }

    /// <summary> Depends on rb velocity</summary>
    void GoalSound()
    {
        float ballSpeed = pukRB.velocity.magnitude;

        if (ballSpeed < ballSFXSpeed[0])
            SoundManager.Instance.PlaySound(SoundType.GoalShotSoft);
        else if (ballSpeed < ballSFXSpeed[1])
            SoundManager.Instance.PlaySound(SoundType.GoalShotMiddle);
        else
            SoundManager.Instance.PlaySound(SoundType.GoalShotHard);
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

    public void PlaySlowMo()
    {
        if (blendRoutine != null)
            StopCoroutine(blendRoutine);

        blendRoutine = StartCoroutine(NormalBlendTimeScale());
    }

    public void PlayStrongSlowMo()
    {
        if (blendRoutine != null)
            StopCoroutine(blendRoutine);

        blendRoutine = StartCoroutine(StrongBlendTimeScale());
    }

    public bool GetSlowMoFinished()
    {
        return blendRoutine == null;
    }

    IEnumerator NormalBlendTimeScale()
    {
        float timeBlended = 0;

        while (timeBlended < slowMoBlendTime)
        {
            timeBlended += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1, slowMoBlendTimeScale, slowMoCurve.Evaluate(timeBlended / slowMoBlendTime));
            yield return null;
        }

        timeBlended = 0;

        while (timeBlended < slowMoBlendTime)
        {
            timeBlended += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(slowMoBlendTimeScale, 1, timeBlended / slowMoBlendTime);
            yield return null;
        }

        blendRoutine = null;
    }

    IEnumerator StrongBlendTimeScale()
    {
        float timeBlended = 0;

        while (timeBlended < strongSlowMoBlendTime)
        {
            timeBlended += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1, strongSlowMoBlendTimeScale, strongSlowMoCurve.Evaluate(timeBlended / strongSlowMoBlendTime));
            yield return null;
        }

        timeBlended = 0;

        while (timeBlended < strongSlowMoBlendTime)
        {
            timeBlended += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(strongSlowMoBlendTimeScale, 1, timeBlended / strongSlowMoBlendTime);
            yield return null;
        }

        blendRoutine = null;
    }
}