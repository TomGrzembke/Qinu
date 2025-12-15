using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary> Mostly used for a dialogue speed up or to get to a certain game state more quickly </summary>
public class DebugControls : MonoBehaviour
{
    #region Serialized
    [SerializeField] UnityEvent slashEvent;
    [SerializeField] UnityEvent asteriskEvent;
    [SerializeField] UnityEvent num7;

    [SerializeField] CanvasGroup[] cgToggleUIAsterisk;
    #endregion

    #region Non Serialized
    PlayerInputActions inputActions;
    #endregion

    void Awake()
    {
        inputActions = new();
        inputActions.Player.Debug7.performed += ctx => CallNum7(num7);
        inputActions.Player.DebugAsterisk.performed += ctx => CallAsteriskEvent();
        inputActions.Player.DebugSlash.performed += ctx => CallSlashEvent();
        inputActions.Player.Reset.performed += CallResetEvent;
    }

    void OnEnable()
    {
        inputActions.Enable();
    }
    void OnDisable()
    {
        inputActions.Disable();
        
        inputActions = new();
        inputActions.Player.Reset.performed -= CallResetEvent;
    }

    public void CallNum7(UnityEvent givenEvent)
    {
        givenEvent?.Invoke();
    }
    public void CallAsteriskEvent()
    {
        for (int i = 0; i < cgToggleUIAsterisk.Length; i++)
        {
            cgToggleUIAsterisk[i].alpha = cgToggleUIAsterisk[i].alpha == 0 ? 1 : 0;
        }

        asteriskEvent?.Invoke();
    }
    public void CallSlashEvent()
    {
        slashEvent?.Invoke();
    }

    
    public void CallResetEvent(InputAction.CallbackContext ctx)
    {
        GameStateManager.ResetGame();
    }
}