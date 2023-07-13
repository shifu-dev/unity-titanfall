using System;
using UnityEngine;

[Flags]
public enum WeaponCategory
{
    Melee = 1 << 0,
    Pistol = 1 << 1,
    Light = 1 << 2,
    Assault = 1 << 3,
    Heavy = 1 << 4,
    Sniper = 1 << 5,
    Special = 1 << 6,
    Misc = 1 << 7
}

[DisallowMultipleComponent]
public abstract class Weapon : Equipable
{
    protected WeaponAsset _asset;
    public WeaponAsset sourceAsset => _asset;

    public abstract WeaponCategory category { get; }

    protected virtual void Awake()
    {
        WeaponInitializer initializer = GetComponent<WeaponInitializer>();

        if (initializer)
        {
            Init(initializer);

            if (initializer.destroyAfterUse)
            {
                Destroy(initializer);
            }
        }
    }

    protected virtual void Init(WeaponInitializer initializer)
    {
        _asset = initializer.source as WeaponAsset;
    }
}