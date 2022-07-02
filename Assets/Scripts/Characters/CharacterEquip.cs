using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    [Serializable]
    protected struct EquipData
    {
        public static readonly EquipData empty;

        public Equipable equipable;
        public Weight weight;
        public float equip_speed;
        public float unequip_speed;
        public uint slot;
    }

    protected enum EquipStatus
    {
        Unknown,
        Equip,
        Equipped,
        Unequip,
        Unequipped
    }

    public CharacterInputs charInputs { get; protected set; }
    public CharacterInventory charInventory { get; protected set; }

    [ReadOnly, SerializeField] protected EquipData _left_current;
    [ReadOnly, SerializeField] protected EquipData _left_next;
    [ReadOnly, SerializeField] protected EquipStatus _left_status;
    [ReadOnly, SerializeField] protected bool _left_locked;

    [ReadOnly, SerializeField] protected EquipData _right_current;
    [ReadOnly, SerializeField] protected EquipData _right_next;
    [ReadOnly, SerializeField] protected EquipStatus _right_status;
    [ReadOnly, SerializeField] protected bool _right_locked;

    protected float deltaTime;

    public CharacterEquip()
    {
        _left_current = EquipData.empty;
        _left_next = EquipData.empty;
        _left_locked = false;

        _right_current = EquipData.empty;
        _right_next = EquipData.empty;
        _right_locked = false;
    }

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
        charInventory = character.charInventory;
    }

    public override void OnUpdateCharacter()
    {
        deltaTime = Time.deltaTime;

        bool fire1 = charInputs.use1;
        var rightEquipData = _right_current;
        Weapon rightWeapon = _right_current.equipable == null ? null : _right_current.equipable.weapon;
        uint rightWeaponSlot = _right_current.slot;

        if (rightWeapon && fire1 && _right_current.weight.value > 0.9f)
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
            if (rightWeaponSlot == slot && (_right_status == EquipStatus.Equip ||
                _right_status == EquipStatus.Equipped))
            {
                RightHandUnequip();
                return true;
            }

            Weapon weapon = charInventory.GetWeaponAtSlot(slot);
            if (weapon == null)
                return false;

            EquipData equipData = new EquipData();
            equipData.equipable = weapon;
            equipData.equip_speed = .5f;
            equipData.unequip_speed = .5f;
            equipData.slot = slot;

            return RightHandEquip(equipData);
        }

        bool ProcessGrenadeInput(int slot)
        {
            return true;
        }

        LeftHandUpdate();
        RightHandUpdate();
    }

    protected virtual bool LeftHandEquip(EquipData data)
    {
        if (_left_locked)
        {
            return false;
        }

        if (_left_status == EquipStatus.Unknown)
        {
            _left_current = data;
            _left_status = EquipStatus.Equip;
            _left_next = EquipData.empty;
            return true;
        }

        if (_left_current.equipable == data.equipable)
        {
            _left_current = data;
            _left_status = EquipStatus.Equip;
            _left_next = EquipData.empty;
            return true;
        }

        _left_next = data;
        _left_status = EquipStatus.Unequip;
        return true;
    }

    protected virtual void LeftHandUpdate()
    {
        if (_right_status == EquipStatus.Unknown ||
            _right_status == EquipStatus.Equipped ||
            _right_status == EquipStatus.Unequipped)
        {
            return;
        }
        else if (_left_status == EquipStatus.Equip)
        {
            _left_current.weight.value = Mathf.MoveTowards(_left_current.weight.value,
                0f, _left_current.equip_speed * deltaTime);

            if (_left_current.weight.value == 0f)
            {
                _left_status = EquipStatus.Equipped;
            }
        }
        else if (_left_status == EquipStatus.Unequip)
        {
            _left_current.weight.value = Mathf.MoveTowards(_left_current.weight.value,
                1f, _left_current.unequip_speed * deltaTime);

            if (_left_current.weight.value == 1f)
            {
                _left_status = EquipStatus.Unequipped;
            }
        }

        OnLeftHandUpdate();

        // if current is unequipped, set next to equip
        if (_left_status == EquipStatus.Unequipped)
        {
            _left_current = EquipData.empty;

            if (_left_next.equipable != null)
            {
                _left_current = _left_next;
                _left_status = EquipStatus.Equip;
                _left_next = EquipData.empty;

                OnLeftHandUpdate();
            }
        }
    }

    protected virtual bool LeftHandUnequip()
    {
        _left_status = EquipStatus.Unequip;
        return true;
    }

    protected virtual void OnLeftHandUpdate()
    {
    }

    protected virtual bool RightHandEquip(EquipData data)
    {
        if (_right_locked)
        {
            return false;
        }

        if (_right_status == EquipStatus.Unknown)
        {
            _right_current = data;
            _right_status = EquipStatus.Equip;
            _right_next = EquipData.empty;
            return true;
        }

        if (_right_current.equipable != null &&
            _right_current.equipable == data.equipable)
        {
            data.weight = _right_current.weight;

            _right_current = data;
            _right_status = EquipStatus.Equip;
            _right_next = EquipData.empty;
            return true;
        }

        _right_next = data;
        _right_status = EquipStatus.Unequip;
        return true;
    }

    protected virtual void RightHandUpdate()
    {
        if (_right_status == EquipStatus.Unknown ||
            _right_status == EquipStatus.Equipped ||
            _right_status == EquipStatus.Unequipped)
        {
            return;
        }
        else if (_right_status == EquipStatus.Equip)
        {
            _right_current.weight.value = Mathf.MoveTowards(_right_current.weight.value,
                1f, _right_current.equip_speed * deltaTime);

            if (_right_current.weight.value == 1f)
            {
                _right_status = EquipStatus.Equipped;
            }
        }
        else if (_right_status == EquipStatus.Unequip)
        {
            _right_current.weight.value = Mathf.MoveTowards(_right_current.weight.value,
                0f, _right_current.unequip_speed * deltaTime);

            if (_right_current.weight.value == 0f)
            {
                _right_status = EquipStatus.Unequipped;
            }
        }

        OnRightHandUpdate();

        // if current is unequipped, set next to equip
        if (_right_status == EquipStatus.Unequipped)
        {
            _right_current = EquipData.empty;
            _right_status = EquipStatus.Unknown;

            if (_right_next.equipable != null)
            {
                _right_current = _right_next;
                _right_status = EquipStatus.Equip;
                _right_next = EquipData.empty;

                OnRightHandUpdate();
            }
        }
    }

    protected virtual bool RightHandUnequip()
    {
        _right_status = EquipStatus.Unequip;
        return true;
    }

    protected virtual void OnRightHandUpdate()
    {
        if (_right_status == EquipStatus.Equipped)
        {
            if (_right_current.equipable.weapon)
            {
                OnRightWeaponEquipped();
            }
        }
    }

    protected virtual void OnRightWeaponEquipped()
    {
        var weapon = _right_current.equipable.weapon;
        if (weapon)
        {
            Debug.Log("CharacterEquip: OnRightWeaponEquipped");

            // if (equipData.isEquipped)
            // {
            //     weapon.OnEquip();
            // }

            // if (equipData.isUnequipped)
            // {
            //     weapon.OnUnequip();
            // }

            return;
        }
    }

    public void OnWeaponFound(InteractableScanResult scanResult, Weapon weapon)
    {
        Debug.Log($"CharacterEquip | WeaponFound {{{weapon.name}}}");
        uint slot = charInventory.AddWeapon(weapon);
        if (slot > 0)
        {
            scanResult.interactable.canInteract = false;

            Debug.Log($"CharacterEquip | Weapon added in inventory {{{weapon.name}, {{{slot}}}}}");
            EquipData equipData = new EquipData();
            equipData.equipable = weapon;
            equipData.equip_speed = 1f;
            equipData.unequip_speed = 1f;
            equipData.slot = slot;

            if (RightHandEquip(equipData))
            {
                weapon.OnPickup();
                Debug.Log($"CharacterEquip | EquipWeapon {{{weapon.name}}}");
            }
        }
    }

    public void OnGrenadeFound(Grenade grenade)
    {
    }
}