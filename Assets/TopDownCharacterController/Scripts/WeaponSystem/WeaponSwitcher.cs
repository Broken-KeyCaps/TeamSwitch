using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField] private WeaponStats[] _weapons;

    private WeaponInstance[] _instances;
    private int _currentWeaponIndex = 0;

    private WeaponController _weaponController;

    private void Awake()
    {
        _weaponController = GetComponent<WeaponController>();
        _instances = new WeaponInstance[_weapons.Length];
        for (int i = 0; i < _weapons.Length; i++)
        {
            _instances[i] = new WeaponInstance(_weapons[i]);
        }
    }

    private void Start()
    {
        _weaponController.SetWeapon(_instances[_currentWeaponIndex]);
    }

    private void Update()
    {
        HandleKeyboardWeaponSelect();
        HandleMouseScrollWeaponSelect();
    }

    private void HandleKeyboardWeaponSelect()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                int index = (i == 0) ? _weapons.Length - 1 : i - 1;
                if (index >= 0 && index < _instances.Length)
                {
                    _currentWeaponIndex = index;
                    _weaponController.SetWeapon(_instances[_currentWeaponIndex]);
                }
                break;
            }
        }
    }

    private void HandleMouseScrollWeaponSelect()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            _currentWeaponIndex = (_currentWeaponIndex + 1) % _instances.Length;
            _weaponController.SetWeapon(_instances[_currentWeaponIndex]);
        }
        else if (scroll < 0f)
        {
            _currentWeaponIndex = (_currentWeaponIndex - 1 + _instances.Length) % _instances.Length;
            _weaponController.SetWeapon(_instances[_currentWeaponIndex]);
        }
    }
}