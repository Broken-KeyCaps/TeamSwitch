using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("Info")]
    [Tooltip("The name of the weapon")]
    public string Title = "";

    [Tooltip("A short description of the weapon")]
    public string Description = "";

    [Tooltip("Sprite of the weapon displayed in the world")]
    public Sprite WorldSprite;

    [Tooltip("Sprite of the weapon displayed in the UI")]
    public Sprite UISprite;

    [Tooltip("Sound effect played when firing")]
    public AudioClip ShootSound;

    [Tooltip("Sprite used as the weapon's crosshair in the HUD or UI. Displayed at the center of the screen while this weapon is equipped.")]
    public Sprite Crosshair;

    [Header("Damage & Fire Rate")]
    [Tooltip("Damage dealt on hit")]
    public float Damage = 10;

    [Tooltip("Number of shots fired per second")]
    public float FireRate = 5;

    [Tooltip("Maximum distance the weapon can hit")]
    public float Distance = 100;

    [Header("Recoil")]
    [Tooltip("Minimum recoil intensity")]
    public float MinRecoil = 0;

    [Tooltip("Maximum recoil intensity")]
    public float MaxRecoil = 10;

    [Tooltip("How quickly recoil increases per shot")]
    public float RecoilIncreasePerShot = 1f;

    [Tooltip("How quickly recoil decreases over time")]
    public float RecoilDecreaseSpeed = 2f;

    [Tooltip("Delay before recoil reduction begins (in seconds)")]
    public float RecoilDecayDelay = 0.3f;

    [Header("Shot Pattern")]
    [Tooltip("If true, holding the fire button will keep shooting. If false, you must click each time.")]
    public bool IsAutomatic = false;

    [Tooltip("Number of projectiles fired per shot (for shotguns, etc.)")]
    public int PelletsPerShot = 1;

    [Tooltip("Number of shots fired per trigger pull")]
    public int ShotsPerTrigger = 1;

    [Tooltip("How many targets a bullet can pierce through")]
    public int PierceCount = 1;

    [Header("Ammunition")]
    [Tooltip("Maximum number of bullets in the magazine")]
    public int MagazineSize = 40;

    [Tooltip("Time it takes to reload (in seconds)")]
    public float ReloadTime = 2;

    [Header("Projectile Settings")]
    [Tooltip("If true, fires a physical projectile. If false, uses instant raycast hits.")]
    public bool IsProjectile;

    [Tooltip("Prefab of the projectile to spawn when fired")]
    public GameObject ProjectilePrefab;

    [Tooltip("Speed at which the projectile travels")]
    public float ProjectileSpeed = 80;

    [Tooltip("Time (in seconds) before the projectile is automatically destroyed")]
    public float ProjectileLifetime = 5f;

    [Header("Layers")]
    [Tooltip("Layers considered as enemies (can take damage)")]
    public LayerMask EnemyLayers;

    [Tooltip("Layers considered as obstacles that stop bullets")]
    public LayerMask WallLayers;

    [Header("Impact Settings")]
    public ImpactEffectSet ImpactEffects;
}