using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Footsteps : MonoBehaviour
{
    [SerializeField] private AudioClip[] _leftFootstepSounds;
    [SerializeField] private AudioClip[] _rightFootstepSounds;

    [SerializeField] private float _stepDistance = 2.0f;
    [SerializeField] private Vector2 _stepVariationRange = new Vector2(0f, 0.5f);

    private AudioSource _audioSource;

    private Vector3 _lastPosition;

    private bool _leftStep = true;

    private float _distanceMoved;
    private float _stepVariation;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        _distanceMoved += (transform.position - _lastPosition).magnitude;

        if (_distanceMoved >= _stepDistance + _stepVariation)
        {
            PlayFootstep();
            _distanceMoved = 0f;
            _stepVariation = Random.Range(_stepVariationRange.x, _stepVariationRange.y);
        }

        _lastPosition = transform.position;
    }

    private void PlayFootstep()
    {
        AudioClip stepSound = _leftStep
                ? _leftFootstepSounds[Random.Range(0, _leftFootstepSounds.Length)]
                : _rightFootstepSounds[Random.Range(0, _rightFootstepSounds.Length)];

        _audioSource.PlayOneShot(stepSound);
        _leftStep = !_leftStep;
    }
}
