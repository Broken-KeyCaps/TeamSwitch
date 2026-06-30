using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class ProjectileDamage : MonoBehaviour
{
    [Header("Projectile Type")]
    [Tooltip("If true, this projectile causes an explosion on impact.")]
    [SerializeField] private bool _isExplosive = false;

    [Header("Explosion Settings")]
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionDamageMultiplier = 1f;
    [SerializeField] private GameObject _explosionEffect;

    private float _damage;
    private GameObject _owner;
    private LayerMask _enemyLayers;
    private LayerMask _wallLayers;
    private int _maxPierceCount;
    private ImpactEffectSet _impactEffects;

    private int _pierceCount = 0;

    public void Init(
        GameObject owner,
        float damage,
        LayerMask enemyLayers,
        LayerMask wallLayers,
        int maxPierceCount,
        ImpactEffectSet impactEffects,
        float lifetime)
    {
        _owner = owner;
        _damage = damage;
        _enemyLayers = enemyLayers;
        _wallLayers = wallLayers;
        _maxPierceCount = maxPierceCount;
        _impactEffects = impactEffects;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_owner != null && other.transform.root.gameObject == _owner)
            return;

        int layer = other.gameObject.layer;

        if (_isExplosive)
        {
            Explode();
            return;
        }

        if (((1 << layer) & _enemyLayers) != 0)
        {
            var health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(_damage);

            SpawnImpactEffect(other.gameObject);
            _pierceCount++;

            if (_pierceCount >= _maxPierceCount)
                Destroy(gameObject);

            return;
        }

        if (((1 << layer) & _wallLayers) != 0)
        {
            SpawnImpactEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void SpawnImpactEffect(GameObject hitObject)
    {
        if (_impactEffects == null)
            return;

        var info = hitObject.GetComponent<DamageableInfo>();

        CharacterType? characterType = null;
        MaterialType? materialType = null;

        if (info != null)
        {
            if (info.Kind == DamageableKind.Character)
                characterType = info.CharacterType;
            else if (info.Kind == DamageableKind.Surface)
                materialType = info.MaterialType;
        }

        GameObject effectPrefab = _impactEffects.GetEffect(characterType, materialType);

        if (effectPrefab == null)
            effectPrefab = _impactEffects.DefaultImpact;

        if (effectPrefab != null)
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
    }

    private void Explode()
    {
        if (_explosionEffect != null)
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _enemyLayers);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                float falloff = Mathf.Clamp01(1f - dist / _explosionRadius);
                float finalDamage = _damage * _explosionDamageMultiplier * falloff;
                health.TakeDamage(finalDamage);

                SpawnImpactEffect(hit.gameObject);
            }
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_isExplosive)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _explosionRadius);
        }
    }
#endif
}