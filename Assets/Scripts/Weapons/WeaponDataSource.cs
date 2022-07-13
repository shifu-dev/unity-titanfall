using UnityEngine;

public abstract class WeaponDataSource : ScriptableObject
{
    [Label("Weapon Name"), SerializeField]
    protected string _weaponName;
    public string weaponName => _weaponName;

    [Label("Prefab"), SerializeField]
    protected Weapon _prefab;
    protected Weapon prefab => _prefab;

    public virtual Weapon Instantiate()
    {
        return Instantiate(Vector3.zero);
    }

    public virtual Weapon Instantiate(Vector3 pos)
    {
        return Instantiate(pos, Quaternion.identity);
    }

    public virtual Weapon Instantiate(Vector3 pos, Quaternion rot)
    {
        return Instantiate(pos, rot, null);
    }

    public virtual Weapon Instantiate(Vector3 pos, Quaternion rot, Transform parent)
    {
        if (_prefab != null)
        {
            GameObject instance = GameObject.Instantiate(_prefab.gameObject, pos, rot, parent);
            Weapon weapon = instance.GetComponent<Weapon>();

            AddWeaponInitializer(instance);

            weapon.Init();

            return weapon;
        }

        return null;
    }

    public virtual WeaponInitializer AddWeaponInitializer(GameObject go)
    {
        WeaponInitializer initializer = go.AddComponent<WeaponInitializer>();
        initializer.destroyAfterUse = true;
        initializer.source = this;

        return initializer;
    }
}