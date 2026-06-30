using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float _destroyAfterTime = 5;

    private void Start()
    {
        Destroy(gameObject, _destroyAfterTime);
    }
}