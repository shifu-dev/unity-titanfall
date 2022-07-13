using System;
using UnityEngine;

public class RifleWeapon : ReloadableWeapon
{
    public new RifleWeaponDataSource source => _source;
    [NonSerialized] private RifleWeaponDataSource _source;

    public override WeaponCategory category => WeaponCategory.ASSAULT;

    protected override void SetSource(WeaponDataSource source)
    {
        base.SetSource(source);

        _source = source as RifleWeaponDataSource;
    }
}