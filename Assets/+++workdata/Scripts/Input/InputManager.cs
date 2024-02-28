using System;
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
        moveAction.performed += ctx => Movement(ctx.ReadValue<Vector2>());
        moveAction.canceled += ctx => Movement(ctx.ReadValue<Vector2>());
    }

    void Movement(Vector2 direction)
    {
        if (direction != Vector2.zero)
            movementVec = direction.normalized;
    }

    public void SubscribeToMove(Action<Vector2> method)
    {
        moveAction.performed += ctx => method(ctx.ReadValue<Vector2>());
        moveAction.canceled += ctx => method(ctx.ReadValue<Vector2>());
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
