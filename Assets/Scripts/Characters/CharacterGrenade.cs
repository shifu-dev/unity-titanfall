using System;
using UnityEngine;

public class CharacterGrenade : CharacterBehaviour
{
    protected CharacterInputs inputs;

    [SerializeField] protected GrenadeSlots grenadeSlots1;
    [SerializeField] protected GrenadeSlots grenadeSlots2;

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        inputs = character.characterInputs;

        grenadeSlots1.Init();
        grenadeSlots2.Init();
    }

    public override void OnUpdateCharacter()
    {
        if (inputs.grenadeSlot1)
        {
            Grenade grenade = grenadeSlots1.Get();
            if (grenade != null)
            {
            }
        }
    }

    protected bool AddGrenade(Grenade grenade)
    {
        if (grenade == null)
            return false;

        if (grenadeSlots1.Add(grenade))
        {
            grenade.OnEquip();
            return true;
        }

        if (grenadeSlots2.Add(grenade))
        {
            grenade.OnEquip();
            return true;
        }

        return false;
    }

    protected Grenade GetGrenade(GrenadeCategory category)
    {
        if (category == GrenadeCategory.Unknown)
            return null;

        Grenade grenade = null;

        if (grenadeSlots1.category == category)
        {
            grenade = grenadeSlots1.Get();
            if (grenade != null)
            {
                return grenade;
            }
        }

        if (grenadeSlots2.category == category)
        {
            grenade = grenadeSlots2.Get();
            if (grenade != null)
            {
                return grenade;
            }
        }

        return grenade;
    }

    protected Grenade PopGrenade(GrenadeCategory category)
    {
        if (category == GrenadeCategory.Unknown)
            return null;

        Grenade grenade = null;

        if (grenadeSlots1.category == category)
        {
            grenade = grenadeSlots1.Pop();
            if (grenade != null)
            {
                return grenade;
            }
        }

        if (grenadeSlots2.category == category)
        {
            grenade = grenadeSlots2.Pop();
            if (grenade != null)
            {
                return grenade;
            }
        }

        return grenade;
    }

    protected void EquipGrenade(GrenadeCategory category)
    {
    }

    protected void UnequipGrenade()
    {
    }

    public void OnGrenadeFound(Grenade grenade)
    {
        AddGrenade(grenade);
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