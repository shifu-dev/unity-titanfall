using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    public CharacterInputs charInputs { get; protected set; }
    public CharacterInventory charInventory { get; protected set; }

    [SerializeField] protected EquipHand m_leftHand;
    [SerializeField] protected EquipHand m_rightHand;

    public CharacterEquip()
    {
        m_leftHand = new EquipHand(-1);
        m_rightHand = new EquipHand(1);
    }

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.characterInputs;
        charInventory = character.characterInventory;
    }

    public override void OnUpdateCharacter()
    {
        bool fire1 = charInputs.use1;
        Weapon rightWeapon = m_rightHand.current.weapon;
        if (rightWeapon && fire1 && m_rightHand.currentWeight > 0.9f)
        {
            rightWeapon.OnPrimaryFire();
        }

        bool processedInput = false;
        if (processedInput == false)
        {
            if (processedInput == false && charInputs.weapon1)
            {
                processedInput = ProcessWeaponInput(1);
            }

            if (processedInput == false && charInputs.weapon2)
            {
                processedInput = ProcessWeaponInput(2);
            }

            if (processedInput == false && charInputs.weapon3)
            {
                processedInput = ProcessWeaponInput(3);
            }

            if (processedInput == false && charInputs.grenade1)
            {
                processedInput = ProcessGrenadeInput(1);
            }

            if (processedInput == false && charInputs.grenade2)
            {
                processedInput = ProcessGrenadeInput(2);
            }
        }

        bool ProcessWeaponInput(int slot)
        {
            return true;
        }

        bool ProcessGrenadeInput(int slot)
        {
            return true;
        }
    }

    public void OnWeaponFound(Weapon weapon)
    {
        Debug.Log("CharacterEquip| Weapon Found");
        uint slot = charInventory.AddWeapon(weapon);
        if (slot > 0)
        {
            EquipData equipData = new EquipData();
            equipData.equipable = weapon;
            equipData.equipSpeed = .1f;
            equipData.unequipSpeed = .1f;
            equipData.equipableObject = weapon.gameObject;
            equipData.weapon = weapon;

            m_rightHand.Equip(equipData);
        }
    }

    public void OnGrenadeFound(Grenade grenade)
    {
    }

    public void HandleRightWeapon()
    {
    }
}

[Serializable]
public struct EquipHand
{
    public static EquipHand invalid;

    [SerializeField, ReadOnly] private int m_id;
    [SerializeField] private GameObject m_source;
    [SerializeField] public bool locked;
    [SerializeField, ReadOnly] private EquipData m_current;
    public EquipData current => m_current;

    [SerializeField, ReadOnly] private EquipData m_next;

    public int id => m_id;
    public IEquipable currentEquipable => m_current.equipable;
    public EquipStatus currentStatus => m_current.status;
    public float currentWeight => m_current.weight;

    public EquipHand(int id)
    {
        m_id = id;
        m_source = null;
        m_current = default;
        m_next = default;
        locked = false;
    }

    public void Update()
    {
        float targetStatusValue = 0;
        if (m_current.status == EquipStatus.Equip)
        {
            targetStatusValue = 1;
        }
        else if (m_current.status == EquipStatus.Unequip)
        {
            targetStatusValue = 0;
        }
        else
        {
            return;
        }

        if (targetStatusValue == 1)
        {
            m_current.weight = Mathf.MoveTowards(m_current.weight,
                targetStatusValue, m_current.equipSpeed * Time.deltaTime);

            if (m_current.weight == targetStatusValue)
            {
                m_current.status = EquipStatus.Equipped;
            }
        }
        else if (targetStatusValue == 0)
        {
            m_current.weight = Mathf.MoveTowards(m_current.weight,
                targetStatusValue, m_current.unequipSpeed * Time.deltaTime);

            if (m_current.weight == targetStatusValue)
            {
                m_current.status = EquipStatus.Unequipped;
            }
        }

        // if finalState reached
        if (m_current.status == EquipStatus.Equipped)
        {
            GameObject gameObject = m_current.gameObject;
            if (gameObject != null)
            {
                gameObject.transform.SetParent(m_source.transform, false);
                gameObject.transform.localPosition = m_current.localPositionOnEquip;
                gameObject.transform.localRotation = m_current.localRotationOnEquip;
            }
        }
        else if (m_current.status == EquipStatus.Unequipped)
        {
            GameObject gameObject = m_current.gameObject;
            if (gameObject != null)
            {
                gameObject.transform.SetParent(m_current.parentOnUnequip.transform, false);
                gameObject.transform.localPosition = m_current.localPositionOnUnequip;
                gameObject.transform.localRotation = m_current.localRotationOnUnequip;
            }
        }

        // update the events
        if (m_current.OnUpdate != null)
        {
            m_current.OnUpdate(m_current.equipable, m_current.status, m_current.weight);
        }

        // check if there is next to equip
        if (m_current.status == EquipStatus.Unequipped)
        {
            if (m_next.equipable != null)
            {
                m_current = m_next;
                m_current.status = EquipStatus.Equip;

                m_next.Clear();

                // // update the events to provide the fresh values
                // if (m_current.OnUpdate != null)
                // {
                //     m_current.OnUpdate(m_current.equipable, m_current.equipStatus, m_current.equipMeter);
                // }
            }
        }
    }

    public bool CanEquip(IEquipable equipable)
    {
        if (locked)
        {
            return false;
        }

        if (equipable == null)
        {
            return false;
        }

        return true;
    }

    public bool Equip(EquipData equipData)
    {
        if (locked)
        {
            return false;
        }

        if (equipData.equipable == m_current.equipable)
        {
            m_current.status = EquipStatus.Equip;
            m_next = default;
            return true;
        }

        m_current.status = EquipStatus.Unequip;
        m_next = equipData;
        return true;
    }

    public bool Unequip()
    {
        m_current.status = EquipStatus.Unequip;
        return true;
    }
}

[Serializable]
public struct EquipData
{
    public Action<IEquipable, EquipStatus, float> OnUpdate;

    public IEquipable equipable;
    public GameObject equipableObject;
    public EquipStatus status;
    public float weight;
    public float equipSpeed;
    public float unequipSpeed;
    public Vector3 localPositionOnEquip;
    public Quaternion localRotationOnEquip;
    public Vector3 localPositionOnUnequip;
    public Quaternion localRotationOnUnequip;
    public GameObject parentOnUnequip;

    [SerializeField] private Weapon m_weapon;
    public Weapon weapon
    {
        get => m_weapon;
        set
        {
            m_weapon = value;
            m_grenade = null;
        }
    }

    [SerializeField] private Grenade m_grenade;
    public Grenade grenade
    {
        get => m_grenade;
        set
        {
            m_weapon = null;
            m_grenade = value;
        }
    }

    public GameObject gameObject => equipable == null ? null : equipable.gameObject;

    public void Clear()
    {
        this = default;
    }

    public bool isNull
    {
        get
        {
            if (equipable == null)
                return true;

            return false;
        }
    }
}