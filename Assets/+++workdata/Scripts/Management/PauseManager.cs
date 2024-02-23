using UnityEngine;

public class PauseManager : MonoBehaviour
{
    #region serialized fields
    public static PauseManager Instance;
    #endregion

    #region private fields
    PlayerInputActions inputActions;
    bool paused;
    #endregion

    void Awake()
    {
        Instance = this;
        inputActions = new();

        inputActions.Player.Pause.performed += ctx => PauseButton();
    }

    void PauseButton()
    {
        GameStateManager.OptionsWindow();
    }

    public void PauseLogic()
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
    }

    public void PauseLogic(bool condition)
    {
        paused = condition;
        Time.timeScale = paused ? 0 : 1;
    }

    #region OnEnable/Disable
    public void OnEnable()
    {
        inputActions.Enable();
    }

    public void OnDisable()
    {
        inputActions.Disable();
    }
    #endregion
}