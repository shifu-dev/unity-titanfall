using System;
using UnityEngine;

public class CharacterWeapon : CharacterBehaviour
{
    [SerializeField] protected GrenadeSlots grenadeSlots1;
    [SerializeField] protected GrenadeSlots grenadeSlots2;

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        grenadeSlots1.Init();
        grenadeSlots2.Init();
    }

    public void OnGrenadeFound(Grenade grenade)
    {
        PickGrenade(grenade);
    }

    public bool PickGrenade(Grenade grenade)
    {
        if (grenadeSlots1.Equip(grenade))
            return true;

        if (grenadeSlots2.Equip(grenade))
            return true;

        return false;
    }

    public bool DropGrenade(GrenadeCategory category)
    {
        return true;
    }

    public void DropAllGrenades(GrenadeCategory category)
    {
    }

    public void DropAllGrenades()
    {
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

    public int capacity => grenades.Length;

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

    public bool WantsGrenade(Grenade grenade)
    {
        if (grenade == null || grenade.category == GrenadeCategory.Unknown)
            return false;

        // no space left
        if (m_count == capacity)
            return false;

        // check category

        if (category == GrenadeCategory.Unknown)
        {
            if (fixedCategory || m_count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public int CanPushGrenade(Grenade grenade)
    {
        if (grenade == null || grenade.category == GrenadeCategory.Unknown)
            return -1;

        // check category
        if (category == GrenadeCategory.Unknown)
        {
            if (fixedCategory || m_count > 0)
            {
                return -1;
            }
        }

        // find space
        if (m_count < capacity)
        {
            return m_count - 1;
        }

        return -1;
    }

    public bool Equip(Grenade grenade)
    {
        if (grenade == null || grenade.category == GrenadeCategory.Unknown)
            return false;

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
            m_count++;
            m_category = grenade.category;
            grenade.OnEquip();

            return true;
        }

        return false;
    }

    public Grenade Unequip()
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

        grenades[slot].OnUnequip();
        return grenades[slot];
    }
}