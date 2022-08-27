using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterInventory : CharacterBehaviour
{
    [Serializable]
    protected struct WeaponSlot
    {
        public static WeaponSlot invalid;

        public WeaponCategory category;
        public Transform index;
        public Weapon weapon;
    }

    [SerializeField] protected WeaponSlot[] _weaponSlots;

    //////////////////////////////////////////////////////////////////
    /// Weapons | BEGIN

    protected FireableWeapon _weapon1;
    protected Transform _weapon1Holder;

    protected FireableWeapon _weapon2;
    protected Transform _weapon2Holder;

    public bool ValidateWeapon(int index, Weapon weapon)
    {
        // always allow storing null, works as remove weapon
        if (weapon is null) return true;

        if (index < 0 || index > _weaponSlots.Length) return false;

        return _weaponSlots[index].category.HasFlag(weapon.category);
    }

    public bool StoreWeapon(int index, Weapon weapon)
    {
        if (ValidateWeapon(index, weapon))
        {
            _weaponSlots[index].weapon = weapon;
            return true;
        }

        return false;
    }

    public Weapon GetWeapon(int index)
    {
        if (index < 0 || index > _weaponSlots.Length)
        {
            return null;
        }

        return _weaponSlots[index].weapon;
    }

    public int GetEmptySlotForWeapon(Weapon weapon)
    {
        if (weapon is not null)
        {
            for (int i = 0; i < _weaponSlots.Length; i++)
            {
                if (_weaponSlots[i].weapon is null &&
                    _weaponSlots[i].category.HasFlag(weapon.category))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public int GetFirstSlotForWeapon(Weapon weapon)
    {
        if (weapon is not null)
        {
            for (int i = 0; i < _weaponSlots.Length; i++)
            {
                if (_weaponSlots[i].category.HasFlag(weapon.category))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// Weapons | END
    //////////////////////////////////////////////////////////////////
}