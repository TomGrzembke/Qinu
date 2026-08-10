using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Coordinates the optional boss offer without changing normal tournament progression.</summary>
public class OptionalBossFightManager : MonoBehaviour
{
    enum BossFlowState
    {
        Unavailable,
        WaitingForCleanup,
        Explaining,
        Choosing,
        Fighting,
        Finished
    }

    [SerializeField] GameObject choiceWindow;
    [SerializeField] GameObject notyPrefab;
    [SerializeField] GameObject anthonyPrefab;
    [SerializeField] string bossOfferDialogue = "BossOffer";
    [SerializeField] BossFlowState state;
    [SerializeField] TextMeshProUGUI retryText;
    [SerializeField] string retryMessage = "Retry: <b><color=#FF3B30>Noty</color></b>?";
    [SerializeField] GameObject spaceVisual;
    public static OptionalBossFightManager Instance { get; private set; }

    CanvasGroup choiceCanvasGroup;
    Button fightButton;
    Button passButton;
    GameObject anthonyInstance;
    Coroutine offerRoutine;

    void Awake()
    {
        Instance = this;
        CacheChoiceUI();
        SetChoiceVisible(false);
    }

    void Start()
    {
        if (TournamentManager.Instance)
        {
            TournamentManager.Instance.OnBossFightEnded += OnBossFightEnded;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (TournamentManager.Instance)
        {
            TournamentManager.Instance.OnRoundCleanupFinished -= ShowInitialBossOffer;
            TournamentManager.Instance.OnBossFightEnded -= OnBossFightEnded;
        }

        fightButton?.onClick.RemoveListener(FightNoty);
        passButton?.onClick.RemoveListener(PassBossFight);
    }

    /// <summary>Returns true when this manager takes responsibility for the normal win ending.</summary>
    public bool TryBeginBossOffer()
    {
        if (state != BossFlowState.Unavailable)
            return true;

        if (!CanRunBossFight())
            return false;

        state = BossFlowState.WaitingForCleanup;

        if (TournamentManager.Instance.IsResolvingRound)
        {
            TournamentManager.Instance.OnRoundCleanupFinished -= ShowInitialBossOffer;
            TournamentManager.Instance.OnRoundCleanupFinished += ShowInitialBossOffer;
        }
        else
        {
            ShowInitialBossOffer();
        }

        return true;
    }

    bool CanRunBossFight()
    {
        bool hasRequiredReferences = choiceWindow
            && notyPrefab
            && anthonyPrefab
            && fightButton
            && passButton;
        bool hasRequiredManagers = TournamentManager.Instance
            && CharManager.Instance
            && MinigameManager.Instance
            && DialogueController.Instance;

        if (!hasRequiredReferences || !hasRequiredManagers)
        {
            Debug.LogError("Optional boss fight is missing a required reference. Loading the normal ending instead.", this);
            return false;
        }

        return true;
    }

    void ShowInitialBossOffer()
    {
        TournamentManager.Instance.OnRoundCleanupFinished -= ShowInitialBossOffer;

        if (offerRoutine != null)
            StopCoroutine(offerRoutine);

        offerRoutine = StartCoroutine(ShowInitialBossOfferCoroutine());
    }

    IEnumerator ShowInitialBossOfferCoroutine()
    {
        state = BossFlowState.Explaining;
        MinigameManager.Instance.CageBall();

        anthonyInstance = CharManager.Instance.InitializeChar(anthonyPrefab, true);
        DialogueController.Instance.StartDialogue(bossOfferDialogue);

        yield return new WaitUntil(() => !DialogueController.Instance.InDialogue);

        offerRoutine = null;
        ShowChoice();
    }

    public void ShowChoice()
    {
        state = BossFlowState.Choosing;
        MinigameManager.Instance.CageBall();
        SetChoiceVisible(true);
        InputManager.Instance.ShowCursor();
    }

    void FightNoty()
    {
        if (state != BossFlowState.Choosing) return;

        SetChoiceVisible(false);
        InputManager.Instance.HideCursor();

        if (anthonyInstance && anthonyInstance.TryGetComponent(out NPCNav anthonyNavigation))
            anthonyNavigation.GoHome();

        state = BossFlowState.Fighting;
        if (!TournamentManager.Instance.StartBossFight(notyPrefab))
        {
            Debug.LogError("The Noty boss fight could not be started.", this);
            ShowChoice();
        }

        if (spaceVisual != null)
        {
            spaceVisual.SetActive(true);
        }
    }

    void PassBossFight()
    {
        if (state != BossFlowState.Choosing) return;

        state = BossFlowState.Finished;
        SetChoiceVisible(false);
        InputManager.Instance.HideCursor();
        EndLoader.Instance.LoadEnd();
    }

    void OnBossFightEnded(bool playerWon)
    {
        if (playerWon)
        {
            state = BossFlowState.Finished;
            EndLoader.Instance.LoadEnd();
            return;
        }

        // Noty stays in the arena. The next attempt reuses the same instance.

        if (retryText != null)
        {
            retryText.text = retryMessage;
        }

        ShowChoice();
    }

    void CacheChoiceUI()
    {
        if (!choiceWindow) return;

        choiceCanvasGroup = choiceWindow.GetComponent<CanvasGroup>();
        Button[] buttons = choiceWindow.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.name;
            if (buttonName.Contains("HECK", StringComparison.OrdinalIgnoreCase))
                fightButton = button;
            else if (buttonName.Contains("END", StringComparison.OrdinalIgnoreCase)
                || buttonName.Contains("PASS", StringComparison.OrdinalIgnoreCase))
                passButton = button;
        }

        fightButton?.onClick.AddListener(FightNoty);
        passButton?.onClick.AddListener(PassBossFight);

        if (!fightButton || !passButton)
            Debug.LogError("Boss choice needs one HECK YEAH button and one PASS/END button.", choiceWindow);
    }

    void SetChoiceVisible(bool visible)
    {
        if (!choiceWindow) return;

        choiceWindow.SetActive(visible);
        if (!choiceCanvasGroup) return;

        choiceCanvasGroup.alpha = visible ? 1f : 0f;
        choiceCanvasGroup.interactable = visible;
        choiceCanvasGroup.blocksRaycasts = visible;
    }
}
