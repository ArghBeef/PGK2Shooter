using System.Collections.Generic;
using UnityEngine;

public partial class WeaponSettings
{
    private static readonly Dictionary<int, WeaponSettings> _byId = new();
    public int weaponId = 1;

    public static WeaponSettings Get(int id) =>
        _byId.TryGetValue(id, out var ws) ? ws : null;

    void OnEnable() => _byId[weaponId] = this;
#if UNITY_EDITOR
    void OnValidate() => _byId[weaponId] = this;
#endif
}
