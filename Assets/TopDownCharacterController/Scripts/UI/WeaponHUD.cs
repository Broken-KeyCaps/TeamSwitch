using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponHUD : MonoBehaviour
{
    [SerializeField] private Image _weaponImage;
    [SerializeField] private TMP_Text _weaponNameText;
    [SerializeField] private TMP_Text _weaponAmmoText;

    public void SetWeaponInfo(Sprite weaponSprite, string weaponName)
    {
        if (_weaponImage != null && weaponSprite != null)
        {
            _weaponImage.sprite = weaponSprite;
        }

        if (_weaponNameText != null)
        {
            _weaponNameText.text = weaponName;
        }
    }

    public void SetWeaponAmmo(int currentAmmo, int maxAmmo)
    {
        if (_weaponAmmoText != null)
        {
            _weaponAmmoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
}
