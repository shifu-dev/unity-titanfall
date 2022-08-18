using System;
using UnityEngine;

public abstract class FireableWeapon : Weapon
{
    protected WeaponBulletAsset _bulletSource;

    protected float _fireRate;
    protected uint _fireForce;
    protected uint _fireRecoil;
    protected AudioClip _fireAudio;
    protected Transform _fireMuzzleTransform;

    protected float _fireLastTimeFrame;
    protected bool _isTriggerDown;
    protected float _recoil;

    public FireableWeapon()
    {
        _bulletSource = null;
        _fireRate = 0f;
        _fireForce = 0;
        _fireRecoil = 0;
        _fireAudio = null;
        _fireMuzzleTransform = null;
        _fireLastTimeFrame = 0f;
        _isTriggerDown = false;
        _recoil = 0f;
    }

    protected override void Init(WeaponInitializer initializer)
    {
        base.Init(initializer);
    }

    public virtual void OnTriggerDown()
    {
        _isTriggerDown = true;
    }

    public virtual void OnTriggerHold()
    {
        if (CanFire())
        {
            Fire();
        }
    }

    public virtual void OnTriggerUp()
    {
        _isTriggerDown = false;
    }

    public virtual float ConsumeRecoil()
    {
        float recoil = _recoil;
        _recoil = 0f;
        return recoil;
    }

    protected virtual bool CanFire()
    {
        if (_isTriggerDown == false)
            return false;

        if (_bulletSource == null || _fireMuzzleTransform == null)
            return false;

        if (_fireLastTimeFrame + (1f / _fireRate) > Time.time)
            return false;

        return true;
    }

    protected virtual void Fire()
    {
        _fireLastTimeFrame = Time.time;

        WeaponBullet bullet = _bulletSource.Instantiate(
            _fireMuzzleTransform.position, _fireMuzzleTransform.rotation);

        if (bullet != null)
        {
            bullet.Launch(_fireForce);

            _recoil += _fireRecoil;

            if (_fireAudio != null)
            {
                // play fire audio
            }

            OnFire();
        }
    }

    protected virtual void OnFire()
    {
    }
}