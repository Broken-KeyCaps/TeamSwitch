using UnityEngine;

public class BulletMover : MonoBehaviour
{
    private Vector3 _target;
    private float _speed;

    public void Init(Vector3 target, float speed, float lifetime)
    {
        _target = target;
        _speed = speed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _target) < 0.05f)
        {
            Destroy(gameObject);
        }
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Wall") || (collision.transform.parent != null && collision.transform.parent.CompareTag("Wall")))
        {
            Destroy(gameObject);
            return;
        }

        
        if (collision.CompareTag("Player"))
        {
            Debug.Log(" BULLET HIT THE PLAYER HITBOX: " + collision.gameObject.name);

            
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth == null) enemyHealth = collision.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                Debug.Log(" FOUND HEALTH SCRIPT! Sending 10 damage to " + collision.gameObject.name);
                enemyHealth.TakeDamage(10f);
            }
            

            Destroy(gameObject);
        }
        
    }
}