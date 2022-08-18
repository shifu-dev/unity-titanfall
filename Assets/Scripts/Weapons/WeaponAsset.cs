using UnityEngine;

public abstract class WeaponAsset : DataAsset
{
    public const string MENU_PATH = "Weapon/";

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
            // deactivate the prefab to avoid calling Awake()
            // on instantiated instances
            _prefab.gameObject.SetActive(false);
            GameObject instance = GameObject.Instantiate(_prefab.gameObject, pos, rot, parent);
            _prefab.gameObject.SetActive(true);

            Weapon weapon = instance.GetComponent<Weapon>();

            AddWeaponInitializer(instance);

            instance.SetActive(true);
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