using UnityEngine;

/// <summary>
/// Điều khiển nhân vật Player: di chuyển, nhảy và dash (lướt).
/// Sử dụng Input Manager cũ (không cần New Input System wrapper).
/// Phím: A/D hoặc Arrow Left/Right = di chuyển | Space = nhảy | Shift = dash | double-tap A/D = dash
/// Yêu cầu: Rigidbody2D, BoxCollider2D, Animator.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Di chuyển")]
    public float moveSpeed = 5f;

    [Header("Nhảy")]
    public float jumpForce = 15f;

    [Header("Kiểm tra mặt đất")]
    public Transform groundcheck;
    public LayerMask groundlayer;
    public float groundCheckRadius = 0.2f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.4f;
    public float doubleTapTime = 0.25f;
    public float dashCooldown = 0.5f;

    // --- Trạng thái runtime (hiện trong Inspector để debug) ---
    public bool isGrounded;
    public bool isDashing;
    public float moveInput;
    public bool isFacingRight = true;

    // Double-tap để dash
    public float lastLeftTapTime  = -1f;
    public float lastRightTapTime = -1f;
    public float lastDashTime     = float.NegativeInfinity;

    // --- Component references (tự lấy trong Awake, hoặc gán qua Inspector) ---
    public Animator animator;
    public Rigidbody2D rb;

    // Nội bộ
    private float _dashTimer;
    private float _dashDirection;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (rb       == null) rb       = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    // -------------------------------------------------------------------------
    // Update: đọc input và xử lý logic
    // -------------------------------------------------------------------------

    private void Update()
    {
        // --- Kiểm tra mặt đất ---
        if (groundcheck != null)
            isGrounded = Physics2D.OverlapCircle(groundcheck.position, groundCheckRadius, groundlayer);
        else
            isGrounded = false;

        // --- Đọc input di chuyển ngang ---
        float rawX = Input.GetAxisRaw("Horizontal");
        moveInput = rawX;

        // Phát hiện double-tap để dash
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - lastRightTapTime <= doubleTapTime)
                TryDash(1f);
            lastRightTapTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - lastLeftTapTime <= doubleTapTime)
                TryDash(-1f);
            lastLeftTapTime = Time.time;
        }

        // Dash bằng Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            float dir = (rawX != 0f) ? Mathf.Sign(rawX) : (isFacingRight ? 1f : -1f);
            TryDash(dir);
        }

        // --- Jump ---
        if (Input.GetButtonDown("Jump") && isGrounded && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- Đếm thời gian dash ---
        if (isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
                isDashing = false;
        }

        // --- Cập nhật hướng nhìn ---
        if (moveInput > 0.01f && !isFacingRight) Flip();
        if (moveInput < -0.01f && isFacingRight)  Flip();

        // --- Animator parameters ---
        if (animator != null)
        {
            animator.SetBool("isrunning", Mathf.Abs(moveInput) > 0.01f && !isDashing);
            animator.SetBool("isjumping", !isGrounded && !isDashing);
            animator.SetBool("isdashing", isDashing);
        }
    }

    // -------------------------------------------------------------------------
    // FixedUpdate: áp dụng vật lý
    // -------------------------------------------------------------------------

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Dash: vận tốc ngang cố định, tắt trọng lực
            rb.gravityScale   = 0f;
            rb.linearVelocity = new Vector2(_dashDirection * dashSpeed, 0f);
        }
        else
        {
            rb.gravityScale   = 5f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    // -------------------------------------------------------------------------
    // Dash logic
    // -------------------------------------------------------------------------

    private void TryDash(float direction)
    {
        if (isDashing) return;
        if (Time.time - lastDashTime < dashCooldown) return;

        isDashing      = true;
        _dashTimer     = dashDuration;
        _dashDirection = direction;
        lastDashTime   = Time.time;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundcheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundcheck.position, groundCheckRadius);
    }
}
