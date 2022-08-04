using System;
using UnityEngine;

public class RifleWeapon : ReloadableWeapon
{
    [NonSerialized] private RifleWeaponSource _source;
    public new RifleWeaponSource source => _source;

    public override WeaponCategory category => WeaponCategory.ASSAULT;

    protected override void Init(WeaponInitializer initializer)
    {
        base.Init(initializer);

        _source = initializer.source as RifleWeaponSource;
    }
}