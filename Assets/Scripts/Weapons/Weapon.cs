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
[RequireComponent(typeof(WeaponInputs))]
public abstract class Weapon : Equipable
{
    public override EquipableType type => EquipableType.Grenade;
    public override Weapon weapon => this;

    public WeaponInputs inputs { get; protected set; }

    public abstract WeaponCategory category { get; }

    //////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////

    protected virtual void Awake()
    {
        inputs = GetComponent<WeaponInputs>();
    }

    public virtual void OnPrimaryFire()
    {
    }

    public virtual void OnSecondaryFire()
    {
    }

    public virtual void OnMelee()
    {
    }
}