using MyBox;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> Used as a transition to the end scene </summary>
public class EndLoader : MonoBehaviour
{
    [SerializeField] string toEndDialogue = "toEnd";
    [SerializeField] float transitionEndTime = 2f;
    [field: SerializeField] public Vector3 QinuEndPos { get; private set; }
    [field: SerializeField] public bool WonGame { get; private set; }


    public static EndLoader Instance;
    float currentScore;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += TrySubscribeGameEndListener;
    }

    void TrySubscribeGameEndListener(Scene scene, LoadSceneMode mode)
    {
        if (TournamentManager.Instance == null) return;

        TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(OnValueChanged, false);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= TrySubscribeGameEndListener;
        TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;
    }

    void OnValueChanged(float value)
    {
        currentScore = value;

        if (currentScore > 0) WonGame = true;
        else WonGame = false;

        if (value != -TournamentManager.Instance.LoosePoints && value != TournamentManager.Instance.WinPoints) return;

        LoadEnd();
    }

    [ButtonMethod]
    public void LoadEnd()
    {
        QinuEndPos = TournamentManager.Instance.LeftPlayers[0].transform.position;

        DialogueController.Instance.StartDialogue(toEndDialogue);

        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        yield return new WaitUntil(() => !DialogueController.Instance.InDialogue);

        yield return new WaitForSeconds(transitionEndTime);
        SceneLoader.Instance.LoadSceneViaIndex(Scenes.End);
        SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.Gameplay);
    }


}