using UnityEngine;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement (Side-Scroller)")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float jumpForce = 9.5f;
        [SerializeField] private float acceleration = 45f;
        [SerializeField] private float deceleration = 55f;
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float groundCheckRadius = 0.25f;

        [Header("Visuals & Animation")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite spriteIdle;

        private Rigidbody2D rb;
        private float horizontalInput;
        private bool jumpRequested;
        private bool isGrounded;

        private readonly Collider2D[] groundBuffer = new Collider2D[4];

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");

        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public bool IsGrounded => isGrounded;
        public bool IsMovementLocked => movementLockTimer > 0f;

        private float movementLockTimer = 0f;

        public void LockMovement(float duration)
        {
            movementLockTimer = Mathf.Max(movementLockTimer, duration);
            horizontalInput = 0f;
            jumpRequested = false;
            if (rb != null)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (spriteRenderer != null && spriteIdle != null && animator == null)
            {
                spriteRenderer.sprite = spriteIdle;
            }
        }

        private void Update()
        {
            if (movementLockTimer > 0f)
            {
                movementLockTimer -= Time.deltaTime;
                horizontalInput = 0f;
                jumpRequested = false;
            }
            else
            {
                horizontalInput = Input.GetAxisRaw("Horizontal");

                if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
                {
                    jumpRequested = true;
                }
            }

            UpdateFacingAndAnimation();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (IsMovementLocked)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            // Smooth Horizontal Velocity using Acceleration & Deceleration
            float targetVelocityX = horizontalInput * moveSpeed;
            float accelRate = Mathf.Abs(targetVelocityX) > 0.01f ? acceleration : deceleration;
            float newVelocityX = Mathf.MoveTowards(rb.velocity.x, targetVelocityX, accelRate * Time.fixedDeltaTime);

            rb.velocity = new Vector2(newVelocityX, rb.velocity.y);

            // Jump handling
            if (jumpRequested)
            {
                if (isGrounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                }
                jumpRequested = false;
            }
        }

        private void CheckGrounded()
        {
            Vector2 feetPos = (Vector2)transform.position + Vector2.down * 0.65f;
            int count = Physics2D.OverlapCircleNonAlloc(feetPos, groundCheckRadius, groundBuffer, groundLayer);

            isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                Collider2D col = groundBuffer[i];
                if (col.gameObject != gameObject && !col.isTrigger)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        private void UpdateFacingAndAnimation()
        {
            float absSpeed = Mathf.Abs(horizontalInput);

            // Flip arah hadap
            if (absSpeed > 0.05f)
            {
                FacingDirection = new Vector2(Mathf.Sign(horizontalInput), 0f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = horizontalInput < 0f;
                }
            }

            // Integrasi Unity Animator
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat(SpeedParam, absSpeed);
                animator.SetBool(IsGroundedParam, isGrounded);
            }
            else if (spriteRenderer != null && spriteIdle != null && absSpeed <= 0.05f)
            {
                spriteRenderer.sprite = spriteIdle;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 feetPos = (Vector2)transform.position + Vector2.down * 0.65f;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(feetPos, groundCheckRadius);
        }
    }
}
