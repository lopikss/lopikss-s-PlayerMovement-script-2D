using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 
    [Header("Movement ---------------------------------------------------------------------------")]
    [SerializeField] [Range(0, 25f)] private float MovementSpeed = 2f;
    [SerializeField] [Range(0, 0.3f)] private float movementSmoothness = 0.05f;

    [Header("Jumping-----------------------------------------------------------------------------")]
    [SerializeField] [Range(0f, 25f)] private float JumpPower = 7f;
    [SerializeField] private bool doubleJump = false;

    [Header("fall speed controller --------------------------------------------------------------")]
    [SerializeField] [Range(0f, 10f)] private float FallMultiplier = 2.5f;
    [SerializeField] [Range(0f, 10f)] private float LowJumpMultiplier = 2f;

    [Header("ground checker ---------------------------------------------------------------------")]
    [SerializeField] private Transform GroundCheckObject = null;
    [SerializeField] private bool boxCheck;
    [SerializeField] private bool cirleCheck;
    [SerializeField] [Range(0, 3f)] private float boxHeight = 1f;
    [SerializeField] [Range(0, 3f)] private float boxWidth = 1f;
    [SerializeField] [Range(0, 3f)] private float circleSize = 1f;
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private bool _isGrounded = false;





    // private statements
    private float _movementDirection;
    private Vector3 _velocity = Vector3.zero;

    private bool _isFacingRight = true;
    private bool _isJumping = false;
    private bool _doubleJump;


    private Rigidbody2D _rigidbody2D;



    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Horizontal move input
        _movementDirection = Input.GetAxis("Horizontal");

        //player flipper part
        if (_movementDirection > 0 && !_isFacingRight)
        {
            FlipPlayer();
        }

        else if (_movementDirection < 0 && _isFacingRight)
        {
            FlipPlayer();
        }

        //_isGrounded statements
        if (_isGrounded)
        {
            _doubleJump = true;
        }

        // jump input
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _isJumping = true;
        }

        else if (Input.GetButtonDown("Jump") && _doubleJump && doubleJump)
        {
            _isJumping = true;
            _doubleJump = false;
        }
    }



    void FixedUpdate()
    {
        // Moving Part
        Vector3 targetVelocity =
            new Vector2(_movementDirection * MovementSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity =
            Vector3.SmoothDamp(
                _rigidbody2D.velocity, targetVelocity, ref _velocity, movementSmoothness
            );

        // Jumping Part
        _isGrounded = false;


        //box collider
        if (boxCheck == true)
        {
            Collider2D[] colliders =
                Physics2D.OverlapBoxAll(GroundCheckObject.position, new Vector2(boxWidth, boxHeight), 0f, WhatIsGround);


            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    _isGrounded = true;
                }
            }
        }

        //circleCollider
        if (cirleCheck == true)
        {
            Collider2D[] colliders =
                    Physics2D.OverlapCircleAll(GroundCheckObject.position, circleSize, WhatIsGround);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    _isGrounded = true;
                }
            }
        }


        if (_isJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
            _isJumping = false;
        }

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
    }

    private void FlipPlayer()
    {
        _isFacingRight = !_isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    #region Debug

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
    #endregion
}