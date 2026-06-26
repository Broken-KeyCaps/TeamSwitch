using UnityEngine;
using Photon.Pun;

public class NetProjectile : MonoBehaviour
{
    private Vector3 _target;
    private float _speed;
    private bool _isInitialized = false;

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
            
            PhotonView hitView = collision.GetComponent<PhotonView>();
            if (hitView == null) hitView = collision.GetComponentInParent<PhotonView>();

            if (hitView != null)
            {
                
                hitView.RPC("RPC_GetTiedUp", RpcTarget.All);
            }

            Destroy(gameObject); 
        }
    }
}