using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField][Range(0, 15f)] private float movementSpeed = 2f;
    [SerializeField][Range(0, 0.3f)] private float movementSmoothness = 0.05f;


    [Header("Crouching")]
    [SerializeField] private bool _canPlayerCrouch;
    [SerializeField][Range(0, 1f)] private float _crouchSpeedDecreaser = 1f;
    [SerializeField] private Collider2D _crouchDisableCollider;
    [SerializeField] private Collider2D _crouchDisableCollider2;


    [Header("Jumping")]
    [SerializeField] private bool _allowMultipleJump = false;
    [SerializeField][Range(0f, 25f)] private float _firstJumpPower = 7f;
    [SerializeField][Range(0, 10)] private float _multipleJumpPower = 7f;
    [SerializeField][Range(0, 5)] private int _multipleJumpAmount = 1;


    [Header("Fall Speed Controller")]
    [SerializeField][Range(0f, 10f)] private float _fallMultiplier = 2.5f;
    [SerializeField][Range(0f, 10f)] private float _lowJumpMultiplier = 2f;


    [Header("Ground Checker")]
    [SerializeField] private Transform _groundCheck = null;
    [SerializeField] private bool _boxCheck;
    [SerializeField] private bool _cirleCheck;
    [SerializeField][Range(0, 3f)] private float _groundBoxCheckWidth = 1f;
    [SerializeField][Range(0, 3f)] private float _groundBoxCheckHeight = 1f;
    [SerializeField][Range(0, 3f)] private float _groundCircleCheckRadius = 1f;
    [SerializeField] private LayerMask _whatIsGround;


    [Header("Ciel Checker")]
    [SerializeField] private Transform _cielCheck = null;
    [SerializeField][Range(0, 3f)] private float _ceilBoxCheckWidth = 1f;
    [SerializeField][Range(0, 3f)] private float _ceilBoxCheckHeight = 1f;

    [Header("Temp Checks")]
    [SerializeField] private bool _isGrounded = false;


    private float _movementDirection;
    private Vector3 _velocity = Vector3.zero;

    private bool _isFacingRight = true;
    private bool _isJumping = false;
    private bool _isMultipleJumping = false;
    private bool _isCrouching = false;
    private bool speedHasBeenDecreased = false;
    private bool disableJump = false;

    private readonly Collider2D[] colliders = new Collider2D[4];

    private Rigidbody2D _rigidbody2D;

    // SO stands for "Script Only" 
    private int _SO_multipleJump;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Horizontal Movement Input
        _movementDirection = Input.GetAxis("Horizontal");

        // Player Sprite Flip
        if ((_movementDirection > 0 && !_isFacingRight) || (_movementDirection < 0 && _isFacingRight))
        {
            FlipPlayer();
        }

        // Multiple Jump
        if (_isGrounded)
        {
            _SO_multipleJump = _multipleJumpAmount;
        }

        // Crouching Feature
        if (_canPlayerCrouch)
        {
            _isCrouching = false;

            if (Input.GetButton("Crouch"))
            {
                _isCrouching = true;
                if (!speedHasBeenDecreased)
                {
                    movementSpeed *= _crouchSpeedDecreaser;
                    speedHasBeenDecreased = true;
                }
            }

            // Check With Collider (Post-Process Input)
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(_whatIsGround);
            var numberOfColliders = Physics2D.OverlapBox(_cielCheck.position, new Vector2(_ceilBoxCheckWidth, _ceilBoxCheckHeight), 0f, filter2D, colliders);
                   

            if (numberOfColliders > 0)
            {
                _isCrouching = true;
                disableJump = true;
            }
            else
            {
                disableJump = false;
            }

            // Makes Player's Hitbox Smaller While Crouching
            if (_crouchDisableCollider != null)
            {
                if (_isCrouching)
                {
                    _crouchDisableCollider.enabled = false;
                }
                else
                {
                    _crouchDisableCollider.enabled = true;
                }
            }

            if (_crouchDisableCollider2 != null)
            {
                if(_isCrouching)
                {
                    _crouchDisableCollider2.enabled = false;
                }
                else
                {
                    _crouchDisableCollider2.enabled = true;
                }
            }
        }

        // Jump Input Handler
        if (Input.GetButtonDown("Jump") && _isGrounded && disableJump == false)
        {
            _isJumping = true;
        }

        // Multiple Jump Input Handler
        else if (Input.GetButtonDown("Jump") && _allowMultipleJump && _SO_multipleJump > 0)
        {
            _isMultipleJumping = true;
            --_SO_multipleJump;
        }
    }

    void FixedUpdate()
    {
        // Horizontal Axes Movement
        Vector3 targetVelocity =
            new Vector2(_movementDirection * movementSpeed, _rigidbody2D.velocity.y);
        _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref _velocity, movementSmoothness);

        // Jump
        _isGrounded = false;

        if (_isJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * _firstJumpPower, ForceMode2D.Impulse);
            _isJumping = false;
        }

        // Multiple Jump
        if (_isMultipleJumping)
        {
            _rigidbody2D.AddForce(Vector2.up * _multipleJumpPower, ForceMode2D.Impulse);
            _isMultipleJumping = false;
        }

        // Falling Handler
        if (_rigidbody2D.velocity.y < 0f)
        {
            _rigidbody2D.gravityScale = _fallMultiplier;
        }
        else if (_rigidbody2D.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            _rigidbody2D.gravityScale = _lowJumpMultiplier;
        }
        else
        {
            _rigidbody2D.gravityScale = 1f;
        }

        // Box Collider
        if (_boxCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(_whatIsGround);

            var numberOfColliders = Physics2D.OverlapBox(
                _groundCheck.position,
                new Vector2(_groundBoxCheckWidth, _groundBoxCheckHeight),0f, filter2D, colliders);
                

            if (numberOfColliders > 0) _isGrounded = true;
        }

        // Circle Collider
        if (_cirleCheck == true)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            filter2D.SetLayerMask(_whatIsGround);

            var numberOfColliders = Physics2D.OverlapCircle(
                _groundCheck.position,
                _groundCircleCheckRadius,
                filter2D, colliders
            );

            if (numberOfColliders > 0) _isGrounded = true;
        }
    }

    private void FlipPlayer()
    {
        _isFacingRight = !_isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnDrawGizmos()
    {
        if (_groundCheck != null && _boxCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_groundCheck.position, new Vector3
                (_groundBoxCheckWidth, _groundBoxCheckHeight, 0f));
        }

        if (_groundCheck != null && _cirleCheck == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCircleCheckRadius);
        }

        if (_cielCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_cielCheck.position, new Vector3
                (_ceilBoxCheckWidth, _ceilBoxCheckHeight, 0f));
        }
    }
}
