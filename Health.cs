using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : NetworkBehaviour
{
    [SerializeField] public float maxHealth = 100f;
    public NetworkVariable<float> current = new();
    public UnityEvent onDeath;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            current.Value = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        ApplyDamage(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float amount) => ApplyDamage(amount);

    void ApplyDamage(float amount)
    {
        if (amount <= 0 || current.Value <= 0) return;

        current.Value = Mathf.Max(current.Value - amount, 0f);
        if (current.Value == 0f) Die();
    }

    public void ResetHealth()
    {
        if (IsServer)
            current.Value = maxHealth;
    }

    void Die()
    {
        onDeath?.Invoke();

        if (!CompareTag("Player"))
        {
            NetworkObject.Despawn();
        }
        else
        {
            GameManager.Instance?.OnPlayerEliminated(OwnerClientId);
            GetComponent<Player>().Freeze(true);
        }
    }
    public void Heal(float amount)
    {
        if (!IsServer) return;

        current.Value = Mathf.Min(current.Value + amount, maxHealth);
    }
}
