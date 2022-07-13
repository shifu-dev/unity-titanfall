using System;
using UnityEngine;

public abstract class FireableWeapon : Weapon
{
    public new FireableWeaponDataSource source => _source;
    [NonSerialized] private FireableWeaponDataSource _source;

    [Header("FIREABLE WEAPON ----------------------------------------------------------------------------------------------------"), Space]

    [Label("Fire Rate"), SerializeField]
    protected float _fireRate;
    public float fireRate => _fireRate;

    protected override void SetSource(WeaponDataSource source)
    {
        base.SetSource(source);

        _source = source as FireableWeaponDataSource;
    }

    public virtual bool CanFire()
    {
        return true;
    }

    public virtual Vector3 Fire()
    {
        return Vector3.zero;
    }
}