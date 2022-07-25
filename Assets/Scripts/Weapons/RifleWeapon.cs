using System;
using UnityEngine;

public class RifleWeapon : ReloadableWeapon
{
    [NonSerialized] private RifleWeaponDataSource _source;
    public new RifleWeaponDataSource source => _source;

    public override WeaponCategory category => WeaponCategory.ASSAULT;

    protected override void Init(WeaponInitializer initializer)
    {
        base.Init(initializer);

        _source = initializer.source as RifleWeaponDataSource;
    }
}