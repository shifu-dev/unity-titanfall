using System;
using UnityEngine;

[Flags]
public enum WeaponCategory
{
    MELEE = 2 << 1,
    PISTOL = 2 << 2,
    LIGHT = 2 << 3,
    ASSAULT = 2 << 4,
    HEAVY = 2 << 5,
    SNIPER = 2 << 6,
    SPECIAL = 2 << 7,
    MISC = 2 << 8
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