using UnityEngine;
using UnityEngine.Events;

public class DebugControls : MonoBehaviour
{
    #region serialized fields
    [SerializeField] UnityEvent slashEvent;
    [SerializeField] UnityEvent asteriskEvent;
    [SerializeField] UnityEvent num7;

    [SerializeField] GameObject[] toggleUIAsterisk;
    #endregion

    #region private fields
    PlayerInputActions inputActions;
    #endregion

    void Awake()
    {
        inputActions = new();
        inputActions.Player.Debug7.performed += ctx => CallNum7(num7);
        inputActions.Player.DebugAsterisk.performed += ctx => CallAsteriskEvent();
        inputActions.Player.DebugSlash.performed += ctx => CallSlashEvent();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }
    void OnDisable()
    {
        inputActions.Disable();
    }

    public void CallNum7(UnityEvent givenEvent)
    {
        givenEvent?.Invoke();
    }
    public void CallAsteriskEvent()
    {
        for (int i = 0; i < toggleUIAsterisk.Length; i++)
        {
            toggleUIAsterisk[i].SetActive(!toggleUIAsterisk[i].activeInHierarchy);
        }

        asteriskEvent?.Invoke();
    }
    public void CallSlashEvent()
    {
        slashEvent?.Invoke();
        GameStateManager.ReloadGameScene();
    }
}