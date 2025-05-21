using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Health), typeof(NetworkObject))]
public class ExplosiveBarrel : NetworkBehaviour
{
    [Header("Physics")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float upwardModifier = 1f;


    public GameObject explosionEffectPrefab;
    public float explosionDamage = 60f;

    void Awake() =>
        GetComponent<Health>().onDeath.AddListener(Explode);

    void Explode()
    {
        if (!IsServer) return;

        if (explosionEffectPrefab)
            Destroy(Instantiate(explosionEffectPrefab,
                                 transform.position,
                                 Quaternion.identity), 3f);

        foreach (Collider hit in Physics.OverlapSphere(transform.position, explosionRadius))
        {
            var rb = hit.attachedRigidbody;
            if (rb)
                rb.AddExplosionForce(explosionForce, transform.position,
                                     explosionRadius, upwardModifier,
                                     ForceMode.Impulse);

            var hp = hit.GetComponent<Health>();
            if (hp)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float frac = Mathf.InverseLerp(explosionRadius, 0f, dist);
                hp.TakeDamageServerRpc(explosionDamage * frac);
            }
        }

        NetworkObject.Despawn();
    }
}
