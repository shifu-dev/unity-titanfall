using System;
using UnityEngine;

public abstract class ReloadableWeapon : FireableWeapon
{
    protected uint _capacity;
    public uint capacity => _capacity;

    protected uint _cur;
    public uint current => _cur;

    protected bool _hasMag;
    public bool hasMag => _hasMag;

    protected bool _isTriggered;
    public bool isMagTriggered => _isTriggered;

    public ReloadableWeapon()
    {
        _capacity = 0;
        _cur = 0;
        _hasMag = false;
        _isTriggered = false;
    }

    protected override void Init(WeaponInitializer initializer)
    {
        base.Init(initializer);

        _isTriggered = true;
        _hasMag = true;
    }

    protected override bool CanFire()
    {
        if (base.CanFire() == false)
        {
            return false;
        }

        if (_hasMag == false || _cur == 0 ||
            _isTriggered == false)
            return false;

        return true;
    }

    public virtual bool ShouldReload()
    {
        return _hasMag == false || _cur < _capacity;
    }

    public virtual bool NeedReload()
    {
        return _hasMag == false || _cur == 0;
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
        _isTriggered = false;
    }

    public virtual void OnTrigger()
    {
        _isTriggered = true;
    }
}