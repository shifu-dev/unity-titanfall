using UnityEngine;

public enum EquipableType
{
    Weapon,
    Grenade
}

public abstract class Equipable : MonoBehaviour
{
    public abstract EquipableType type { get; }

    public virtual Weapon weapon => GetComponent<Weapon>();
    public virtual Grenade grenade => GetComponent<Grenade>();
}