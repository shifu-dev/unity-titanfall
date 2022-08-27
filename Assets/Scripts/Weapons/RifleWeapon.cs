using System;
using UnityEngine;

public class RifleWeapon : ReloadableWeapon
{
    [NonSerialized]
    protected new RifleWeaponAsset _asset;
    public new RifleWeaponAsset sourceAsset => _asset;

    public override WeaponCategory category => WeaponCategory.ASSAULT;

    protected override void Init(WeaponInitializer initializer)
    {
        base.Init(initializer);

        _bulletSource = initializer.bulletDataSource;

        _asset = initializer.source as RifleWeaponAsset;
        if (_asset is not null)
        {
            _bulletSource = _asset.bulletSource;
            _fireRate = _asset.fireRate;
            _fireForce = _asset.fireForce;
            _fireRecoil = _asset.fireRecoil;
            _fireAudio = _asset.fireAudio;

            _capacity = _asset.capacity;
            _current = _asset.initial;
        }
    }
}