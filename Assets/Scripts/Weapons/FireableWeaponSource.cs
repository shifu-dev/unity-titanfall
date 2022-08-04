using UnityEngine;

public abstract class FireableWeaponSource : WeaponSource
{
    [Header("FIREABLE WEAPON"), Space]

    [Label("Bullet Source"), SerializeField]
    protected WeaponBulletSource _bulletSource;
    public WeaponBulletSource bulletSource => _bulletSource;

    [Label("Fire Rate"), SerializeField]
    protected uint _fireRate;
    public uint fireRate => _fireRate;
    public float fireInterval => 1f / _fireRate;

    [Label("Fire Force"), SerializeField]
    protected uint _fireForce;
    public uint fireForce => _fireForce;

    [Label("Fire Recoil"), SerializeField]
    protected uint _fireRecoil;
    public uint fireRecoil => _fireRecoil;

    [Label("Fire Audio"), SerializeField]
    protected AudioClip _fireAudio;
    public AudioClip fireAudio => _fireAudio;
}