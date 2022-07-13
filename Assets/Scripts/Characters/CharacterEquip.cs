using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    [Serializable]
    protected struct EquipData
    {
        public static readonly EquipData empty;

        public Equipable equipable;
        public float equip_speed;
        public float unequip_speed;
        public int slot;
    }

    protected enum EquipStatus
    {
        Unknown,

        EquipStart,
        Equipping,
        EquipFinish,

        UnequipStart,
        Unequipping,
        UnequipFinish
    }

    protected CharacterInputs _charInputs;
    protected CharacterInventory _charInventory;
    protected CharacterInteraction _charInteraction;

    [Header("Left Hand"), Space]
    [SerializeField, ReadOnly] protected EquipData _leftCurrent;
    [SerializeField, ReadOnly] protected EquipData _leftNext;
    [SerializeField, ReadOnly] protected EquipStatus _leftStatus;
    [SerializeField, ReadOnly, Range(0, 1)] protected float _leftWeight;
    [SerializeField, ReadOnly] protected bool _leftLocked;
    [SerializeField, ReadOnly] protected bool _leftSupporting;

    [Header("Right Hand"), Space]
    [SerializeField, ReadOnly] protected EquipData _rightCurrent;
    [SerializeField, ReadOnly] protected EquipData _rightNext;
    [SerializeField, ReadOnly] protected EquipStatus _rightStatus;
    [SerializeField, ReadOnly, Range(0, 1)] protected float _rightWeight;
    [SerializeField, ReadOnly] protected bool _rightLocked;
    [SerializeField, ReadOnly] protected bool _rightSupporting;
    [SerializeField, ReadOnly] protected bool _rightWeaponReloading;

    public CharacterEquip()
    {
        _leftCurrent = EquipData.empty;
        _leftNext = EquipData.empty;
        _leftStatus = EquipStatus.Unknown;
        _leftWeight = 0f;
        _leftLocked = false;
        _leftSupporting = false;

        _rightCurrent = EquipData.empty;
        _rightNext = EquipData.empty;
        _rightStatus = EquipStatus.Unknown;
        _rightWeight = 0f;
        _rightLocked = false;
        _rightSupporting = false;
    }

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        _charInputs = character.charInputs;
        _charInventory = character.charInventory;
        _charInteraction = character.charInteraction;
    }

    public override void OnUpdateCharacter()
    {
        base.OnUpdateCharacter();

        CheckEquipInputs();

        bool fire1 = _charInputs.use1;
        var right_equip_data = _rightCurrent;
        Weapon right_weapon = _rightCurrent.equipable == null ? null : _rightCurrent.equipable.weapon;
        int right_weapon_slot = _rightCurrent.slot;

        bool processedInput = false;
        if (processedInput == false)
        {
            if (processedInput == false && _charInputs.weapon1)
            {
                processedInput = ProcessWeaponInput(0);
            }

            if (processedInput == false && _charInputs.weapon2)
            {
                processedInput = ProcessWeaponInput(1);
            }

            if (processedInput == false && _charInputs.weapon3)
            {
                processedInput = ProcessWeaponInput(2);
            }

            if (processedInput == false && _charInputs.grenade1)
            {
                processedInput = ProcessGrenadeInput(0);
            }

            if (processedInput == false && _charInputs.grenade2)
            {
                processedInput = ProcessGrenadeInput(1);
            }
        }

        bool ProcessWeaponInput(int slot)
        {
            if (right_weapon_slot == slot && (_rightStatus == EquipStatus.Equipping ||
                _rightStatus == EquipStatus.EquipFinish))
            {
                RightHandUnequip();
                return true;
            }

            Weapon weapon = _charInventory.GetWeapon(slot);
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

    protected virtual void CheckEquipInputs()
    {
        if (_charInputs.action)
        {
            InteractableScanResult scan_result = _charInteraction.FindScanResult(
                (InteractableScanResult scan_result) => scan_result.raycasted);

            Interactable interactable = scan_result.interactable;
            if (interactable != null)
            {
                Equipable equipable = interactable.GetComponent<Equipable>();
                if (equipable != null)
                {
                    if (equipable.weapon)
                    {
                        RightHandPickWeapon(equipable.weapon);
                    }
                }
            }
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Left Hand
    //////////////////////////////////////////////////////////////////

    protected virtual bool LeftHandEquip(EquipData data, bool force = false)
    {
        if (force)
        {
            // open the look if forced
            _leftLocked = false;
        }

        if (_leftLocked)
        {
            return false;
        }

        _leftNext = data;
        _leftStatus = EquipStatus.UnequipStart;

        if (_leftCurrent.equipable != null &&
            _leftCurrent.equipable == _leftNext.equipable)
        {
            _leftCurrent = EquipData.empty;
            _leftStatus = EquipStatus.Unknown;
        }

        return true;
    }

    protected virtual bool LeftHandEquipInstant(EquipData data, bool force = false)
    {
        if (LeftHandEquip(data, force))
        {
            // 0f speed is considered as instant
            _leftNext.equip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void LeftHandUpdate()
    {
        if (_leftStatus == EquipStatus.Unknown && _leftNext.equipable != null)
        {
            _leftCurrent = _leftNext;
            _leftStatus = EquipStatus.EquipStart;
            _leftNext = EquipData.empty;
        }

        switch (_leftStatus)
        {
            case EquipStatus.Unknown:
            case EquipStatus.EquipFinish:
            case EquipStatus.UnequipFinish:
                return;

            case EquipStatus.EquipStart:
            case EquipStatus.Equipping:

                if (_leftStatus == EquipStatus.EquipStart)
                {
                    OnLeftHandUpdate();
                    _leftStatus = EquipStatus.Equipping;
                }

                // 0f speed is considered as instant
                if (_leftCurrent.equip_speed == 0f)
                {
                    _leftWeight = 1f;
                }
                else
                {
                    _leftWeight = Mathf.MoveTowards(_leftWeight,
                        1f, _leftCurrent.equip_speed * delta_time);
                }

                if (_leftWeight == 1f)
                {
                    _leftStatus = EquipStatus.EquipFinish;
                }

                OnLeftHandUpdate();

                break;

            case EquipStatus.UnequipStart:
            case EquipStatus.Unequipping:

                if (_leftStatus == EquipStatus.UnequipStart)
                {
                    OnLeftHandUpdate();
                    _leftStatus = EquipStatus.Unequipping;
                }

                // 0f speed is considered as instant
                if (_leftCurrent.unequip_speed == 0f)
                {
                    _leftWeight = 0f;
                }
                else
                {
                    _leftWeight = Mathf.MoveTowards(_leftWeight,
                        0f, _leftCurrent.unequip_speed * delta_time);
                }

                if (_leftWeight == 0f)
                {
                    _leftStatus = EquipStatus.UnequipFinish;
                }

                OnLeftHandUpdate();

                break;
        }

        // if current is unequipped, empty current
        // so that next can be processed
        if (_leftStatus == EquipStatus.UnequipFinish)
        {
            _leftCurrent = EquipData.empty;
            _leftStatus = EquipStatus.Unknown;
        }
    }

    protected virtual bool LeftHandUnequip(bool force = false)
    {
        if (force)
        {
            // open the look if forced
            _leftLocked = false;
        }

        if (_leftLocked)
        {
            return false;
        }

        _leftStatus = EquipStatus.UnequipStart;
        return true;
    }

    protected virtual bool LeftHandUnequipInstant(bool force = false)
    {
        if (LeftHandUnequip(force))
        {
            _leftCurrent.unequip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void OnLeftHandUpdate()
    {
        Equipable equipable = _leftCurrent.equipable;
        if (equipable == null) return;

        switch (_leftStatus)
        {
            case EquipStatus.EquipStart:
                equipable.OnEquipStart();
                break;

            case EquipStatus.EquipFinish:
                equipable.OnEquipFinish();
                break;

            case EquipStatus.UnequipStart:
                equipable.OnUnequipStart();
                break;

            case EquipStatus.UnequipFinish:
                equipable.OnUnequipFinish();
                break;
        }

        if (equipable.weapon)
        {
            OnLeftWeaponUpdate();
            return;
        }
    }

    protected virtual void OnLeftWeaponUpdate()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// Right Hand
    //////////////////////////////////////////////////////////////////

    protected virtual bool RightHandEquip(EquipData data, bool force = false)
    {
        if (force)
        {
            // open the look if forced
            _rightLocked = false;
        }

        if (_rightLocked)
        {
            return false;
        }

        _rightNext = data;
        _rightStatus = EquipStatus.UnequipStart;

        if (_rightCurrent.equipable != null &&
            _rightCurrent.equipable == _rightNext.equipable)
        {
            _rightCurrent = EquipData.empty;
            _rightStatus = EquipStatus.Unknown;
        }

        return true;
    }

    protected virtual bool RightHandEquipInstant(EquipData data, bool force = false)
    {
        if (RightHandEquip(data, force))
        {
            // 0f speed is considered as instant
            _rightNext.equip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void RightHandUpdate()
    {
        if (_rightStatus == EquipStatus.Unknown && _rightNext.equipable != null)
        {
            _rightCurrent = _rightNext;
            _rightStatus = EquipStatus.EquipStart;
            _rightNext = EquipData.empty;
        }

        switch (_rightStatus)
        {
            case EquipStatus.Unknown:
            case EquipStatus.EquipFinish:
            case EquipStatus.UnequipFinish:
                return;

            case EquipStatus.EquipStart:
            case EquipStatus.Equipping:

                if (_rightStatus == EquipStatus.EquipStart)
                {
                    OnRightHandUpdate();
                    _rightStatus = EquipStatus.Equipping;
                }

                // 0f speed is considered as instant
                if (_rightCurrent.equip_speed == 0f)
                {
                    _rightWeight = 1f;
                }
                else
                {
                    _rightWeight = Mathf.MoveTowards(_rightWeight,
                        1f, _rightCurrent.equip_speed * delta_time);
                }

                if (_rightWeight == 1f)
                {
                    _rightStatus = EquipStatus.EquipFinish;
                }

                OnRightHandUpdate();

                break;

            case EquipStatus.UnequipStart:
            case EquipStatus.Unequipping:

                if (_rightStatus == EquipStatus.UnequipStart)
                {
                    OnRightHandUpdate();
                    _rightStatus = EquipStatus.Unequipping;
                }

                // 0f speed is considered as instant
                if (_rightCurrent.unequip_speed == 0f)
                {
                    _rightWeight = 0f;
                }
                else
                {
                    _rightWeight = Mathf.MoveTowards(_rightWeight,
                        0f, _rightCurrent.unequip_speed * delta_time);
                }

                if (_rightWeight == 0f)
                {
                    _rightStatus = EquipStatus.UnequipFinish;
                }

                OnRightHandUpdate();

                break;
        }

        // if current is unequipped, empty current
        // so that next can be processed
        if (_rightStatus == EquipStatus.UnequipFinish)
        {
            _rightCurrent = EquipData.empty;
            _rightStatus = EquipStatus.Unknown;
        }
    }

    protected virtual bool RightHandUnequip(bool force = false)
    {
        if (force)
        {
            // open the look if forced
            _rightLocked = false;
        }

        if (_rightLocked)
        {
            return false;
        }

        _rightStatus = EquipStatus.UnequipStart;
        return true;
    }

    protected virtual bool RightHandUnequipInstant(bool force = false)
    {
        if (RightHandUnequip(force))
        {
            _rightCurrent.unequip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void OnRightHandUpdate()
    {
        Equipable equipable = _rightCurrent.equipable;
        if (equipable == null) return;

        switch (_rightStatus)
        {
            case EquipStatus.EquipStart:
                equipable.OnEquipStart();
                break;

            case EquipStatus.EquipFinish:
                equipable.OnEquipFinish();
                break;

            case EquipStatus.UnequipStart:
                equipable.OnUnequipStart();
                break;

            case EquipStatus.UnequipFinish:
                equipable.OnUnequipFinish();
                break;
        }

        if (equipable.weapon)
        {
            OnRightWeaponUpdate();
            return;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Right Weapon
    //////////////////////////////////////////////////////////////////

    protected virtual void RightHand(out Weapon weapon, out int slot)
    {
        if (_rightCurrent.equipable != null &&
            _rightCurrent.equipable.weapon != null)
        {
            slot = _rightCurrent.slot;
            weapon = _rightCurrent.equipable.weapon;
            return;
        }

        slot = -1;
        weapon = null;
    }

    protected virtual Weapon RightHandWeapon()
    {
        if (_rightCurrent.equipable != null)
        {
            return _rightCurrent.equipable.weapon;
        }

        return null;
    }

    protected virtual int RightHandWeaponSlot()
    {
        if (_rightCurrent.equipable != null &&
            _rightCurrent.equipable.weapon != null)
        {
            return _rightCurrent.slot;
        }

        return -1;
    }

    /// picks up weapon and stores in inventory
    protected virtual void RightHandPickWeapon(Weapon weapon)
    {
        int slot = GetSlotForWeaponInInventory(weapon);
        if (slot < 0) return;

        RightHand(out Weapon curr_weapon, out int curr_slot);

        if (curr_weapon != null && curr_slot == slot)
        {
            // drop the current weapon
            RightHandUnequipInstant();

            // throw weapon
        }

        // clear the slot for new weapon
        DropWeaponFromInventory(slot);

        bool added = StoreWeaponInInventory(slot, weapon);
        if (added == false) return;

        // added the item to the inventory successfully
        weapon.OnPickup();

        if (curr_weapon != null && curr_slot == slot)
        {
            RightHandEquipWeapon(slot);
        }
    }

    /// picks up weapon temporarily, does not stores in inventory
    protected virtual void RightHandPickWeaponTemp(Weapon weapon)
    {
        RightHandUnequip();

        // added the item to the inventory successfully
        weapon.OnPickup();

        RightHandEquipWeapon(weapon);
    }

    protected virtual void RightHandEquipWeapon(int slot)
    {
        Weapon weapon = GetWeaponInInventory(slot);

        if (weapon != null)
        {
            EquipData equipData = new EquipData();
            equipData.equipable = weapon;
            equipData.equip_speed = 1f;
            equipData.unequip_speed = 1f;
            equipData.slot = slot;

            if (RightHandEquip(equipData))
            {
                weapon.OnEquipStart();
            }
        }
    }

    protected virtual void RightHandEquipWeapon(Weapon weapon)
    {
        EquipData equipData = new EquipData();
        equipData.equipable = weapon;
        equipData.equip_speed = 1f;
        equipData.unequip_speed = 1f;
        equipData.slot = 0;

        if (RightHandEquip(equipData))
        {
            weapon.OnEquipStart();
        }
    }

    protected virtual void RightHandDropWeapon()
    {
        RightHand(out Weapon weapon, out int slot);
        if (weapon != null)
        {
            RightHandUnequipInstant();
            DropWeaponFromInventory(slot);
        }
    }

    protected virtual void OnRightWeaponUpdate()
    {
        Weapon weapon = RightHandWeapon();
        if (weapon == null) return;

        if (_rightStatus != EquipStatus.EquipFinish)
        {
            return;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Character Inventory API
    //////////////////////////////////////////////////////////////////

    protected int GetSlotForWeaponInInventory(Weapon weapon)
    {
        // check for empty slots
        int slot = _charInventory.GetEmptySlotForWeapon(weapon);
        if (slot >= 0) return slot;

        // check if the weapon can enter currently equipped slot
        // useful to swap weapons
        int curr_slot = RightHandWeaponSlot();
        if (curr_slot >= 0)
        {
            if (_charInventory.ValidateWeapon(curr_slot, weapon))
            {
                return curr_slot;
            }
        }

        // return any slot even if slot is already filled
        return _charInventory.GetFirstSlotForWeapon(weapon);
    }

    protected bool StoreWeaponInInventory(int slot, Weapon weapon)
    {
        return _charInventory.StoreWeapon(slot, weapon);
    }

    protected Weapon GetWeaponInInventory(int slot)
    {
        return _charInventory.GetWeapon(slot);
    }

    protected bool DropWeaponFromInventory(int slot)
    {
        Weapon weapon = _charInventory.GetWeapon(slot);
        bool dropped = _charInventory.StoreWeapon(slot, null);

        if (dropped)
        {
            if (weapon != null)
            {
                weapon.OnDrop();
            }

            return true;
        }

        return false;
    }
}