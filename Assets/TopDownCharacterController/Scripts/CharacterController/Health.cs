using UnityEngine;

public class Health : MonoBehaviour
{
    public CharacterStats Character;

    [SerializeField] private float _health;

    private void Start()
    {
        _health = Character.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_health - damage < 0f)
        {
            Die();
        }
        else
        {
            _health -= damage;
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
