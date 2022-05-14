using System;
using UnityEngine;

public class CharacterGrenade : CharacterBehaviour
{
    protected CharacterInputs m_characterInputs;
    protected CharacterEquip m_characterEquip;

    [SerializeField] protected GrenadeSlots m_grenadeSlots1;
    [SerializeField] protected GrenadeSlots m_grenadeSlots2;
    [SerializeField, ReadOnly] protected Grenade m_grenadeBeingEquipped;
    [SerializeField, ReadOnly] protected Grenade m_grenadeEquipped;

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        m_characterInputs = character.characterInputs;
        m_characterEquip = character.characterEquip;

        m_grenadeSlots1.Init();
        m_grenadeSlots2.Init();
    }

    public override void OnUpdateCharacter()
    {
        // process the inputs
        Grenade grenadeToEquip = null;
        if (m_characterInputs.grenade1)
        {
            grenadeToEquip = m_grenadeSlots1.Get();
        }
        else if (m_characterInputs.grenade2)
        {
            grenadeToEquip = m_grenadeSlots2.Get();
        }

        // check if we can equip the grenade
        bool canEquip(Grenade grenade)
        {
            if (grenade == null)
                return false;

            // check if a grenade with same category is already being equipped
            if (m_grenadeBeingEquipped && grenadeToEquip.category == m_grenadeBeingEquipped.category)
                return false;

            // check if a grenade with same category is already equipped
            if (m_grenadeEquipped && grenadeToEquip.category == m_grenadeEquipped.category)
                return false;

            return true;
        };

        // equip the grenade
        if (canEquip(grenadeToEquip))
        {
            EquipData equipData = new EquipData();
            equipData.equipable = grenadeToEquip;
            equipData.equipableObject = grenadeToEquip.gameObject;
            equipData.OnUpdate = this.OnUpdateEquipStatus;
            equipData.equipSpeed = .5f;
            equipData.unequipSpeed = .5f;
            equipData.parentOnUnequip = null;

            if (m_characterEquip.leftHand.Equip(equipData))
            {
                m_grenadeBeingEquipped = grenadeToEquip;
            }
        }

        // fire the grenade
        if (m_characterInputs.fire1)
        {
            ThrowGrenade(m_grenadeEquipped);
            return;
        }
    }

    protected bool AddGrenade(Grenade grenade)
    {
        if (grenade == null)
            return false;

        if (m_grenadeSlots1.Add(grenade))
        {
            grenade.OnEquip();
            return true;
        }

        if (m_grenadeSlots2.Add(grenade))
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

        if (m_grenadeSlots1.category == category)
        {
            grenade = m_grenadeSlots1.Get();
            if (grenade != null)
            {
                return grenade;
            }
        }

        if (m_grenadeSlots2.category == category)
        {
            grenade = m_grenadeSlots2.Get();
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

        if (m_grenadeSlots1.category == category)
        {
            grenade = m_grenadeSlots1.Pop();
            if (grenade != null)
            {
                return grenade;
            }
        }

        if (m_grenadeSlots2.category == category)
        {
            grenade = m_grenadeSlots2.Pop();
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

    protected void ThrowGrenade(Grenade grenade)
    {
    }

    public void OnGrenadeFound(Grenade grenade)
    {
        AddGrenade(grenade);
    }

    //////////////////////////////////////////////////////////////////
    /// CharacterEquip update evenets
    //////////////////////////////////////////////////////////////////

    protected void OnUpdateEquipStatus(IEquipable equipable, EquipStatus equipStatus, float meter)
    {
        if (equipStatus == EquipStatus.Unequip || equipStatus == EquipStatus.Unequipped)
        {
            m_grenadeBeingEquipped = null;
            return;
        }

        if (equipStatus == EquipStatus.Equipped)
        {
            m_grenadeEquipped = m_grenadeBeingEquipped;
            m_grenadeBeingEquipped = null;
        }
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