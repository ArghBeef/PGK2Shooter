using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health), typeof(NetworkObject))]
public class ExplosiveBarrel : NetworkBehaviour
{
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionForce = 700f;
    [SerializeField] float upwardModifier = 1f;
    [SerializeField] GameObject explosionEffectPrefab;
    [SerializeField] float explosionDamage = 60f;

    Renderer[] renderers;
    Collider[] colliders;

    void Awake()
    {
        GetComponent<Health>().onDeath.AddListener(Explode);
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void Explode()
    {
        if (!IsServer) return;

        SpawnExplosionClientRpc(transform.position);

        foreach (Collider hit in Physics.OverlapSphere(transform.position, explosionRadius))
        {
            if (hit.TryGetComponent(out Health hp) && hp != GetComponent<Health>())
            {
                float frac = Mathf.InverseLerp(explosionRadius, 0f,
                                               Vector3.Distance(transform.position, hit.transform.position));
                hp.TakeDamageServerRpc(explosionDamage * frac);
            }

            if (hit.attachedRigidbody != null)
            {
                hit.attachedRigidbody.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );
            }
        }

        HideBarrelClientRpc();
        StartCoroutine(DelayedDespawn());
    }

    IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.05f);
        NetworkObject.Despawn(true);
    }

    [ClientRpc]
    void SpawnExplosionClientRpc(Vector3 pos)
    {
        if (explosionEffectPrefab != null)
        {
            var fx = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
            Destroy(fx, 3f);
        }
    }

    [ClientRpc]
    void HideBarrelClientRpc()
    {
        foreach (var r in renderers)
            r.enabled = false;

        foreach (var c in colliders)
            c.enabled = false;
    }
}
