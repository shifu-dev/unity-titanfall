using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Bullets/Data Source", fileName = "Weapon Bullet Data Source")]
public class WeaponBulletDataSource : DataSource
{
    [Label("Ammo Name"), SerializeField]
    protected string _ammoName;
    public string ammoName => _ammoName;

    [Label("Prefab"), SerializeField]
    protected WeaponBullet _prefab;
    public WeaponBullet prefab => _prefab;

    public WeaponBullet Instantiate(Vector3 pos, Quaternion rot)
    {
        if (_prefab == null)
        {
            return null;
        }

        return GameObject.Instantiate(_prefab, pos, rot);
    }
}