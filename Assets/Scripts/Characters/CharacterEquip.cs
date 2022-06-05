using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    public CharacterInputs charInputs { get; protected set; }
    public CharacterInventory charInventory { get; protected set; }

    [SerializeField] protected CharacterEquipHand m_leftHand;
    [SerializeField] protected CharacterEquipHand m_rightHand;

    public CharacterEquip()
    {
        m_leftHand = new CharacterEquipHand(-1);
        m_rightHand = new CharacterEquipHand(1);
    }

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
        charInventory = character.charInventory;
    }

    public override void OnUpdateCharacter()
    {
        bool fire1 = charInputs.use1;
        var rightEquipData = m_rightHand.current;
        Weapon rightWeapon = m_rightHand.current.weapon;
        uint rightWeaponSlot = m_rightHand.current.weaponSlot;

        if (rightWeapon && fire1 && m_rightHand.current.weight.value > 0.9f)
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

        bool ProcessWeaponInput(uint slot)
        {
            if (rightWeaponSlot == slot && (rightEquipData.isEquipping || rightEquipData.isEquipped))
            {
                m_rightHand.Unequip();
                return true;
            }

            Weapon weapon = charInventory.GetWeaponAtSlot(slot);
            if (weapon == null)
                return false;

            CharacterEquipData equipData = new CharacterEquipData();
            equipData.equipable = weapon;
            equipData.equipSpeed = .5f;
            equipData.unequipSpeed = .5f;
            equipData.weapon = weapon;
            equipData.weaponSlot = slot;
            equipData.callback = OnWeaponEquipped;

            return m_rightHand.Equip(equipData);
        }

        bool ProcessGrenadeInput(int slot)
        {
            return true;
        }

        m_leftHand.Update();
        m_rightHand.Update();
    }

    public void OnWeaponFound(InteractableScanResult scanResult, Weapon weapon)
    {
        Debug.Log($"CharacterEquip | WeaponFound {{{weapon.name}}}");
        uint slot = charInventory.AddWeapon(weapon);
        if (slot > 0)
        {
            scanResult.interactable.canInteract = false;

            Debug.Log($"CharacterEquip | Weapon sent to inventory {{{weapon.name}, {{{slot}}}}}");
            CharacterEquipData equipData = new CharacterEquipData();
            equipData.equipable = weapon;
            equipData.equipSpeed = 1f;
            equipData.unequipSpeed = 1f;
            equipData.weapon = weapon;
            equipData.weaponSlot = slot;
            equipData.callback = OnWeaponEquipped;

            if (m_rightHand.Equip(equipData))
            {
                weapon.OnPickup();
                Debug.Log($"CharacterEquip | EquipWeapon {{{weapon.name}}}");
            }
        }
    }

    public void OnWeaponEquipped(CharacterEquipData equipData)
    {
        var weapon = equipData.weapon;
        if (weapon)
        {
            if (equipData.isEquipped)
            {
                weapon.OnEquip();
            }

            if (equipData.isUnequipped)
            {
                weapon.OnUnequip();
            }

            return;
        }
    }

    public void OnGrenadeFound(Grenade grenade)
    {
    }
}

[Serializable]
public struct CharacterEquipHand
{
    public static readonly CharacterEquipHand invalid;

    [SerializeField] private GameObject m_source;
    [SerializeField] private bool m_locked;

    [SerializeField, ReadOnly] private CharacterEquipData m_current;
    public CharacterEquipData current => m_current;

    [SerializeField, ReadOnly] private CharacterEquipData m_next;
    public CharacterEquipData next => m_next;

    public CharacterEquipHand(int id)
    {
        m_source = null;
        m_locked = false;
        m_current = CharacterEquipData.empty;
        m_next = CharacterEquipData.empty;
    }

    public bool Equip(CharacterEquipData equipData)
    {
        if (m_locked)
        {
            return false;
        }

        if (m_current.status == EquipStatus.Unknown)
        {
            m_current = equipData;
            m_current.status = EquipStatus.Equip;
            m_next = CharacterEquipData.empty;
            return true;
        }

        if (m_current.equipable == equipData.equipable)
        {
            m_current = equipData;
            m_current.status = EquipStatus.Equip;
            m_next = CharacterEquipData.empty;
            return true;
        }

        m_current.status = EquipStatus.Unequip;
        m_next.status = EquipStatus.Equip;
        m_next = equipData;
        return true;
    }

    public bool Unequip()
    {
        m_current.status = EquipStatus.Unequip;
        return true;
    }

    public void Update()
    {
        float targetWeight = 0;
        if (m_current.status == EquipStatus.Equip)
        {
            targetWeight = 1;
        }
        else if (m_current.status == EquipStatus.Unequip)
        {
            targetWeight = 0;
        }
        else
        {
            return;
        }

        if (targetWeight == 1)
        {
            m_current.weight.value = Mathf.MoveTowards(m_current.weight.value,
                targetWeight, m_current.equipSpeed * Time.deltaTime);

            if (m_current.weight.value == targetWeight)
            {
                m_current.status = EquipStatus.Equipped;
            }
        }
        else if (targetWeight == 0)
        {
            m_current.weight.value = Mathf.MoveTowards(m_current.weight.value,
                targetWeight, m_current.unequipSpeed * Time.deltaTime);

            if (m_current.weight.value == targetWeight)
            {
                m_current.status = EquipStatus.Unequipped;
            }
        }

        // if current is equipped
        if (m_current.status == EquipStatus.Equipped)
        {
            GameObject equipableObject = m_current.equipableObject;
            if (equipableObject != null)
            {
                equipableObject.transform.SetParent(m_source.transform, false);
                equipableObject.transform.localPosition = m_current.positionOnEquip;
                equipableObject.transform.localRotation = m_current.rotationOnEquip;
            }
        }

        // if current is unequipped, set next to equip
        if (m_current.status == EquipStatus.Unequipped)
        {
            GameObject equipableObject = m_current.equipableObject;
            if (equipableObject != null)
            {
                equipableObject.transform.SetParent(m_current.parentOnUnequip, false);
                equipableObject.transform.localPosition = m_current.positionOnUnequip;
                equipableObject.transform.localRotation = m_current.rotationOnUnequip;
            }
        }

        if (m_current.callback != null)
        {
            m_current.callback(m_current);
        }

        // if current is unequipped, set next to equip
        if (m_current.status == EquipStatus.Unequipped)
        {
            m_current = CharacterEquipData.empty;

            if (m_next.equipable != null)
            {
                m_current = m_next;
                m_current.status = EquipStatus.Equip;
                m_next = CharacterEquipData.empty;
            }
        }
    }
}

[Serializable]
public struct CharacterEquipData
{
    public static readonly CharacterEquipData empty;

    public Action<CharacterEquipData> callback;

    private IEquipable m_equipable;
    public IEquipable equipable
    {
        get => m_equipable;
        set
        {
            m_equipable = value;
            m_equipableObject = value == null ? null : value.gameObject;
        }
    }

    [SerializeField] private GameObject m_equipableObject;
    public GameObject equipableObject
    {
        get => equipable == null ? null : equipable.gameObject;
    }

    public EquipStatus status;
    public bool isEquipping => status == EquipStatus.Equip;
    public bool isEquipped => status == EquipStatus.Equipped;
    public bool isUnequipping => status == EquipStatus.Unequip;
    public bool isUnequipped => status == EquipStatus.Unequipped;

    public Weight weight;
    public float equipSpeed;
    public float unequipSpeed;
    public Vector3 positionOnEquip;
    public Quaternion rotationOnEquip;
    public Vector3 positionOnUnequip;
    public Quaternion rotationOnUnequip;
    public Transform parentOnUnequip;

    [SerializeField] private Weapon m_weapon;
    [SerializeField] private Grenade m_grenade;
    [SerializeField] private uint m_slot;

    public Weapon weapon
    {
        get => m_weapon;
        set
        {
            m_weapon = value;
            m_grenade = null;
        }
    }
    public uint weaponSlot
    {
        get => weapon ? m_slot : 0;
        set => m_slot = weapon ? value : 0;
    }

    public Grenade grenade
    {
        get => m_grenade;
        set
        {
            m_weapon = null;
            m_grenade = value;
        }
    }
    public uint grenadeSlot
    {
        get => grenade ? m_slot : 0;
        set => m_slot = grenade ? value : 0;
    }
}