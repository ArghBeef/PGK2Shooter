using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Collider))]
public class WeaponPickup : NetworkBehaviour
{
    public WeaponSettings weaponData;

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (!other.TryGetComponent<Player>(out var p)) return;

        p.EquipServerRpc(NetworkObject);
    }
}
