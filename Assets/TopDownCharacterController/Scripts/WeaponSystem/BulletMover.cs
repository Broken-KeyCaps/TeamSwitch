using UnityEngine;
using Photon.Pun; 

public class BulletMover : MonoBehaviour
{
    private Vector3 _target;
    private float _speed;
    private bool _isInitialized = false; 

    [Header("Net Gun Toggle")]
    [SerializeField] private bool _isNetGunProjectile = false; 

    private void Start()
    {
        
        if (_isNetGunProjectile)
        {
            Destroy(gameObject, 5f); 
        }
    }


    public void Init(Vector3 target, float speed, float lifetime)
    {
        _target = target;
        _speed = speed;
        _isInitialized = true; 
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        
        if (!_isInitialized) return;

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
            Debug.Log("PROJECTILE HIT THE PLAYER HITBOX: " + collision.gameObject.name);

            
            if (_isNetGunProjectile)
            {
                PhotonView hitView = collision.GetComponent<PhotonView>();
                if (hitView == null) hitView = collision.GetComponentInParent<PhotonView>();

                if (hitView != null)
                {
                    
                    hitView.RPC("RPC_GetTiedUp", RpcTarget.All);
                }
            }
           
            else
            {
                Health enemyHealth = collision.GetComponent<Health>();
                if (enemyHealth == null) enemyHealth = collision.GetComponentInParent<Health>();

                if (enemyHealth != null)
                {
                    Debug.Log("FOUND HEALTH SCRIPT! Sending damage to " + collision.gameObject.name);
                    enemyHealth.TakeDamage(10f); 
                }
            }

            Destroy(gameObject);
        }
    }
}