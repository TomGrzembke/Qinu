using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region serialized fields

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float deceleration = 10f;
    #endregion

    #region private fields
    PlayerInputActions playerControls;
    InputAction move;
    Vector2 moveDir;
    Vector2 currentVelocity;
    Rigidbody2D rb;
    #endregion

    void Awake()
    {
        playerControls = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        move = playerControls.Player.Move;

        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        moveDir = move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        Vector2 targetVelocity = new Vector2(moveDir.x, moveDir.y).normalized * moveSpeed;

        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);

        if (currentVelocity.magnitude < 0.1f) currentVelocity = Vector2.zero;

        rb.velocity = currentVelocity;

        Flip();
    }

    void Flip()
    {
        Vector3 localScale = transform.localScale;
        bool facingRight = localScale.x > 0;

        transform.localScale = localScale.SetX(facingRight ? (facingRight ? localScale.x : -localScale.x) : (facingRight ? -localScale.x : localScale.x));

        //if (moveDir.x > 0)
        //    transform.localScale = localScale.SetX(xScale < 0 ? xScale : -xScale);

        //else if (moveDir.x < 0)
        //    transform.localScale = localScale.SetX(xScale > 0 ? -xScale : xScale);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

    }
}