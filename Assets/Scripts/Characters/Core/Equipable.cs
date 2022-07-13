using UnityEngine;

public enum EquipableType
{
    Character,
    Weapon,
    Grenade
}

public abstract class Equipable : Interactable
{
    public abstract EquipableType type { get; }

    public virtual Weapon weapon => GetComponent<Weapon>();
    public virtual Grenade grenade => GetComponent<Grenade>();

    [Header("EQUIPABLE ----------------------------------------------------------------------------------------------------"), Space]
    [SerializeField] protected Collider[] _colliders;

    public virtual void OnPickup()
    {
        canInteract = false;
        DisableColliders();
    }

    public virtual void OnDrop()
    {
        canInteract = true;
        EnableColliders();
    }

    public virtual void OnEquipStart()
    {
    }

    public virtual void OnEquipFinish()
    {
    }

    public virtual void OnUnequipStart()
    {
    }

    public virtual void OnUnequipFinish()
    {
    }

    protected virtual void DisableColliders()
    {
        foreach (var collider in _colliders)
        {
            collider.enabled = false;
        }
    }

    protected virtual void EnableColliders()
    {
        foreach (var collider in _colliders)
        {
            collider.enabled = true;
        }
    }
}