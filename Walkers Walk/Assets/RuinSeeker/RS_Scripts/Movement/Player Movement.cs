using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private float gravityScale = 1f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing;
    private float dashTimer;
    private float doubleClickTime = 0.25f;
    private float lastClickTime;
    [HideInInspector] public bool invertedControlls = false;
    private bool hasDashedInAir;

    [Header("Ground & Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float leftRightGroundCheckOffset = 0.2f;
    [SerializeField] private float wallCheckOffset = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask headLayer;

    [Header("Coyote Time")]
    [SerializeField, Range(0f, 0.5f)] private float coyoteTime = 0.1f;

    [Header("Input Buffering")]
    [SerializeField, Range(0f, 0.5f)] private float inputBufferTime = 0.1f;

    private bool facingRight = true;
    private bool isGrounded;
    private bool isWallSliding;
    private float coyoteTimer;
    private float inputBufferTimer;

    private float auxRbGravityScale;
    private float auxGravityScale;

    private enum PlayerState { Walking, Jumping, WallSliding, WallJumping }
    private PlayerState currentState;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D playerCollider;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Checkpoint")]
    [SerializeField] private GameObject startCheckpoint;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    public float fudgeThreshold = 0.1f;

    private void Start()
    {
        currentState = PlayerState.Walking;
        auxGravityScale = gravityScale;
        auxRbGravityScale = rb.gravityScale;

        transform.position = startCheckpoint.transform.position;
    }

    private void Update()
    {
        HandleState();

        if (Input.GetMouseButtonDown(0))
        {
            inputBufferTimer = inputBufferTime;

            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= doubleClickTime)
            {
                if (!hasDashedInAir)
                {
                    Dash();
                    animator.SetTrigger("Dash");
                    hasDashedInAir = true;
                }
            }
            lastClickTime = Time.time;
        }

        if (inputBufferTimer > 0)
        {
            Jump();
            inputBufferTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }

        Debug.Log(currentState);
    }

    private void FixedUpdate()
    {
        RaycastHit2D GroundHitLeft = Physics2D.Raycast(transform.position + new Vector3(-leftRightGroundCheckOffset, 0, 0), Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D GroundHitMiddle = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D GroundHitRight = Physics2D.Raycast(transform.position + new Vector3(leftRightGroundCheckOffset, 0, 0), Vector2.down, groundCheckDistance, groundLayer);

        isGrounded = GroundHitLeft.collider != null || GroundHitMiddle.collider != null || GroundHitRight.collider != null;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (!isDashing) hasDashedInAir = false;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }

        RaycastHit2D WallHitTop = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckOffset, 0), facingRight ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D WallHitMiddle = Physics2D.Raycast(transform.position, facingRight ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D WallHitBottom = Physics2D.Raycast(transform.position + new Vector3(0, -wallCheckOffset, 0), facingRight ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);

        isWallSliding = WallHitTop.collider != null || WallHitMiddle.collider != null || WallHitBottom.collider != null;

        RaycastHit2D HeadHitLeft = Physics2D.Raycast(transform.position + new Vector3(-leftRightGroundCheckOffset, 0, 0), Vector2.up, groundCheckDistance, headLayer);
        RaycastHit2D HeadHitMiddle = Physics2D.Raycast(transform.position, Vector2.up, groundCheckDistance, headLayer);
        RaycastHit2D HeadHitRight = Physics2D.Raycast(transform.position + new Vector3(leftRightGroundCheckOffset, 0, 0), Vector2.up, groundCheckDistance, headLayer);

        if (HeadHitLeft || HeadHitMiddle || HeadHitRight)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }


        if (!isGrounded && !isWallSliding)
        {
            rb.velocity += Vector2.down * gravityScale * Time.fixedDeltaTime;
        }

        if (isGrounded && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        ////NOHACERCASO
        //if (rb.velocity.y <= 0)
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        //    if (hit.collider != null)
        //    {
        //        float distanceToPlatform = hit.distance;

        //        if (distanceToPlatform < fudgeThreshold)
        //        {
        //            transform.position = new Vector2(transform.position.x, hit.point.y + playerCollider.bounds.extents.y);
        //            rb.velocity = new Vector2(rb.velocity.x, 0);
        //        }
        //    }
        //}

        if (isDashing)
        {
            float moveDirection = facingRight ? 1 : -1;
            if (invertedControlls) moveDirection *= -1;
            rb.velocity = new Vector2(dashSpeed * moveDirection, 0);
        }
        else
        {
            switch (currentState)
            {
                case PlayerState.Walking:
                    //Walk();
                    break;
                case PlayerState.Jumping:
                    JumpMovement();
                    break;
                case PlayerState.WallSliding:
                    WallSlide();
                    break;
                case PlayerState.WallJumping:
                    WallJump();
                    break;
            }

            if (currentState != PlayerState.WallSliding)
            {
                Walk();
            }

            if (currentState != PlayerState.WallSliding)
            {
                currentState = PlayerState.Walking;
            }

            if (!isWallSliding && !isGrounded)
            {
                currentState = PlayerState.Jumping;
            }
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case PlayerState.Walking:
                if (!isGrounded && isWallSliding)
                {
                    currentState = PlayerState.WallSliding;
                }
                break;

            case PlayerState.Jumping:
                if (rb.velocity.y <= 0)
                {
                    if (isWallSliding)
                    {
                        currentState = PlayerState.WallSliding;
                    }
                }
                break;

            case PlayerState.WallSliding:
                if (isGrounded)
                {
                    currentState = PlayerState.Walking;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    currentState = PlayerState.WallJumping;
                    WallJump();
                }
                break;

            case PlayerState.WallJumping:
                if (rb.velocity.y <= 0)
                {
                    if (isGrounded)
                    {
                        currentState = PlayerState.Walking;
                    }
                    else if (isWallSliding)
                    {
                        currentState = PlayerState.WallSliding;
                    }
                }
                break;
        }
    }

    private void Walk()
    {
        float moveDirection = facingRight ? 1 : -1;
        rb.velocity = new Vector2(speed * moveDirection, rb.velocity.y);

        if (isGrounded && isWallSliding)
        {
            Flip();
        }
    }

    private void Jump()
    {
        if (coyoteTimer > 0)
        {
            SoundManager.PlaySound(SoundType.JUMP);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentState = PlayerState.Jumping;
            coyoteTimer = 0;
            hasDashedInAir = false;
        }
    }

    private void JumpMovement()
    {
        if (isGrounded)
        {
            currentState = PlayerState.Walking;
        }
    }

    private void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
    }

    private void WallJump()
    {
        Flip();
        SoundManager.PlaySound(SoundType.JUMP);
        float moveDirection = facingRight ? 1 : -1;
        rb.velocity = new Vector2(speed * moveDirection, wallJumpForce);
        currentState = PlayerState.Jumping;
        hasDashedInAir = false;
    }

    private void Dash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        rb.gravityScale = 0;
        gravityScale = 0;
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = auxRbGravityScale;
        gravityScale = auxGravityScale;
    }

    private void Flip()
    {
        SoundManager.PlaySound(SoundType.WALLBUMP);

        facingRight = !facingRight;
        if (facingRight)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + new Vector3(-leftRightGroundCheckOffset, 0, 0), transform.position + new Vector3(-leftRightGroundCheckOffset, 0, 0) + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(transform.position + new Vector3(leftRightGroundCheckOffset, 0, 0), transform.position + new Vector3(leftRightGroundCheckOffset, 0, 0) + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + new Vector3(0, wallCheckOffset, 0), transform.position + new Vector3(0, wallCheckOffset, 0) + (facingRight ? Vector3.right : Vector3.left) * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + (facingRight ? Vector3.right : Vector3.left) * wallCheckDistance);
        Gizmos.DrawLine(transform.position + new Vector3(0, -wallCheckOffset, 0), transform.position + new Vector3(0, -wallCheckOffset, 0) + (facingRight ? Vector3.right : Vector3.left) * wallCheckDistance);
    }

    public void InvertControls()
    {
        invertedControlls = !invertedControlls;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Dead"))
        {
            DeadFunction();
        }
    }

    public void DeadFunction()
    {
        rb.velocity = Vector3.zero;
        transform.position = GameManager.Instance.GetCheckpointPosition();
        enemySpawner.DeleteAllEnemies();
        enemySpawner.SpawnAllEnemies();
        invertedControlls = false;
    }

    public void JumpAfterKillingEnemy()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce / 1.5f);
        SoundManager.PlaySound(SoundType.JUMP);

    }
}