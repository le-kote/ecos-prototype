using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(FootAnimationManager))]
public class SpringLikeMover : MonoBehaviour
{
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private FootAnimationManager _animationManager;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 5f;
    [SerializeField] private float _walkAccel = 6f;
    [SerializeField] private float _sprintAccel = 6f;
    [SerializeField] private float _walkDecel = 12f;
    [SerializeField] private float _sprintDecel = 3f;
    [SerializeField] private float _airAccel = 1f;
    [SerializeField] private float _airDecel = .25f;

    [Header("Ground detection")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform[] _footEffectors;

    [Header("Spring")]
    [SerializeField] private float _springMoveSpeed = 4f;
    [SerializeField] private float _bodyHeight = 1.5f;

    private float _input;
    private bool _isGrounded;

    private bool _sprintingPressed;
    private bool _isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animationManager = GetComponent<FootAnimationManager>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        UpdateGrounded();
        HandleMovement();
        HandleSpring();
    }

    private void UpdateGrounded()
    {
        bool grounded = false;

        foreach (var item in _footEffectors)
        {
            if (Physics2D.Raycast((Vector2)item.position + Vector2.down, Vector2.down, .1f, _groundLayer))
            {
                grounded = true;
                break;
            }
        }

        _isGrounded = grounded;
    }

    private void HandleMovement()
    {
        if (_rb.bodyType == RigidbodyType2D.Static)
            return;

        if (_rb.linearVelocity.x < _sprintSpeed * .7 && _input != 0)
            _isSprinting = false;

        if (Mathf.Abs(_input) > 0)
        {
            var accel = _isGrounded ? (_isSprinting ? _sprintAccel : _walkAccel) : _airAccel;
            var speed = _isSprinting ? _sprintSpeed : _walkSpeed;

            if (_sprintingPressed)
                _isSprinting = true;

            _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, Vector2.right * _input * speed, accel * Time.fixedDeltaTime);
        }
        else
        {
            var decel = _isGrounded ? (_isSprinting ? _sprintDecel : _walkDecel) : _airDecel;
            _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, Vector2.zero, decel * Time.fixedDeltaTime);

            if (_rb.linearVelocity.x <= .01f && !_sprintingPressed)
                _isSprinting = false;
        }
    }

    private void HandleSpring()
    {
        if (!_animationManager.IsGrounded)
            return;

        var height = 0f;
        var divider = 0;

        foreach (var item in _animationManager.Legs)
        {
            if (!item.UsedInHeightCalculation)
                return;

            height += item.Target.position.y;
            divider++;
        }

        if (divider <= 0)
            return;

        height /= divider;
        height += _bodyHeight;

        transform.position = Vector3.Lerp(transform.position, new(transform.position.x, height, transform.position.z), _springMoveSpeed * Time.fixedDeltaTime);
    }

    public void GatherInput(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>().x;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _sprintingPressed = true;
            _isSprinting = true;
        }
        else if (context.canceled)
        {
            _sprintingPressed = false;
        }
    }
}
