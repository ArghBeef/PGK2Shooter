using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class WeaponBehaviour : NetworkBehaviour
{
    [HideInInspector] public WeaponSettings Data;
    [HideInInspector] public Transform FirePoint;

    readonly NetworkVariable<int> idCode = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<int> ammo = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<bool> isReloading = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<float> reloadFrac = new(writePerm: NetworkVariableWritePermission.Server);

    float lastShotTime;

    public int CurrentAmmo => ammo.Value;
    public bool IsReloading => isReloading.Value;
    public float ReloadFraction => reloadFrac.Value;

    public override void OnNetworkSpawn()
    {
        idCode.OnValueChanged += (_, newId) =>
        {
            Data = WeaponSettings.Get(newId);
        };

        if (!IsServer && idCode.Value != 0)
            Data = WeaponSettings.Get(idCode.Value);
    }

    public void Initialize(WeaponSettings data, Transform firePoint)
    {
        Data = data;
        FirePoint = firePoint;

        if (IsServer)
        {
            idCode.Value = data.weaponId;
            ammo.Value = data.magazineSize;
        }
    }

    public void TryFire(Vector3 forwardDir)
    {
        if (!IsOwner || Data == null) return;
        if (IsReloading || CurrentAmmo <= 0) return;
        if (Time.time < lastShotTime + Data.shotCooldown) return;

        FireServerRpc(forwardDir);
    }

    [ServerRpc]
    void FireServerRpc(Vector3 dir)
    {
        SpawnProjectiles(dir);
        ammo.Value--;
        lastShotTime = Time.time;

        if (ammo.Value <= 0)
            StartCoroutine(ReloadRoutine());

    }

    void SpawnProjectiles(Vector3 baseDir)
    {
        for (int i = 0; i < Data.bulletsPerShot; i++)
        {
            Vector3 dir = Quaternion.Euler(
                              Random.Range(-Data.spreadAngle, Data.spreadAngle),
                              Random.Range(-Data.spreadAngle, Data.spreadAngle),
                              0f) * baseDir;

            GameObject go = Instantiate(Data.bulletPrefab,
                                         FirePoint.position,
                                         Quaternion.LookRotation(dir));

            Vector3 velocity = dir * Data.bulletSpeed;

            Bullet b = go.GetComponent<Bullet>();
            b.ServerSetup(Data.damage, velocity);

            NetworkObject netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();

            Destroy(go, Data.bulletRange / Data.bulletSpeed);
        }
    }
    IEnumerator ReloadRoutine()
    {
        if (IsReloading) yield break;
        isReloading.Value = true;
        float t = 0f;

        while (t < Data.reloadTime)
        {
            t += Time.deltaTime;
            reloadFrac.Value = t / Data.reloadTime;
            yield return null;
        }

        reloadFrac.Value = 0f;
        ammo.Value = Data.magazineSize;
        isReloading.Value = false;
    }
}
