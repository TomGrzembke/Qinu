using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    const int deltaSpeedModifier = 20;

    [SerializeField] int edgeThreshold = 50;
    [SerializeField] float virtualMouseYOffset = 3;
    
    public static InputManager Instance;

    const int CenterGraceDistance = 2;
    readonly Vector2 StartingVirtualCursorPoint = new(265, 270);

    PlayerInputActions input;
    Camera Cam;
    bool usedTouch;
    Vector2 additionalDelta;

    Volume postProcessVolume;
    LensDistortion lensDistortion;


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

    Camera GetCam()
    {
        if (Cam == null) Cam = Camera.main;

        return Cam;
    }

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
        postProcessVolume = FindObjectOfType<Volume>();
        postProcessVolume.profile.TryGet(out lensDistortion);
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

    public Vector2 GetDistortedMouseDelta()
    {
        var rawMousePos = MouseDelta;

        if (!lensDistortion.active || lensDistortion.intensity.value == 0)
        {
            return rawMousePos;
        }

        float width = Screen.width;
        float height = Screen.height;
        var aspect = width / height;

        var intensity = lensDistortion.intensity.value * 0.1f;
        var xMult = lensDistortion.xMultiplier.value;
        var yMult = -lensDistortion.yMultiplier.value + virtualMouseYOffset;
        var scale = lensDistortion.scale.value;
        var center = lensDistortion.center.value;

        var uv = new Vector2(rawMousePos.x / width, rawMousePos.y / height);
        var coord = (uv - center) * 2.0f;
        coord.x *= aspect;

        Vector2 guess = coord / scale;

        for (int i = 0; i < 2; i++)
        {
            var r2 = (guess.x * guess.x * xMult) + (guess.y * guess.y * yMult);
            var f = 1.0f + r2 * intensity;
            guess = (coord / scale) / f;
        }

        guess.x /= aspect;
        var finalUV = (guess * 0.5f) + center;

        return new Vector2(finalUV.x * width, finalUV.y * height);
    }
}