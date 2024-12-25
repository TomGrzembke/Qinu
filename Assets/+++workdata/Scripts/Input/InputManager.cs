using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Serialized
    [field: SerializeField] public Vector2 MousePos { get; private set; }
    [field: SerializeField] public Vector2 MovementVec { get; private set; }
    [field: SerializeField] public InputAction MoveAction { get; private set; }
    [field: SerializeField] public InputAction MousePosAction { get; private set; }
    [field: SerializeField] public InputAction LeftclickAction { get; private set; }
    [field: SerializeField] public InputAction RightClickAction { get; private set; }
    [field: SerializeField] public InputAction Ability0Action { get; private set; }
    [SerializeField] Joystick joystickScript;
    [SerializeField] bool useJoystickDebug;

    #endregion

    #region Non Serialized
    public static InputManager Instance;
    PlayerInputActions input;
    Camera cam;
    public bool HasMoveInput => MovementVec.magnitude > 0 || LeftclickAction.IsPressed() || RightClickAction.IsPressed();
    public bool UsedTouch { get; private set; }

    #endregion


    void Awake()
    {
        Instance = this;
        input = new();

        MoveAction = input.Player.Move;
        MoveAction.performed += ctx => Movement(ctx.ReadValue<Vector2>().normalized);
        MoveAction.canceled += ctx => Movement(ctx.ReadValue<Vector2>().normalized);

        MousePosAction = input.Player.MousePos;

        LeftclickAction = input.Player.LeftClick;
        RightClickAction = input.Player.RightClick;
        Ability0Action = input.Player.Ability0;
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Movement(Vector2 direction) => MovementVec = direction;

    void MouseInput(Vector2 direction)
    {
        MousePos = cam.ScreenToWorldPoint(direction);
    }

    void Update()
    {
        if (!cam)
            cam = Camera.main;

        if (cam && !UsedTouch)
            MousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        MovementVec = joystickScript.Direction;
        if (Input.touchCount > 0 || useJoystickDebug)
        {
            //MousePos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
            UsedTouch = true;
            joystickScript.gameObject.SetActive(UsedTouch);
        }
    }

    /// <summary> Takes a Method and an Inputaction to subscribe them</summary>
    /// <param name="method"></param>
    public void SubscribeTo(Action<InputAction.CallbackContext> method, InputAction inputAction)
    {
        inputAction.performed += method;
        inputAction.canceled += method;
    }

    public void DesubscribeTo(Action<InputAction.CallbackContext> method, InputAction inputAction)
    {
        inputAction.performed -= method;
        inputAction.canceled -= method;
    }

    #region OnEnable/Disable
    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }
    #endregion
}
