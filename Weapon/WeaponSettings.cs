using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon-Data")]
public class WeaponSettings : ScriptableObject
{
    //settings for building up different type of weapons

    [Header("Core")]
    public string weaponName = "Weapon";
    public GameObject bulletPrefab;
    public bool isAutomatic = false;
    public int bulletsPerShot = 1;
    public float damage = 25f;

    [Header("Timing")]
    public float shotCooldown = 0.25f;
    public int magazineSize = 5;
    public float reloadTime = 1.2f;

    [Header("Ballistics")]
    public float bulletSpeed = 35f;
    public float bulletRange = 50f;
    public float spreadAngle = 1f;

    [Header("Player modifiers")]
    public float moveSpeedMultiplier = 1f;
}
