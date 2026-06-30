using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Vector3 _baseOffset = new Vector3(0, 0, -10);
    [SerializeField] private float _smoothTime = 0.25f;

    [Header("Look Ahead")]
    [SerializeField] private float _maxLookDistance = 5f;
    [SerializeField] private float _maxMouseDistance = 10f;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 playerPos = _target.transform.position;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = playerPos.z;

        Vector3 dir = (mouseWorld - playerPos);
        float distance = dir.magnitude;
        dir.Normalize();

        float t = Mathf.Clamp01(distance / _maxMouseDistance);
        Vector3 lookOffset = dir * (_maxLookDistance * t);

        Vector3 targetPos = playerPos + _baseOffset + lookOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
    }
}