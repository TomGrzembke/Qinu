using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    PlayerInputActions input;

    public Vector2 MousePos => mousePos;
    [SerializeField] Vector2 mousePos;

    Camera cam;
    public Vector2 MovementVec => movementVec;
    [SerializeField] Vector2 movementVec;

    public InputAction moveAction
    {
        get;
        private set;
    }

    public bool HasMoveInput => movementVec.magnitude > 0 || leftclickAction.IsPressed() || rightClickAction.IsPressed();

    public InputAction leftclickAction
    {
        get;
        private set;
    }

    public InputAction rightClickAction
    {
        get;
        private set;
    }

    void Awake()
    {
        Instance = this;
        input = new();

        moveAction = input.Player.Move;
        moveAction.performed += ctx => Movement(ctx.ReadValue<Vector2>().normalized);
        moveAction.canceled += ctx => Movement(ctx.ReadValue<Vector2>().normalized);

        leftclickAction = input.Player.LeftClick;
        rightClickAction = input.Player.RightClick;
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Movement(Vector2 direction) => movementVec = direction;

    void Update()
    {
        if (!cam) 
            cam = Camera.main;

        if (cam)
            mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    /// <summary> Takes a Method and an Inputaction to subscribe them</summary>
    /// <param name="method"></param>
    public void SubscribeTo(Action<InputAction.CallbackContext> method, InputAction inputAction)
    {
        inputAction.performed += ctx => method(ctx);
        inputAction.canceled += ctx => method(ctx);
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
