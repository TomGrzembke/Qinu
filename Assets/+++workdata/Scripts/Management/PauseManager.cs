using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    PlayerInputActions inputActions;
    [SerializeField] GameObject objectToToggle;
    bool paused;

    void Awake()
    {
        Instance = this;
        inputActions = new();

        inputActions.Player.Pause.performed += ctx => PauseLogic();
    }

    public void PauseLogic()
    {
        PauseLogic(!paused);
    }

    public void PauseLogic(bool _paused)
    {
        paused = _paused;
        Time.timeScale = paused ? 0 : 1; //Could slowmow blend this 

        objectToToggle.SetActive(paused);

        HandleCursor();
    }

    void HandleCursor()
    {
        if (paused)
        {
            InputManager.Instance.ShowCursor();
            return;
        }

        if (SceneManager.GetSceneByBuildIndex((int)Scenes.MainMenu).isLoaded) return;

        InputManager.Instance.HideCursor();
    }

    public void OnEnable()
    {
        inputActions.Enable();
    }

    public void OnDisable()
    {
        inputActions.Disable();
    }
}