using System;
using UnityEngine;
using System.Collections.Generic;

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

    public WeaponInputs weaponInputs { get; protected set; }

    public abstract WeaponCategory category { get; }
    [SerializeField] protected Collider[] m_colliders;

    //////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////

    protected virtual void Awake()
    {
        weaponInputs = GetComponent<WeaponInputs>();
    }

    public virtual void OnPickup()
    {
        Debug.Log("Weapon | OnPickup");
        DisableColliders();
    }

    public virtual void OnDrop()
    {
        EnableColliders();
    }

    public virtual void OnInventory()
    {
    }

    public virtual void OnEquip()
    {
        Debug.Log("Weapon | OnEquip");
    }

    public virtual void OnEquipFinished()
    {
    }

    public virtual void OnUnequip()
    {
        Debug.Log("Weapon | OnUnequip");
    }

    public virtual void OnUnequipFinished()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// OnUse
    //////////////////////////////////////////////////////////////////

    public virtual void OnPrimaryFire()
    {
    }

    public virtual void OnSecondaryFire()
    {
        Debug.Log("Weapon | OnSecondaryFire");
    }

    public virtual void OnMelee()
    {
        Debug.Log("Weapon | OnMelee");
    }

    protected virtual void DisableColliders()
    {
        foreach (var collider in m_colliders)
        {
            collider.enabled = false;
        }
    }

    protected virtual void EnableColliders()
    {
        foreach (var collider in m_colliders)
        {
            collider.enabled = true;
        }
    }
}