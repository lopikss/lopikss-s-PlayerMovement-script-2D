using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] [Range(0, 15f)] private float movementSpeed = 2f;
    [SerializeField] [Range(0, 0.3f)] private float movementSmoothness = 0.05f;


    [Header("Crouching")]
    [SerializeField] private bool canPlayerCrouch;
    [SerializeField] [Range(0, 1f)] private float crouchSpeedDecreaser = 1f;
    [SerializeField] private Collider2D crouchDisableCollider;
    [SerializeField] private Collider2D crouchDisableCollider2;


    [Header("Jumping")]
    [SerializeField] private bool allowMultipleJump = false;
    [SerializeField] [Range(0f, 25f)] private float jumpPower = 7f;
    [SerializeField] [Range(0, 10)] private float doubleJumpPower = 7f;
    [SerializeField] [Range(0, 5)] private int multipleJumpAmount = 1;


    [Header("Fall Speed Controller")]
    [SerializeField] [Range(0f, 10f)] private float fallMultiplier = 2.5f;
    [SerializeField] [Range(0f, 10f)] private float lowJumpMultiplier = 2f;


    [Header("Ground Checker")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private bool boxCheck;
    [SerializeField] private bool cirleCheck;
    [SerializeField] [Range(0, 3f)] private float groundBoxCheckWidth = 1f;
    [SerializeField] [Range(0, 3f)] private float groundBoxCheckHeight = 1f;
    [SerializeField] [Range(0, 3f)] private float groundCircleCheckRadius = 1f;
    [SerializeField] private LayerMask whatIsGround;


    [Header("Ciel Checker")]
    [SerializeField] private Transform cielCheck = null;
    [SerializeField] [Range(0, 3f)] private float ceilBoxCheckWidth = 1f;
    [SerializeField] [Range(0, 3f)] private float ceilBoxCheckHeight = 1f;

    [Header("Temp Checks")]
    [SerializeField] private bool isGrounded = false;


    private float movementDirection;
    private Vector3 velocity = Vector3.zero;

    private bool isFacingRight = true;
    private bool isJumping = false;
    private bool isMultipleJumping = false;
    private bool isCrouching = false;
    private bool speedHasBeenDecreased = false;
    private bool disableJump = false;

    private readonly Collider2D[] colliders = new Collider2D[4];

    private Rigidbody2D _rigidbody2D;

    // SO stands for "Script Only" 
    private int so_multipleJump;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Horizontal Movement Input
        movementDirection = Input.GetAxis("Horizontal");

        // Player Sprite Flip
        if ((movementDirection > 0 && !isFacingRight) || (movementDirection < 0 && isFacingRight))
        {
            FlipPlayer();
        }

        // Multiple Jump
        if (isGrounded)
        {
            so_multipleJump = multipleJumpAmount;
        }

        // Crouching Feature
        if (canPlayerCrouch)
        {
            isCrouching = false;

            if (Input.GetButton("Crouch"))
            {
                isCrouching = true;
                if (!speedHasBeenDecreased)
                {
                    movementSpeed *= crouchSpeedDecreaser;
                    speedHasBeenDecreased = true;
                }
            }

            // Check With Collider (Post-Process Input)
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(whatIsGround);
            var numberOfColliders = Physics2D.OverlapBox(cielCheck.position, new Vector2(ceilBoxCheckWidth, ceilBoxCheckHeight), 0f, filter2D, colliders);

            if (!isCrouching && speedHasBeenDecreased)
            {
                movementSpeed /= crouchSpeedDecreaser;
                speedHasBeenDecreased = false;
            }


            if (numberOfColliders > 0)
            {
                isCrouching = true;
                disableJump = true;
            }
            else
            {
                disableJump = false;
            }

            // Makes Player's Hitbox Smaller While Crouching
            if (crouchDisableCollider != null)
            {
                if (isCrouching)
                {
                    crouchDisableCollider.enabled = false;
                }
                else
                {
                    crouchDisableCollider.enabled = true;
                }
            }

            if (crouchDisableCollider2 != null)
            {
                if (isCrouching)
                {
                    crouchDisableCollider2.enabled = false;
                }
                else
                {
                    crouchDisableCollider2.enabled = true;
                }
            }
        }

        // Jump Input Handler
        if (Input.GetButtonDown("Jump") && isGrounded && disableJump == false)
        {
            isJumping = true;
        }

        // Multiple Jump Input Handler
        else if (Input.GetButtonDown("Jump") && allowMultipleJump && so_multipleJump > 0)
        {
            isMultipleJumping = true;
            --so_multipleJump;
        }
    }

    void FixedUpdate()
    {
        // Horizontal Axes Movement
        Vector3 targetVelocity =
            new Vector2(movementDirection * movementSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothness);

        // Jump
        isGrounded = false;

        if (isJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            isJumping = false;
        }

        // Multiple Jump
        if (isMultipleJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * doubleJumpPower, ForceMode2D.Impulse);
            isMultipleJumping = false;
        }

        // Falling Handler
        if (_rigidbody2D.velocity.y < 0f)
        {
            _rigidbody2D.gravityScale = fallMultiplier;
        }
        else if (_rigidbody2D.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            _rigidbody2D.gravityScale = lowJumpMultiplier;
        }
        else
        {
            _rigidbody2D.gravityScale = 1f;
        }

        // Box Collider
        if (boxCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(whatIsGround);

            var numberOfColliders = Physics2D.OverlapBox(
                groundCheck.position,
                new Vector2(groundBoxCheckWidth, groundBoxCheckHeight), 0f, filter2D, colliders);


            if (numberOfColliders > 0) isGrounded = true;
        }

        // Circle Collider
        if (cirleCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(whatIsGround);

            var numberOfColliders = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCircleCheckRadius,
                filter2D, colliders
            );

            if (numberOfColliders > 0)isGrounded = true;
        }
    }

    private void FlipPlayer()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null && boxCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, new Vector3
                (groundBoxCheckWidth, groundBoxCheckHeight, 0f));
        }

        if (groundCheck != null && cirleCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCircleCheckRadius);
        }

        if (cielCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(cielCheck.position, new Vector3
                (ceilBoxCheckWidth, ceilBoxCheckHeight, 0f));
        }
    }
}
