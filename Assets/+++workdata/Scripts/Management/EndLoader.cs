using MyBox;
using System.Collections;
using UnityEngine;

/// <summary> Used as a transition to the end scene </summary>
public class EndLoader : MonoBehaviour
{
    #region Serialized
    [SerializeField] string toEndDialogue = "toEnd";
    [SerializeField] float transitionEndTime = 2f;
    [field: SerializeField] public Vector3 QinuEndPos { get; private set; }
    [field: SerializeField] public bool WonGame { get; private set; }
    #endregion

    #region Non Serialized
    public static EndLoader Instance;
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

        if (value != TournamentManager.Instance.WinPoints && value != -TournamentManager.Instance.WinPoints) return;
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