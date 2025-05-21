using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;

[RequireComponent(typeof(Rigidbody), typeof(ClientNetworkTransform))]
public class Player : NetworkBehaviour
{

    [Header("Movement")]
    public float baseMovementSpeed = 5f;
    public float dashForce = 20f; //idk dash is not functionla
    public float dashCooldown = 1f;

    [Header("Parts")]
    public Transform upperBody; //Later will delete all of this
    public Transform firePoint;
    public WeaponBehaviour currentWeaponPrefab;


    WeaponBehaviour currentWeapon;
    Rigidbody rb;
    float h, v, nextDashTime;

    public WeaponBehaviour CurrentWeapon => currentWeapon; //hud

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!IsOwner) return; 
        ReadInputs();
        UpdateAim();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        UpdateMovement();
    }

    void ReadInputs()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");

        if (currentWeapon)
        {
            bool held = Input.GetMouseButton(0);
            bool down = Input.GetMouseButtonDown(0);
            bool shoot = currentWeapon.data.isAutomatic ? held : down; //held i down for those automatic and one bullet
            if (shoot) currentWeapon.TryFire(upperBody.forward);
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextDashTime)
            Dash();
    }

    void UpdateMovement()
    {
        float speed = baseMovementSpeed *
                      (currentWeapon ? currentWeapon.data.moveSpeedMultiplier : 1f);

        rb.linearVelocity = new Vector3(h, 0f, v) * speed;
        rb.angularVelocity = Vector3.zero;
    }

    void UpdateAim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out var dist))
        {
            Vector3 dir = ray.GetPoint(dist) - upperBody.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                upperBody.forward = dir.normalized;
        }
    }

    //Delete it or change idk
    void Dash()
    {
        nextDashTime = Time.time + dashCooldown;
        rb.AddForce(upperBody.forward * dashForce, ForceMode.Impulse);
    }


    [ServerRpc(RequireOwnership = false)]
    public void EquipServerRpc(NetworkObjectReference pickupRef)
    {
        if (!pickupRef.TryGet(out var pickupObj)) return;

        var wp = pickupObj.GetComponent<WeaponPickup>();
        if (wp == null || wp.weaponData == null) return;

        if (currentWeapon)
            currentWeapon.NetworkObject.Despawn();

        var holder = Instantiate(currentWeaponPrefab, transform);
        var netObj = holder.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId);

        currentWeapon = holder;
        currentWeapon.data = wp.weaponData;
        currentWeapon.firePoint = firePoint;

        pickupObj.Despawn();
    }
}
