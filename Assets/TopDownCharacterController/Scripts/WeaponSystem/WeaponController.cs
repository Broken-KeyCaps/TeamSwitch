using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    public WeaponInstance Weapon;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer _weaponSprite;
    [SerializeField] private CrosshairController _crosshairController;

    [Header("HUD")]
    [SerializeField] private WeaponHUD _weaponHUD;

    [Header("Transforms")]
    [SerializeField] private Transform _aimOrigin;
    [SerializeField] private Transform _muzzlePoint;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Animations")]
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _debugRecoilDistance = 3f;

    [Header("Weapon Switching")]
    [SerializeField] private WeaponStats _pistolStats;   
    [SerializeField] private WeaponStats _netGunStats;   

    private float _lastShotTime;
    private float _recoil;

    private bool _isShootingCoroutineRunning;

    private IEnumerator _reloadIEnumerator;

    private PhotonView _view;

    private void Awake()
    {
        _view = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
        
        if (_view != null && !_view.IsMine)
        {
            if (_crosshairController != null)
            {
                _crosshairController.gameObject.SetActive(false);
            }
        }

        
        if (_pistolStats != null)
        {
            SetWeapon(new WeaponInstance(_pistolStats));
        }
    }

    private void Update()
    {
        
        if (Weapon == null || Weapon.Stats == null) return;

        HandleInput();
        UpdateCooldown();
        UpdateRecoil();
        RecoilVisualization();
    }

    private void HandleInput()
    {
        if (_view != null && !_view.IsMine) return;

        
        if (Input.GetKeyDown(KeyCode.Alpha1) && _pistolStats != null)
        {
            SetWeapon(new WeaponInstance(_pistolStats));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && _netGunStats != null)
        {
            SetWeapon(new WeaponInstance(_netGunStats));
        }
        

        if (!CanShoot()) return;

        bool shouldShoot = Weapon.Stats.IsAutomatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shouldShoot)
            StartCoroutine(ShootRoutine());

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
    }

    public bool CanShoot()
    {
        return !_isShootingCoroutineRunning && Weapon != null && Weapon.CanShoot();
    }

    private void UpdateCooldown()
    {
        if (Weapon != null && Weapon.FireCooldown > 0f)
            Weapon.FireCooldown = Mathf.Max(0f, Weapon.FireCooldown - Time.deltaTime);
    }

    private void UpdateRecoil()
    {
        if (Time.time - _lastShotTime > Weapon.Stats.RecoilDecayDelay)
            _recoil = Mathf.MoveTowards(_recoil, Weapon.Stats.MinRecoil, Time.deltaTime * Weapon.Stats.RecoilDecreaseSpeed);

        _recoil = Mathf.Clamp(_recoil, Weapon.Stats.MinRecoil, Weapon.Stats.MaxRecoil);
    }

    private IEnumerator ShootRoutine()
    {
        _isShootingCoroutineRunning = true;

        int shotsPerTrigger = Mathf.Min(Weapon.Stats.ShotsPerTrigger, Weapon.CurrentAmmo);

        for (int spray = 0; spray < shotsPerTrigger; spray++)
        {
            Weapon.FireCooldown = 1f / Weapon.Stats.FireRate;
            _lastShotTime = Time.time;
            _recoil = Mathf.Min(_recoil + Weapon.Stats.RecoilIncreasePerShot, Weapon.Stats.MaxRecoil);

            for (int pellet = 0; pellet < Weapon.Stats.PelletsPerShot; pellet++)
            {
                if (Weapon.Stats.IsProjectile)
                    FireProjectile();
                else
                    FireRaycast();
            }

            if (_animator != null)
                _animator.SetTrigger("Shoot");

            if (_audioSource != null && Weapon.Stats.ShootSound != null)
                _audioSource.PlayOneShot(Weapon.Stats.ShootSound);

            if (spray < Weapon.Stats.ShotsPerTrigger - 1)
                yield return new WaitForSeconds(1f / Weapon.Stats.FireRate);

            Weapon.CurrentAmmo--;
            _weaponHUD?.SetWeaponAmmo(Weapon.CurrentAmmo, Weapon.Stats.MagazineSize);
        }

        CheckReload();

        _isShootingCoroutineRunning = false;
    }

    private void FireRaycast()
    {
        Vector3 start = _aimOrigin.position;
        start.z = 0f;

        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dirToMouse = (mousePos - start).normalized;
        Vector3 forward = ApplySpread(dirToMouse);
        

        RaycastHit2D[] hits = Physics2D.RaycastAll(start, forward, Weapon.Stats.Distance);

        
        Vector3 lastHitPoint = start + forward * Weapon.Stats.Distance;
        int pierceCount = 0;

        foreach (var hit in hits)
        {
            if (hit.collider.transform.root == transform.root)
                continue;

            if (((1 << hit.collider.gameObject.layer) & Weapon.Stats.WallLayers) != 0)
            {
                lastHitPoint = hit.point;
                SpawnImpactEffect(hit.collider.gameObject, hit.point);
                break;
            }

            if (((1 << hit.collider.gameObject.layer) & Weapon.Stats.EnemyLayers) != 0)
            {
                lastHitPoint = hit.point;

                
                Health health = hit.collider.GetComponent<Health>();

                
                if (health == null) health = hit.collider.GetComponentInParent<Health>();

                if (health != null)
                {
                    Debug.Log("SUCCESS! Hit " + health.gameObject.name + " and called TakeDamage!");
                    health.TakeDamage(Weapon.Stats.Damage);
                }
                else
                {
                    Debug.LogWarning("UH OH! The bullet hit " + hit.collider.gameObject.name + " but NO Health script was found on it or its parents!");
                }
                SpawnImpactEffect(hit.collider.gameObject, hit.point);

                pierceCount++;
                if (pierceCount >= Weapon.Stats.PierceCount)
                    break;
            }
        }

        if (Weapon.Stats.ProjectilePrefab != null && _muzzlePoint != null)
        {
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);
            GameObject tracer = Instantiate(Weapon.Stats.ProjectilePrefab, _muzzlePoint.position, rotation);

            BulletMover mover = tracer.GetComponent<BulletMover>();
            if (mover != null)
                mover.Init(lastHitPoint, Weapon.Stats.ProjectileSpeed, Weapon.Stats.ProjectileLifetime);
        }
    }

    private void SpawnImpactEffect(GameObject target, Vector2 hitPoint)
    {
        if (Weapon.Stats.ImpactEffects == null)
        {
            if (Weapon.Stats.ProjectilePrefab != null)
                Instantiate(Weapon.Stats.ProjectilePrefab, hitPoint, Quaternion.identity);
            return;
        }

        var info = target.GetComponent<DamageableInfo>();

        CharacterType? characterType = null;
        MaterialType? materialType = null;

        if (info != null)
        {
            if (info.Kind == DamageableKind.Character)
                characterType = info.CharacterType;
            else if (info.Kind == DamageableKind.Surface)
                materialType = info.MaterialType;
        }

        GameObject effectPrefab = Weapon.Stats.ImpactEffects.GetEffect(characterType, materialType);

        if (effectPrefab == null)
            effectPrefab = Weapon.Stats.ImpactEffects.DefaultImpact;

        if (effectPrefab != null)
            Instantiate(effectPrefab, hitPoint, Quaternion.identity);
    }

    private void FireProjectile()
    {
        if (Weapon.Stats.ProjectilePrefab == null || _muzzlePoint == null)
            return;

        Vector3 start = _muzzlePoint.position;
        start.z = 0f;

        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dirToMouse = (mousePos - start).normalized;
        Vector3 forward = ApplySpread(dirToMouse);
        

        float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);
        GameObject projectile = Instantiate(Weapon.Stats.ProjectilePrefab, start, rotation);

        

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = forward * Weapon.Stats.ProjectileSpeed;

        ProjectileDamage proj = projectile.GetComponent<ProjectileDamage>();
        if (proj != null)
        {
            proj.Init(
                owner: transform.root.gameObject,
                damage: Weapon.Stats.Damage,
                enemyLayers: Weapon.Stats.EnemyLayers,
                wallLayers: Weapon.Stats.WallLayers,
                maxPierceCount: Weapon.Stats.PierceCount,
                impactEffects: Weapon.Stats.ImpactEffects,
                lifetime: Weapon.Stats.ProjectileLifetime
            );
        }
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        float spreadAngle = Random.Range(-_recoil, _recoil);
        Quaternion rotation = Quaternion.AngleAxis(spreadAngle, Vector3.forward);
        return rotation * direction;
    }

    private void RecoilVisualization()
    {
        float distance = _debugRecoilDistance;
        Vector3 start = _aimOrigin.position;
        Vector3 forward = _aimOrigin.up;

        Debug.DrawLine(start, start + forward * distance, Color.white);

        Quaternion leftRot = Quaternion.AngleAxis(_recoil, Vector3.forward);
        Quaternion rightRot = Quaternion.AngleAxis(-_recoil, Vector3.forward);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Debug.DrawLine(start, start + leftDir * distance, Color.red);
        Debug.DrawLine(start, start + rightDir * distance, Color.red);
    }

    public void SetWeapon(WeaponInstance weapon)
    {
        if (weapon == null) return;

        Weapon = weapon;

        _lastShotTime = 0f;
        _recoil = Weapon.Stats.MinRecoil;

        if (_reloadIEnumerator != null)
        {
            MainWindowStoreStop();
        }

        if (_weaponSprite && Weapon.Stats.WorldSprite != null)
            _weaponSprite.sprite = Weapon.Stats.WorldSprite;

        if (_crosshairController != null && Weapon.Stats.Crosshair != null)
            _crosshairController.SetCrosshairSprite(Weapon.Stats.Crosshair);

        if (_weaponHUD != null)
        {
            if (Weapon.Stats.UISprite != null)
                _weaponHUD.SetWeaponInfo(Weapon.Stats.UISprite, Weapon.Stats.Title);

            _weaponHUD.SetWeaponAmmo(Weapon.CurrentAmmo, Weapon.Stats.MagazineSize);
        }

        CheckReload();
    }

    private void MainWindowStoreStop()
    {
        StopCoroutine(_reloadIEnumerator);
        _reloadIEnumerator = null;
    }

    private void CheckReload()
    {
        if (Weapon != null && Weapon.CurrentAmmo == 0)
            StartReload();
    }

    private void StartReload()
    {
        if (_reloadIEnumerator != null)
            StopCoroutine(_reloadIEnumerator);

        _reloadIEnumerator = Reload();
        StartCoroutine(_reloadIEnumerator);
    }

    private IEnumerator Reload()
    {
        if (Weapon.CurrentAmmo >= Weapon.Stats.MagazineSize)
            yield break;

        Weapon.IsReloading = true;
        Weapon.CurrentAmmo = 0;

        yield return new WaitForSeconds(Weapon.Stats.ReloadTime);

        Weapon.IsReloading = false;
        Weapon.CurrentAmmo = Weapon.Stats.MagazineSize;

        _weaponHUD?.SetWeaponAmmo(Weapon.CurrentAmmo, Weapon.Stats.MagazineSize);

        _reloadIEnumerator = null;
    }
}