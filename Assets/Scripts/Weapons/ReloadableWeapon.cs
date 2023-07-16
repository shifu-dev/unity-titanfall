using System;
using UnityEngine;

public abstract class ReloadableWeapon : FireableWeapon
{
    public ReloadableWeapon()
    {
        _ammoCap = 0;
        _curAmmo = 0;
        _hasMag = false;
        _isMagTriggered = false;
    }

    protected override void _Init(WeaponInitializer initializer)
    {
        base._Init(initializer);

        _isMagTriggered = true;
        _hasMag = true;
    }

    protected override bool _CanFire()
    {
        if (!base._CanFire())
        {
            return false;
        }

        if (!_hasMag || _curAmmo == 0 || !_isMagTriggered)
            return false;

        return true;
    }

    public virtual bool ShouldReload()
    {
        return !_hasMag || _curAmmo < _ammoCap;
    }

    public virtual bool NeedReload()
    {
        return !_hasMag || _curAmmo == 0;
    }

    public virtual void OnReloadStart()
    {
    }

    public virtual void OnReloadStop()
    {
    }

    public virtual void OnReloadFinish()
    {
    }

    public virtual void OnMagIn()
    {
        _hasMag = true;
    }

    public virtual void OnMagOut()
    {
        _hasMag = false;
        _isMagTriggered = false;
    }

    public virtual void OnTrigger()
    {
        _isMagTriggered = true;
    }

    public uint ammoCap => _ammoCap;
    public uint curAmmo => _curAmmo;
    public bool hasMag => _hasMag;
    public bool isMagTriggered => _isMagTriggered;

    protected uint _ammoCap;
    protected uint _curAmmo;
    protected bool _hasMag;
    protected bool _isMagTriggered;
}