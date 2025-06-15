using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(ClientNetworkTransform))]
public class Player : NetworkBehaviour
{
    [SerializeField] float baseMovementSpeed = 5f;
    [SerializeField] Transform upperBody;
    [SerializeField] Transform firePoint;
    [SerializeField] WeaponBehaviour weaponPrefab;

    readonly NetworkVariable<NetworkObjectReference> weaponRef =
        new(writePerm: NetworkVariableWritePermission.Server);

    readonly NetworkVariable<Vector3> aimDirection =
        new(Vector3.forward, writePerm: NetworkVariableWritePermission.Owner);

    WeaponBehaviour currentWeapon;
    Rigidbody rb;
    float h, v;

    public WeaponBehaviour CurrentWeapon => currentWeapon;

    public float BaseMovementSpeed
    {
        get => baseMovementSpeed;
        set => baseMovementSpeed = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        weaponRef.OnValueChanged += OnWeaponChanged;

        if (weaponRef.Value.TryGet(out var obj))
            currentWeapon = obj.GetComponent<WeaponBehaviour>();

        AssignUpperBody();

        if (IsServer)
        {
            if (SpawnManager.Instance == null)
            {
                StartCoroutine(WaitForSpawnManagerAndPlace());
                return;
            }

            PlaceAtSpawn((int)OwnerClientId + 1);
        }
    }

    IEnumerator WaitForSpawnManagerAndPlace()
    {
        yield return new WaitUntil(() => SpawnManager.Instance != null);
        PlaceAtSpawn((int)OwnerClientId + 1);
    }

    void PlaceAtSpawn(int index)
    {
        var spawn = SpawnManager.Instance.GetSpawnPointForPlayer(index);
        if (spawn != null)
        {
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }
        else
        {
            Debug.LogError($"No spawn point found for player {OwnerClientId} (index {index})");
        }
    }

    void AssignUpperBody()
    {
        if (upperBody == null)
        {
            var found = transform.Find("Top");
            if (found != null)
            {
                upperBody = found;
            }
            else
            {
                Debug.LogError("Top not found");
            }
        }
    }

    void Update()
    {
        if (upperBody == null) AssignUpperBody();

        if (IsOwner)
        {
            ReadInputs();
            UpdateAim();
        }
        else if (IsClient && IsSpawned && aimDirection.Value != Vector3.zero)
        {
            upperBody.forward = aimDirection.Value;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
    }

    void ReadInputs()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");

        if (currentWeapon?.Data != null)
        {
            bool trigger = currentWeapon.Data.isAutomatic
                           ? Input.GetMouseButton(0)
                           : Input.GetMouseButtonDown(0);

            if (trigger)
                currentWeapon.TryFire(upperBody.forward);
        }
    }

    void Move()
    {
        float speed = baseMovementSpeed *
                     (currentWeapon ? currentWeapon.Data.moveSpeedMultiplier : 1f);

        rb.linearVelocity = new Vector3(h, 0, v) * speed;
        rb.angularVelocity = Vector3.zero;
    }

    void UpdateAim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out var dist))
        {
            Vector3 dir = ray.GetPoint(dist) - upperBody.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                upperBody.forward = dir.normalized;
                aimDirection.Value = dir.normalized;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EquipServerRpc(NetworkObjectReference pickupRef)
    {
        if (!pickupRef.TryGet(out var pickupObj)) return;

        var wp = pickupObj.GetComponent<WeaponPickup>();
        if (wp == null || wp.weaponData == null) return;

        if (weaponPrefab == null) return;

        if (weaponRef.Value.TryGet(out var old))
            old.Despawn();

        var weaponObj = Instantiate(weaponPrefab, transform);
        var netObj = weaponObj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId);

        weaponObj.Initialize(wp.weaponData, firePoint);

        weaponRef.Value = netObj;
    }

    void OnWeaponChanged(NetworkObjectReference _, NetworkObjectReference newRef)
    {
        currentWeapon = newRef.TryGet(out var obj) ? obj.GetComponent<WeaponBehaviour>() : null;
    }

    public void Freeze(bool isFrozen)
    {
        GetComponent<Rigidbody>().isKinematic = isFrozen;
        GetComponent<Collider>().enabled = !isFrozen;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = !isFrozen;
    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
        rb.linearVelocity = Vector3.zero;
    }
}
