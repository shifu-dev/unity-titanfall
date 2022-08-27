using UnityEngine;

[CreateAssetMenu(menuName = MENU_PATH + MENU_NAME, fileName = FILE_NAME)]
public class WeaponBulletAsset : DataAsset
{
    public const string MENU_PATH = "Weapon Bullet/";
    public const string MENU_NAME = "Bullet Asset";
    public const string FILE_NAME = "Bullet Asset";

    [Label("Ammo Name"), SerializeField]
    protected string _ammoName;
    public string ammoName => _ammoName;

    [Label("Prefab"), SerializeField]
    protected WeaponBullet _prefab;
    public WeaponBullet prefab => _prefab;

    public WeaponBullet Instantiate(Vector3 pos, Quaternion rot)
    {
        if (_prefab is null)
        {
            return null;
        }

        return GameObject.Instantiate(_prefab, pos, rot);
    }
}