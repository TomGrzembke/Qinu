using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public Vector2 MousePos { get; private set; }
    [field: SerializeField] public Vector2 MouseDelta { get; private set; }
    [field: SerializeField] public Vector2 MovementVec { get; private set; }
    [field: SerializeField] public InputAction MoveAction { get; private set; }
    [field: SerializeField] public InputAction MousePosAction { get; private set; }
    [field: SerializeField] public InputAction LeftclickAction { get; private set; }
    [field: SerializeField] public InputAction RightClickAction { get; private set; }
    [field: SerializeField] public InputAction Ability0Action { get; private set; }
    
    
    public static InputManager Instance;
    PlayerInputActions input;

    Camera Cam;
    Camera GetCam()
    {
        if (Cam == null) Cam = Camera.main;

        return Cam;
    }

    public bool HasMoveInput =>
        MovementVec.magnitude > 0 || LeftclickAction.IsPressed() || RightClickAction.IsPressed();

    bool usedTouch;
    
    
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

    void Movement(Vector2 direction) => MovementVec = direction;

    void MouseInput(Vector2 delta)
    {
        MouseDelta = delta;
        MousePos = GetCam().ScreenToWorldPoint(delta);
    }

    void Update()
    {
        if (!usedTouch)
        {
            MouseDelta = Mouse.current.position.ReadValue();
            MousePos = GetCam().ScreenToWorldPoint(MouseDelta);
        }

        if (Input.touchCount > 0)
        {
            MousePos = GetCam().ScreenToWorldPoint(Input.GetTouch(0).position);
            usedTouch = true;
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