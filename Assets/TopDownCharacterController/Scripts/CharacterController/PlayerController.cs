using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPun
{
    [Header("Character Data")]
    [SerializeField] private CharacterStats CharacterStats;

    [Header("Dash Settings")]
    public float DashSpeed = 10f;
    public float DashDuration = 0.8f;
    public float DashCooldown = 1f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private Transform _bodyRoot;
    public Animator _animator; 

    private bool _isDashing = false;
    private float _dashCooldownTimer = 0f;
    private Vector2 _lastMoveDirection;

    private void Awake()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_bodyCollider == null) _bodyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (photonView.IsMine) Cursor.visible = false;
    }

    private void Update()
    {
        if (CharacterStats == null || !photonView.IsMine) return;

        
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;

        // Check for Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && _dashCooldownTimer <= 0f && !_isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        if (!_isDashing)
        {
            Movement();
            Turn();
        }
    }

    private void Movement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 0.1f)
        {
            input.Normalize();
            _lastMoveDirection = input; 
        }

        Vector2 targetVelocity = input * CharacterStats.MaxWalkSpeed;

        _rigidbody2D.linearVelocity = Vector2.Lerp(
            _rigidbody2D.linearVelocity,
            targetVelocity,
            ((input != Vector2.zero) ? CharacterStats.Acceleration : CharacterStats.Deceleration) * Time.fixedDeltaTime
        );

        
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", input.x);
            _animator.SetFloat("MoveY", input.y);
            _animator.SetFloat("Speed", input.sqrMagnitude);
        }
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _dashCooldownTimer = DashCooldown;

        if (_animator != null) _animator.SetTrigger("Dash");

        
        Vector2 dashDir = _lastMoveDirection == Vector2.zero ? Vector2.right : _lastMoveDirection;

        _rigidbody2D.linearVelocity = dashDir * DashSpeed;

        yield return new WaitForSeconds(DashDuration);

        _isDashing = false;
    }

    private void Turn()
    {
        if (_bodyRoot == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        _bodyRoot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}