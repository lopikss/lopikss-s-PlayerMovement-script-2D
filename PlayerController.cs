using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement ---------------------------------------------------------------------------")]
    [SerializeField] [Range(0, 15f)] private float MovementSpeed = 2f;
    [SerializeField] [Range(0, 0.3f)] private float movementSmoothness = 0.05f;
    [SerializeField] private bool canPlayerCrouch;
    [SerializeField] [Range(0, 15f)] private float crouchSpeed = 1f;

    [Header("Jumping-----------------------------------------------------------------------------")]
    [SerializeField] [Range(0f, 25f)] private float JumpPower = 7f;
    [SerializeField] private bool allowDoubleJump;
    [SerializeField] [Range(0, 10)] private float doubleJumpPower = 7f;
    [SerializeField] [Range(0, 5)] private int DoubleJumpAmount = 1;

    [Header("fall speed controller --------------------------------------------------------------")]
    [SerializeField] [Range(0f, 10f)] private float FallMultiplier = 2.5f;
    [SerializeField] [Range(0f, 10f)] private float LowJumpMultiplier = 2f;

    [Header("Ground checker ---------------------------------------------------------------------")]
    [SerializeField] private Transform GroundCheckObject = null;
    [SerializeField] private bool boxCheck;
    [SerializeField] private bool cirleCheck;
    [SerializeField] [Range(0, 3f)] private float boxHeight = 1f;
    [SerializeField] [Range(0, 3f)] private float boxWidth = 1f;
    [SerializeField] [Range(0, 3f)] private float circleSize = 1f;
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private bool _isGrounded = false;


    private float _movementDirection;
    private Vector3 _velocity = Vector3.zero;

    private bool isFacingRight = true;
    private bool isJumping = false;
    private bool isDoubleJumping;
    public bool isCrouching;

    readonly private Collider2D[] colliders = new Collider2D[4];

    private Rigidbody2D _rigidbody2D;


    // so stands fot script only 
    private int so_doubleJump;


   


    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // porizontal move input
        _movementDirection = Input.GetAxis("Horizontal");

        // player sprite flipper
        if ((_movementDirection > 0 && !isFacingRight) || (_movementDirection < 0 && isFacingRight))
        {
            FlipPlayer();
        }

        //_isGrounded statements
        if (_isGrounded)
        {
            so_doubleJump = DoubleJumpAmount;
        }


        // makes player crouch
        if (canPlayerCrouch)
        {
            if (Input.GetButtonDown("Crouch") && _isGrounded)
            {
                MovementSpeed -= crouchSpeed;
            }
            if (Input.GetButtonUp("Crouch"))
            {
                MovementSpeed += crouchSpeed;
            }
        }

        // Jump input handeler
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            isJumping = true;
        }
        // double jump input handeler
        else if (Input.GetButtonDown("Jump") && allowDoubleJump && so_doubleJump > 0)
        {
            isDoubleJumping = true;
            --so_doubleJump;
        }
    }


    void FixedUpdate()
    {
        // makes player move (on horizontal axes only)
        Vector3 targetVelocity =
            new Vector2(_movementDirection * MovementSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref _velocity, movementSmoothness);


        // makes player jump
        _isGrounded = false;

        if (isJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
            isJumping = false;
        }

        // makes player double jump jump
        if (isDoubleJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * doubleJumpPower, ForceMode2D.Impulse);
            isDoubleJumping = false;
        }

        // falling handeler
        if (_rigidbody2D.velocity.y < 0f)
        {
            _rigidbody2D.gravityScale = FallMultiplier;
        }
        else if (_rigidbody2D.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            _rigidbody2D.gravityScale = LowJumpMultiplier;
        }
        else
        {
            _rigidbody2D.gravityScale = 1f;
        }


        //Box collider
        if (boxCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(WhatIsGround);
            var numberOfColliders = Physics2D.OverlapBox(GroundCheckObject.position, new Vector2(boxWidth, boxHeight), 0f, filter2D, colliders);
            if (numberOfColliders > 0) _isGrounded = true;
        }

        //Circle collider
        if (cirleCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(WhatIsGround);
            var numberOfColliders = Physics2D.OverlapCircle(GroundCheckObject.position, circleSize, filter2D, colliders);
            if (numberOfColliders > 0) _isGrounded = true;
        }
    }

    private void FlipPlayer()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }


    private void OnDrawGizmos()
    {
        if (GroundCheckObject != null && boxCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(GroundCheckObject.position, new Vector3(boxWidth, boxHeight, 0f));

        }
        if (GroundCheckObject != null && cirleCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheckObject.position, circleSize);

        }
    }
}
