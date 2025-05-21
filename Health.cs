using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : NetworkBehaviour
{
    //Health script

    [Header("Health")]
    public float maxHealth = 100f;
    public NetworkVariable<float> current = new();

    [Header("Event")]
    public UnityEvent onDeath;

    public override void OnNetworkSpawn()
    {
        if (IsServer) current.Value = maxHealth;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float amount)
    {
        if (amount <= 0f || current.Value <= 0f) return;

        current.Value = Mathf.Max(current.Value - amount, 0f);
        if (current.Value == 0f) Die();
    }

    void Die()
    {
        onDeath?.Invoke();
        if (!CompareTag("Player"))
            NetworkObject.Despawn();
        else
            Debug.Log("Player ded");
    }
}
