namespace DOAN.LegacyCombat {
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;

    [Header("Di chuyển")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 9.5f;
    public float turnSmoothTime = 0.08f;
    private float turnSmoothVelocity;

    [Header("Nhảy & Trọng lực")]
    public float gravity = -19.62f;
    public float jumpHeight = 2f;
    private Vector3 velocity;
    private bool isGrounded;

    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public float MoveSpeedRatio { get; private set; }

    private PlayerHealth playerHealth;
    private float gameStartTime;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead) return;

        if (GameUIManager.Instance != null && GameUIManager.Instance.CurrentState != GameState.Playing)
        {
            if (GameUIManager.Instance.CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                GameUIManager.Instance.ResumeGame();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameUIManager.Instance != null && GameUIManager.Instance.CurrentState == GameState.Playing)
            {
                GameUIManager.Instance.PauseGame();
            }
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && direction.magnitude >= 0.1f && vertical > 0;
        float currentSpeed = IsSprinting ? sprintSpeed : walkSpeed;

        IsMoving = direction.magnitude >= 0.1f;
        MoveSpeedRatio = IsSprinting ? 1f : (IsMoving ? 0.5f : 0f);

        if (animator != null)
        {
            animator.SetFloat("Speed", direction.magnitude * (IsSprinting ? 1.5f : 1f));
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (animator != null)
        {
            animator.SetBool("IsShooting", Input.GetButton("Fire1"));
        }
    }

    private void HandlePlayerDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(2f);
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameOver(false);
        }
    }

    public void OnGameStart()
    {
        gameStartTime = Time.time;
        // Logic reset health, etc. goes here
    }
}

}