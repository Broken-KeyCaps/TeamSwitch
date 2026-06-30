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
}