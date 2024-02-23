using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region serialized fields

    public float points;
    [Space(10)]
    public float playerHP;
    [Space(10)]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float deceleration = 10f;
    [Space(10)]
    [SerializeField] bool isSprinting;
    [SerializeField, Tooltip("Multiplies Move Speed by Sprint Speed")] float sprintSpeed = 2f;
    [Space(10)]
    [SerializeField] bool isDashing;
    [SerializeField, Tooltip("If true, the Player can Dash through collider")] bool colliderDash;
    [SerializeField, Tooltip("Distance to Dash")] float dashPower = 20f;
    [SerializeField] float dashingTime = 0.2f;
    [SerializeField] float dashingCooldown = 1f;

    #endregion

    #region private fields

    PlayerInputActions playerControls;
    [HideInInspector] public Vector3 playerPosition;
    Vector2 moveDir = Vector2.zero;
    Vector2 currentVelocity = Vector2.zero;
    InputAction move;
    Collider2D col;
    Rigidbody2D rb;
    NavMeshAgent playerAgent;
    float damage = 5;

    bool canDash = true;
    #endregion

    void Awake()
    {
        playerControls = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerAgent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        move = playerControls.Player.Move;

        playerControls.Player.Sprint.performed += ctx => isSprinting = true;
        playerControls.Player.Sprint.canceled += ctx => isSprinting = false;

        playerControls.Player.Dash.performed += ctx => StartCoroutine(Dash());

        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        if (playerHP <= 0) playerHP = 0;
        playerPosition = transform.position;
        if (isDashing) return;
        moveDir = move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        Movement();
    }

    private void Movement()
    {
        Vector2 targetVelocity = new Vector2(moveDir.x, moveDir.y).normalized * (moveSpeed * (isSprinting ? sprintSpeed : 1));

        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);

        if (currentVelocity.magnitude < 0.1f) currentVelocity = Vector2.zero;

        rb.velocity = currentVelocity;

        Flip();
    }

    IEnumerator Dash()
    {
        if (!canDash || isDashing) yield break;
        if (colliderDash) col.enabled = false;

        canDash = false;
        isDashing = true;


        Vector2 dashDirection = moveDir;

        dashDirection.Normalize();

        rb.velocity = dashDirection * dashPower;

        yield return new WaitForSeconds(dashingTime);

        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;
        col.enabled = true;
    }

    void Flip()
    {
        if (moveDir.x > 0) // Moving right
            gameObject.transform.localScale = new Vector3(1, 1, 1);

        else if (moveDir.x < 0) // Moving left
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Damage"))
        {
            playerHP -= damage;
        }
    }
}