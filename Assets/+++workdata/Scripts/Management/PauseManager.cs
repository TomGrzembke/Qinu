using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    PlayerInputActions inputActions;
    [SerializeField] GameObject objectToToggle;
    bool paused;
    CursorLockMode lastCursorState;

    void Awake()
    {
        Instance = this;
        inputActions = new();

        inputActions.Player.Pause.performed += ctx => PauseLogic();

        lastCursorState = Cursor.lockState;
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
            lastCursorState = Cursor.lockState;
            InputManager.Instance.ShowCursor();
            return;
        }

        if (lastCursorState != CursorLockMode.Locked) return;

        InputManager.Instance.HideCursor();
        lastCursorState = Cursor.lockState;
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