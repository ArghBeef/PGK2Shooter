using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(NetworkObject))]
public class Bullet : NetworkBehaviour
{
    /* public API ----------------------------------------------------------- */
    /// <summary>Called by the weapon *server-side only* just before Spawn().</summary>
    public void ServerSetup(float dmg, Vector3 vel)
    {
        damage = dmg;
        _cachedVelocity = vel;           // store until OnNetworkSpawn
    }

    /* private -------------------------------------------------------------- */
    [HideInInspector] public float damage;

    readonly NetworkVariable<Vector3> initialVel =
        new(Vector3.zero, writePerm: NetworkVariableWritePermission.Server);

    Rigidbody rb;
    Vector3 _cachedVelocity;

    void Awake() => rb = GetComponent<Rigidbody>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            initialVel.Value = _cachedVelocity;   // safe: variable is wired up
            rb.linearVelocity = _cachedVelocity;
        }
        else
        {
            rb.linearVelocity = initialVel.Value;       // remote peers
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (!IsServer || !NetworkObject.IsSpawned) return;

        if (col.collider.TryGetComponent<Health>(out var hp))
            hp.TakeDamage(damage);

        NetworkObject.Despawn();
    }
}
