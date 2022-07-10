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

    [Serializable]
    protected struct GrenadeSlots
    {
        [SerializeField] private GrenadeCategory m_category;
        public GrenadeCategory category => m_category;

        [SerializeField] private bool m_fixedCategory;
        public bool fixedCategory => m_fixedCategory;

        [SerializeField] private int m_count;
        public int count => m_count;

        [SerializeField] private int m_capacity;
        public int capacity => grenades.Length;

        [SerializeField] private Transform[] m_transforms;
        public Transform[] transforms => m_transforms;

        [SerializeField] private Grenade[] m_grenades;
        public Grenade[] grenades => m_grenades;

        public void Init()
        {
            m_count = 0;

            if (m_grenades == null)
            {
                m_grenades = new Grenade[0];
            }

            if (m_fixedCategory == false)
            {
                m_category = GrenadeCategory.Unknown;
            }
        }

        public bool Add(Grenade grenade)
        {
            if (grenade == null || grenade.category == GrenadeCategory.Unknown)
            {
                return false;
            }

            // check category
            if (category == GrenadeCategory.Unknown)
            {
                if (fixedCategory || m_count > 0)
                {
                    return false;
                }
            }

            // find space
            if (m_count < capacity)
            {
                grenades[m_count] = grenade;
                m_category = grenade.category;
                m_count++;

                return true;
            }

            return false;
        }

        public Grenade Get()
        {
            int index = m_count - 1;
            if (index < 0)
            {
                return null;
            }

            m_count--;
            if (count == 0)
            {
                m_category = GrenadeCategory.Unknown;
            }

            return grenades[index];
        }

        public Grenade Pop()
        {
            int index = m_count - 1;
            if (index < 0)
            {
                return null;
            }

            m_count--;
            if (count == 0)
            {
                m_category = GrenadeCategory.Unknown;
            }

            return grenades[index];
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    [SerializeField] protected WeaponSlot[] _weaponSlots;
    [SerializeField] protected GrenadeSlots[] _grenadeSlots;

    //////////////////////////////////////////////////////////////////
    /// Weapons
    //////////////////////////////////////////////////////////////////

    public bool ValidateWeapon(int index, Weapon weapon)
    {
        // always allow storing null, works as remove weapon
        if (weapon == null) return true;

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
        if (weapon != null)
        {
            for (int i = 0; i < _weaponSlots.Length; i++)
            {
                if (_weaponSlots[i].weapon == null &&
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
        if (weapon != null)
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
}