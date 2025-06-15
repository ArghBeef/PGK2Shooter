using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon-Data")]
public partial class WeaponSettings : ScriptableObject
{

    public string weaponName = "Weapon";
    public GameObject bulletPrefab;
    public bool isAutomatic = false;
    public int bulletsPerShot = 1;
    public float damage = 25f;

    public float shotCooldown = 0.25f;
    public int magazineSize = 5;
    public float reloadTime = 1.2f;

    public float bulletSpeed = 35f;
    public float bulletRange = 50f;
    public float spreadAngle = 1f;

    public float moveSpeedMultiplier = 1f;
}
