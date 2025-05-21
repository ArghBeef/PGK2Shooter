using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(NetworkObject))]
public class Bullet : NetworkBehaviour
{
    [HideInInspector] public float damage = 10f;

    void OnCollisionEnter(Collision col)
    {
        if (!IsServer) return;

        Health hp = col.collider.GetComponent<Health>();
        if (hp) hp.TakeDamageServerRpc(damage);

        NetworkObject.Despawn();
    }
}
