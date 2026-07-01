using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPun
{
    public enum PlayerState { InLobby, Playing }
    public PlayerState currentState = PlayerState.Playing;

    [Header("Character Data")]
    [SerializeField] private CharacterStats CharacterStats;

    [Header("Dash Settings")]
    public float DashSpeed = 10f;
    public float DashDuration = 0.8f;
    public float DashCooldown = 1f;

    [Header("Carry & Drag Modifiers")]
    public float CarrySpeedMultiplier = 0.4f; 
    public float DragSpeedMultiplier = 0.75f; 

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private Transform _bodyRoot;
    public Animator _animator;

    private bool _isDashing = false;
    private float _dashCooldownTimer = 0f;
    private Vector2 _lastMoveDirection;

    private PlayerCarrySystem _carrySystem;

    private void Awake()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_bodyCollider == null) _bodyCollider = GetComponent<Collider2D>();
        _carrySystem = GetComponent<PlayerCarrySystem>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.visible = false;
            FollowCamera mainCam = Camera.main.GetComponent<FollowCamera>();
            if (mainCam != null)
            {
                mainCam.target = this.gameObject;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine || _isDashing) return;

        if (currentState == PlayerState.InLobby) return;

        Move();
        Turn();

        if (_dashCooldownTimer > 0)
            _dashCooldownTimer -= Time.fixedDeltaTime;
    }

    private void Update()
    {   
        if (!photonView.IsMine) return;

        if (currentState == PlayerState.InLobby) return;

        if (Input.GetKeyDown(KeyCode.Space) && _dashCooldownTimer <= 0f && !_isDashing)
        {
            if (_carrySystem == null || _carrySystem.CurrentState == PlayerCarrySystem.CarryState.None)
            {
                StartCoroutine(DashRoutine());
            }
        }
    }

    public void SetState(PlayerState newState)
    {
        currentState = newState;
    }
    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            input.Normalize();
            _lastMoveDirection = input;
        }

        
        float currentSpeedMod = 1f;
        if (_carrySystem != null)
        {
            if (_carrySystem.CurrentState == PlayerCarrySystem.CarryState.Carrying)
                currentSpeedMod = CarrySpeedMultiplier;
            else if (_carrySystem.CurrentState == PlayerCarrySystem.CarryState.Dragging)
                currentSpeedMod = DragSpeedMultiplier;
        }

        Vector2 targetVelocity = input * (CharacterStats.MaxWalkSpeed * currentSpeedMod);
        

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

        Vector3 lookDir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        float visualOffset = 90f;
        _bodyRoot.rotation = Quaternion.Euler(0f, 0f, angle + visualOffset);
    }
}