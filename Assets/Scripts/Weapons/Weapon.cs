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
    private WeaponDataSource _source;
    public WeaponDataSource source => _source;

    public abstract WeaponCategory category { get; }

    public virtual void Init()
    {
        WeaponInitializer initializer = GetComponent<WeaponInitializer>();

        if (initializer)
        {
            OnInit(initializer);

            if (initializer.destroyAfterUse)
            {
                Destroy(initializer);
            }
        }
    }

    protected virtual void OnInit(WeaponInitializer initializer)
    {
        _source = initializer.source as WeaponDataSource;
    }
}