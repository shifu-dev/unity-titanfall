using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterInventory : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public CharacterInputs charInputs { get; protected set; }
    public bool autoPickup = true;

    [SerializeField] protected WeaponSlot[] m_weaponSlots;
    [SerializeField] protected GrenadeSlots[] m_grenadeSlots;

    //////////////////////////////////////////////////////////////////
    /// Updates
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
    }

    //////////////////////////////////////////////////////////////////
    /// Weapons
    //////////////////////////////////////////////////////////////////

    public void OnWeaponFound(Weapon weapon)
    {
        if (weapon == null)
            return;

        bool pickup = autoPickup || charInputs.action;

        if (pickup)
        {
            AddWeapon(weapon);
        }
    }

    public uint AddWeapon(Weapon weapon)
    {
        for (uint i = 0; i < m_weaponSlots.Length; i++)
        {
            if (m_weaponSlots[i].Store(weapon))
            {
                return i + 1;
            }
        }

        return 0;
    }

    public bool AddWeaponAtSlot(uint slot, Weapon weapon)
    {
        return m_weaponSlots[slot - 1].Store(weapon);
    }

    public Weapon GetWeaponAtSlot(uint slot)
    {
        if (slot < 1 || slot > m_weaponSlots.Length)
        {
            return null;
        }

        return m_weaponSlots[slot - 1].weapon;
    }

    //////////////////////////////////////////////////////////////////
    /// Grenades
    //////////////////////////////////////////////////////////////////

    public void OnGrenadeFound(Grenade grenade)
    {
    }

    public Grenade GetGrenadeAtSlot(uint slot)
    {
        if (slot < 1 || slot > m_grenadeSlots.Length)
        {
            return null;
        }

        return m_grenadeSlots[slot - 1].Get();
    }
}

[Serializable]
public struct WeaponSlot
{
    public static WeaponSlot invalid;

    public WeaponCategory category;
    public Transform slot;
    public Weapon weapon;

    public WeaponSlot(WeaponCategory category)
    {
        this.category = category;
        this.slot = null;
        this.weapon = null;
    }

    public bool ValidateCategory(Weapon weapon)
    {
        return category.HasFlag(weapon.category);
    }

    public bool Store(Weapon weapon)
    {
        if (ValidateCategory(weapon) == false)
            return false;

        this.weapon = weapon;
        return true;
    }

    public void Remove()
    {
        this.weapon = null;
    }
}

[Serializable]
public struct GrenadeSlots
{
    [SerializeField] private GrenadeCategory m_category;
    public GrenadeCategory category => m_category;

    [SerializeField] private bool m_fixedCategory;
    public bool fixedCategory => m_fixedCategory;

    [SerializeField] private int m_count;
    public int count => m_count;

    [SerializeField] private uint m_capacity;
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
        int slot = m_count - 1;
        if (slot < 0)
        {
            return null;
        }

        m_count--;
        if (count == 0)
        {
            m_category = GrenadeCategory.Unknown;
        }

        return grenades[slot];
    }

    public Grenade Pop()
    {
        int slot = m_count - 1;
        if (slot < 0)
        {
            return null;
        }

        m_count--;
        if (count == 0)
        {
            m_category = GrenadeCategory.Unknown;
        }

        return grenades[slot];
    }
}