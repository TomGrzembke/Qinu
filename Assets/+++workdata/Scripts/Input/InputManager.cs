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

    [Header("Cursor Confinement settings")] [SerializeField]
    int deltaSpeedModifier = 20;

    [SerializeField] int edgeThreshold = 50;

    public static InputManager Instance;

    const int CenterGraceDistance = 2;
    readonly Vector2 StartingVirtualCursorPoint = new(265, 270);

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

    private Vector2 additionalDelta;

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

    public void InitMainMenu()
    {
        ShowCursor();
    }
    
    public void InitGameplayScene()
    {
        MouseDelta = StartingVirtualCursorPoint;
        HideCursor();
    }

    void Movement(Vector2 direction) => MovementVec = direction;

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        additionalDelta = MouseDelta;
    }

    void Update()
    {
        CalculateMousePos();
    }

    void CalculateMousePos()
    {
        if (Input.touchCount > 0)
        {
            MousePos = GetCam().ScreenToWorldPoint(Input.GetTouch(0).position);
            usedTouch = true;
            return;
        }

        if (usedTouch) return;

        CalculateMouseDelta();
        MousePos = GetCam().ScreenToWorldPoint(MouseDelta);
    }

    void CalculateMouseDelta()
    {
        if (!Cursor.visible)
        {
            additionalDelta.x += Input.GetAxis("Mouse X") * deltaSpeedModifier;
            additionalDelta.y += Input.GetAxis("Mouse Y") * deltaSpeedModifier;
            MouseDelta = additionalDelta;
            return;
        }

        if (IsCursorAtCenter()) return;
        MouseDelta = GetTrueMousePosition();
    }

    bool IsCursorAtCenter()
    {
        var newMouseDelta = GetTrueMousePosition();
        float width = Screen.width;
        float height = Screen.height;
        var middle = new Vector2(width / 2, height / 2);

        return Vector2.Distance(newMouseDelta, middle) < CenterGraceDistance;
    }

    bool IsCursorAtEdge()
    {
        Vector2 mousePos = GetTrueMousePosition();

        bool hitLeft = mousePos.x <= edgeThreshold;
        bool hitRight = mousePos.x >= Screen.width - edgeThreshold;
        bool hitBottom = mousePos.y <= edgeThreshold;
        bool hitTop = mousePos.y >= Screen.height - edgeThreshold;
        bool atEdge = hitLeft || hitRight || hitBottom || hitTop;

        return atEdge;
    }

    Vector2 GetTrueMousePosition()
    {
        return Mouse.current.position.ReadValue();
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


    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }
}