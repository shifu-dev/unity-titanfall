using System;
using UnityEngine;

public abstract class ReloadableWeapon : FireableWeapon
{
    public new ReloadableWeaponDataSource source => _source;
    [NonSerialized] private ReloadableWeaponDataSource _source;

    protected bool _hasMag;
    public bool hasMag => _hasMag;

    protected bool _isMagTriggered;
    public bool isMagTriggered => _isMagTriggered;

    protected override void SetSource(WeaponDataSource source)
    {
        base.SetSource(source);

        _source = source as ReloadableWeaponDataSource;
    }

    public virtual bool CanReload()
    {
        return false;
    }

    public virtual bool NeedReload()
    {
        return false;
    }

    public virtual void OnStartReload()
    {
    }

    public virtual void OnStopReload()
    {
    }

    public void OnMagIn()
    {
        _hasMag = true;
    }

    public void OnMagOut()
    {
        _hasMag = false;
        _isMagTriggered = false;
    }

    public void OnMagTrigger()
    {
        _isMagTriggered = true;
    }
}