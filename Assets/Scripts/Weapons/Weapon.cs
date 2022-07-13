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
    //////////////////////////////////////////////////////////////////
    /// Equipable | START

    public override EquipableType type => EquipableType.Grenade;
    public override Weapon weapon => this;

    /// Equipable | END
    //////////////////////////////////////////////////////////////////

    public WeaponDataSource source => _source;
    [NonSerialized] private WeaponDataSource _source;

    public abstract WeaponCategory category { get; }

    public virtual void Init()
    {
        WeaponInitializer initializer = GetComponent<WeaponInitializer>();

        if (initializer)
        {
            SetSource(initializer.source);

            if (initializer.destroyAfterUse)
            {
                Destroy(initializer);
            }
        }
    }

    protected virtual void SetSource(WeaponDataSource source)
    {
        _source = source;
    }

    public virtual void ShowOff()
    {
    }
}