using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Collider))]
public class HealthKit : NetworkBehaviour
{
    [SerializeField] float healAmount = 50f;

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (!other.TryGetComponent<Health>(out var health)) return;

        if (health.current.Value >= health.maxHealth) return;

        health.Heal(healAmount);
        NetworkObject.Despawn();
    }
}
