using System;
using UnityEngine;

public class RifleWeapon : ReloadableWeapon
{
    [NonSerialized]
    protected new RifleWeaponAsset _asset;
    public new RifleWeaponAsset asset => _asset;

    public override WeaponCategory category => WeaponCategory.Assault;

    protected override void _Init(WeaponInitializer initializer)
    {
        base._Init(initializer);

        _bulletSource = initializer.bulletDataSource;

        _asset = initializer.source as RifleWeaponAsset;
        if (_asset is not null)
        {
            _bulletSource = _asset.bulletSource;
            _fireRate = _asset.fireRate;
            _fireForce = _asset.fireForce;
            _fireRecoil = _asset.fireRecoil;
            _fireAudio = _asset.fireAudio;

            _ammoCap = _asset.capacity;
            _curAmmo = _asset.initial;
        }
    }
}