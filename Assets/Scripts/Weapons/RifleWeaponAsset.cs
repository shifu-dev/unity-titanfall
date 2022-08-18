using System;
using UnityEngine;

[CreateAssetMenu(menuName = MENU_PATH + MENU_NAME, fileName = FILE_NAME)]
public class RifleWeaponAsset : WeaponAsset
{
    public const string MENU_NAME = "Rifle Weapon Source";
    public const string FILE_NAME = "Rifle Weapon Source";

    [Label("Bullet Source"), SerializeField]
    protected WeaponBulletAsset _bulletSource;
    public WeaponBulletAsset bulletSource => _bulletSource;

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

    [Label("Capacity"), SerializeField]    
    protected uint _capacity;
    public uint capacity => _capacity;

    [Label("Initial"), SerializeField]
    protected uint _initial;
    public uint initial => _initial;
}