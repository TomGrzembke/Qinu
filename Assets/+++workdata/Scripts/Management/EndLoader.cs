using MyBox;
using System.Collections;
using UnityEngine;

public class EndLoader : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Scenes sceneToLoad = Scenes.End;
    [SerializeField] string toEndDialogue = "toEnd";
    [SerializeField] float transitionEndTime = 2f;
    public static EndLoader Instance;
    #endregion
    [field: SerializeField] public Vector3 QinuEndPos { get; private set; }
    [field: SerializeField] public bool WonGame { get; private set; }
    #region private fields

    float currentScore;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return null;
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(OnValueChanged, true);
    }
    void OnDisable()
    {
        TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;
    }

    void OnValueChanged(float value)
    {
        currentScore = value;

        if (currentScore > 0) WonGame = true;
        else WonGame = false;

        if (value != TournamentManager.Instance.RoundsTilWin && value != -TournamentManager.Instance.RoundsTilWin) return;
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