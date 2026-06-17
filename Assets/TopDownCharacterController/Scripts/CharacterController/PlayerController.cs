using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterStats CharacterStats;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private Transform _bodyRoot;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (_bodyCollider == null)
        {
            _bodyCollider = GetComponent<Collider2D>();
        }

        if (CharacterStats == null)
        {
            Debug.LogError($"CharacterStats is not assigned on {name}!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CharacterStats == null) return;

        Movement();
        Turn();
    }

    private void Movement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Vector2 targetVelocity = input * CharacterStats.MaxWalkSpeed;

        _rigidbody2D.linearVelocity = Vector2.Lerp(
            _rigidbody2D.linearVelocity,
            targetVelocity,
            ((input != Vector2.zero) 
                ? CharacterStats.Acceleration
                : CharacterStats.Deceleration) * Time.fixedDeltaTime
        );
    }

    private void Turn()
    {
        if (_bodyRoot == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _bodyRoot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}