using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    PlayerInputActions input;

    public Vector2 MovementVec => movementVec;
    [SerializeField] Vector2 movementVec;
    InputAction moveAction;

    void Awake()
    {
        Instance = this;
        input = new();

        moveAction = input.Player.Move;
        moveAction.performed += ctx => Movement(ctx.ReadValue<Vector2>().normalized);
        moveAction.canceled += ctx => Movement(ctx.ReadValue<Vector2>().normalized);
    }

    void Movement(Vector2 direction) => movementVec = direction;

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
