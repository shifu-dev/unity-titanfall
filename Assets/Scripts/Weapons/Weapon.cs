using System;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(WeaponInputs))]
public class Weapon : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public WeaponInputs weaponInputs { get; protected set; }
    public WeaponBehaviour[] weaponBehaviours { get; protected set; }

    //////////////////////////////////////////////////////////////////
    /// Updates
    //////////////////////////////////////////////////////////////////

    void Awake()
    {
        weaponInputs = GetComponent<WeaponInputs>();
        var initializer = GetComponent<WeaponInitializer>();

        CollectBehaviours(weaponInputs);
        InitBehaviours(initializer);
    }

    void Update()
    {
        UpdateBehaviours();
    }

    void FixedUpdate()
    {
        FixedUpdateBehaviours();
    }

    void OnDestroy()
    {
        DestroyBehaviours();
    }

    //////////////////////////////////////////////////////////////////
    /// Behaviours
    //////////////////////////////////////////////////////////////////

    protected virtual void CollectBehaviours(params WeaponBehaviour[] exceptions)
    {
        List<WeaponBehaviour> weaponBehavioursList = new List<WeaponBehaviour>();
        GetComponents<WeaponBehaviour>(weaponBehavioursList);

        // no need to check for exceptional behaviours
        if (exceptions.Length > 0)
        {
            weaponBehavioursList.RemoveAll((WeaponBehaviour behaviour) =>
            {
                foreach (var exceptionBehaviour in exceptions)
                {
                    if (behaviour == exceptionBehaviour)
                        return true;
                }

                return false;
            });
        }

        weaponBehaviours = weaponBehavioursList.ToArray();
    }

    protected virtual void InitBehaviours(WeaponInitializer initializer)
    {
        weaponInputs.OnInitWeapon(this, initializer);

        foreach (var behaviour in weaponBehaviours)
        {
            behaviour.OnInitWeapon(this, initializer);
        }
    }

    protected virtual void UpdateBehaviours()
    {
        weaponInputs.OnUpdateWeapon();

        foreach (var behaviour in weaponBehaviours)
        {
            behaviour.OnUpdateWeapon();
        }
    }

    protected virtual void FixedUpdateBehaviours()
    {
        weaponInputs.OnFixedUpdateWeapon();

        foreach (var behaviour in weaponBehaviours)
        {
            behaviour.OnFixedUpdateWeapon();
        }
    }

    protected virtual void DestroyBehaviours()
    {
        weaponInputs.OnDestroyWeapon();

        foreach (var behaviour in weaponBehaviours)
        {
            behaviour.OnDestroyWeapon();
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////

    public virtual void OnPickup()
    {
    }

    public virtual void OnDrop()
    {
    }

    public virtual void OnInventory()
    {
    }

    public virtual void OnEquip()
    {
    }

    public virtual void OnEquipFinished()
    {
    }

    public virtual void OnUnequip()
    {
    }

    public virtual void OnUnequipFinished()
    {
    }
}