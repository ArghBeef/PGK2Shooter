using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Collider))]
public class WeaponPickup : NetworkBehaviour
{
    public WeaponSettings weaponData;

    Renderer[] renderers;
    Collider[] colliders;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Player>(out var player)) return;

        if (IsServer)
        {
            GiveAndDespawn(player);
        }
        else if (player.IsOwner
                 && NetworkObject.IsSpawned
                 && player.NetworkObject.IsSpawned)
        {
            EquipRequestServerRpc(player.NetworkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void EquipRequestServerRpc(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out var playerObj)) return;
        if (!NetworkObject.IsSpawned || weaponData == null) return;

        GiveAndDespawn(playerObj.GetComponent<Player>());
    }

    void GiveAndDespawn(Player player)
    {
        if (player == null || weaponData == null) return;

        player.EquipServerRpc(NetworkObject);

        HidePickupClientRpc();

        NetworkObject.Despawn(true);
    }

    [ClientRpc]
    void HidePickupClientRpc()
    {
        foreach (var r in renderers)
            r.enabled = false;

        foreach (var c in colliders)
            c.enabled = false;
    }
}
