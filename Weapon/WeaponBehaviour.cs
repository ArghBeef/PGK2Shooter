using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponBehaviour : NetworkBehaviour
{
    [HideInInspector] public WeaponSettings data;
    [HideInInspector] public Transform firePoint;

    int currentAmmo;
    bool isReloading;
    float lastShotTime;

    public int CurrentAmmo => currentAmmo; //forhud
    public bool IsReloading => isReloading;
    public float ReloadFraction { get; private set; }

    void Start()
    {
        currentAmmo = data ? data.magazineSize : 0;
    }

    public bool CanFire =>
        !isReloading &&
        Time.time >= lastShotTime + data.shotCooldown &&
        currentAmmo > 0;

    public void TryFire(Vector3 dir)
    {
        if (!IsOwner || !CanFire) return;
        FireServerRpc(dir);
    }

    [ServerRpc]
    void FireServerRpc(Vector3 dir)
    {
        SpawnBullets(dir);
        currentAmmo--;
        lastShotTime = Time.time;

        if (currentAmmo <= 0)
            StartCoroutine(Reload());

        FireClientRpc();
    }

    [ClientRpc] void FireClientRpc() {
    }

    void SpawnBullets(Vector3 baseDir)
    {
        for (int i = 0; i < data.bulletsPerShot; i++)
        {
            Vector3 dir = Quaternion.Euler(
                              Random.Range(-data.spreadAngle, data.spreadAngle),
                              Random.Range(-data.spreadAngle, data.spreadAngle),
                              0) * baseDir;

            GameObject go = Instantiate(data.bulletPrefab,
                                         firePoint.position,
                                         Quaternion.LookRotation(dir));
            var rb = go.GetComponent<Rigidbody>();
            rb.linearVelocity = dir * data.bulletSpeed;

            var netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();

            Destroy(go, data.bulletRange / data.bulletSpeed);
        }
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        float t = 0f;
        while (t < data.reloadTime)
        {
            t += Time.deltaTime;
            ReloadFraction = t / data.reloadTime;
            yield return null;
        }
        ReloadFraction = 0f;

        currentAmmo = data.magazineSize;
        isReloading = false;
    }
}
