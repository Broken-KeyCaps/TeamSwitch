[System.Serializable]
public class WeaponInstance
{
    public WeaponStats Stats;
    public int CurrentAmmo;
    public bool IsReloading;
    public float FireCooldown;

    public WeaponInstance(WeaponStats stats)
    {
        Stats = stats;
        CurrentAmmo = stats.MagazineSize;
        IsReloading = false;
        FireCooldown = 0f;
    }

    public bool CanShoot()
    {
        return !IsReloading && FireCooldown <= 0f && CurrentAmmo > 0;
    }
}