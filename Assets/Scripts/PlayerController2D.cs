using UnityEngine;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement (Side-Scroller)")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 9f;
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float groundCheckRadius = 0.25f;

        [Header("Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite spriteFront;
        [SerializeField] private Sprite spriteSide;
        [SerializeField] private Sprite spriteBack;

        private Rigidbody2D rb;
        private float horizontalInput;
        private bool jumpRequested;
        private bool isGrounded;

        private readonly Collider2D[] groundBuffer = new Collider2D[4];

        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                jumpRequested = true;
            }

            UpdateFacingAndSprite();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            // Horizontal velocity
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

            // Jump
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

        private void UpdateFacingAndSprite()
        {
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                FacingDirection = new Vector2(Mathf.Sign(horizontalInput), 0f);

                if (spriteRenderer != null)
                {
                    if (spriteSide != null) spriteRenderer.sprite = spriteSide;
                    spriteRenderer.flipX = horizontalInput < 0f;
                }
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
