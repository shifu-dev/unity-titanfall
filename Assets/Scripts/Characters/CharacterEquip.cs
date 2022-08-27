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

    // protected CharacterInputs _charInputs;
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
    [SerializeField, ReadOnly] protected bool _rightCanStopWeaponReloading;

    // inputs
    protected float _deltaTime;
    protected int _weaponSlot;
    protected int _grenadeSlot;
    protected bool _equip;
    protected bool _leftFire;
    protected bool _rightFire;
    protected bool _sight;
    protected bool _rightReload;

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

    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        _charInventory = character.charInventory;
        _charInteraction = character.charInteraction;
    }

    public override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();

        _deltaTime = Time.deltaTime;
        CheckEquipInputs();

        var rightEquipData = _rightCurrent;
        Weapon rightWeapon = _rightCurrent.equipable as Weapon;
        int rightWeaponSlot = _rightCurrent.slot;

        bool processedInput = false;
        if (processedInput == false && _weaponSlot >= 0 && _weaponSlot <= 2)
        {
            processedInput = ProcessWeaponInput(_weaponSlot);
        }

        if (processedInput == false && _grenadeSlot >= 0 && _grenadeSlot <= 1)
        {
            processedInput = ProcessGrenadeInput(_grenadeSlot);
        }

        // consume the inputs
        _weaponSlot = -1;
        _grenadeSlot = -1;

        bool ProcessWeaponInput(int slot)
        {
            if (rightWeaponSlot == slot && (_rightStatus == EquipStatus.Equipping ||
                _rightStatus == EquipStatus.EquipFinish))
            {
                RightHandUnequip();
                return true;
            }

            Weapon weapon = _charInventory.GetWeapon(slot);
            if (weapon is null)
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

        ResetInputs();
    }

    protected virtual void CheckEquipInputs()
    {
        if (_equip)
        {
            InteractableScanResult scan_result = _charInteraction.FindScanResult(
                (InteractableScanResult scan_result) => scan_result.raycasted);

            Interactable interactable = scan_result.interactable;
            if (interactable is not null)
            {
                Equipable equipable = interactable.GetComponent<Equipable>();
                if (equipable is not null)
                {
                    if (equipable is Weapon)
                    {
                        RightHandPickWeapon(equipable as Weapon);
                    }
                }
            }
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Inputs | BEGIN

    protected virtual void ResetInputs()
    {
        _weaponSlot = -1;
        _grenadeSlot = -1;
        _equip = false;
        _leftFire = false;
        _rightFire = false;
        _sight = false;
        _rightReload = false;
    }

    public virtual void SwitchToHands()
    {
    }

    public virtual void SwitchToWeapon1()
    {
        _weaponSlot = 0;
    }

    public virtual void SwitchToWeapon2()
    {
        _weaponSlot = 1;
    }

    public virtual void SwitchToWeapon3()
    {
        _weaponSlot = 2;
    }

    public virtual void SwitchToGrenade1()
    {
        _grenadeSlot = 0;
    }

    public virtual void SwitchToGrenade2()
    {
        _grenadeSlot = 1;
    }

    public virtual void ReloadLeftWeapon()
    {
        _rightReload = true;
    }

    public virtual void ReloadRightWeapon()
    {
        _rightReload = true;
    }

    public virtual void FireLeftWeapon()
    {
        _leftFire = true;
    }

    public virtual void FireRightWeapon()
    {
        _rightFire = true;
    }

    public virtual void SightWeapon()
    {
        _sight = true;
    }

    public virtual void EquipCommand()
    {
        _equip = true;
    }

    /// Inputs | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Left Hand | BEGIN

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

        if (_leftCurrent.equipable is not null &&
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
        if (_leftStatus == EquipStatus.Unknown && _leftNext.equipable is not null)
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
                        1f, _leftCurrent.equip_speed * _deltaTime);
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
                        0f, _leftCurrent.unequip_speed * _deltaTime);
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
        if (equipable is null) return;

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

        if (equipable is Weapon)
        {
            OnLeftWeaponUpdate();
            return;
        }
    }

    protected virtual void OnLeftWeaponUpdate()
    {
    }

    /// Left Hand | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Right Hand | BEGIN

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

        if (_rightCurrent.equipable is not null &&
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
        if (_rightStatus == EquipStatus.Unknown && _rightNext.equipable is not null)
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
                        1f, _rightCurrent.equip_speed * _deltaTime);
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
                        0f, _rightCurrent.unequip_speed * _deltaTime);
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
        if (equipable is null) return;

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

        if (equipable is Weapon)
        {
            RightHandHandleWeapon();
            return;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Right Weapon | BEGIN

    protected virtual void RightHand(out Weapon weapon, out int slot)
    {
        if (_rightCurrent.equipable is not null &&
            _rightCurrent.equipable is Weapon)
        {
            slot = _rightCurrent.slot;
            weapon = _rightCurrent.equipable as Weapon;
            return;
        }

        slot = -1;
        weapon = null;
    }

    protected virtual Weapon RightHandWeapon()
    {
        if (_rightCurrent.equipable is not null)
        {
            return _rightCurrent.equipable as Weapon;
        }

        return null;
    }

    protected virtual int RightHandWeaponSlot()
    {
        if (_rightCurrent.equipable is not null &&
            _rightCurrent.equipable is Weapon)
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

        if (curr_weapon is not null && curr_slot == slot)
        {
            // drop the current weapon
            RightHandUnequipInstant();

            // TODO: throw weapon
        }

        // clear the slot for new weapon
        DropWeaponFromInventory(slot);

        bool added = StoreWeaponInInventory(slot, weapon);
        if (added == false) return;

        // added the item to the inventory successfully
        weapon.OnPickup();

        // equip the new weapon if we swapped with the current one
        if (curr_weapon is not null && curr_slot == slot)
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

        if (weapon is not null)
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
        if (weapon is not null)
        {
            RightHandUnequipInstant();
            DropWeaponFromInventory(slot);
        }
    }

    protected virtual void RightHandHandleWeapon()
    {
        Weapon weapon = RightHandWeapon();
        if (weapon is null) return;

        if (_rightStatus != EquipStatus.EquipFinish)
        {
            return;
        }

        FireableWeapon fireableWeapon = weapon as FireableWeapon;
        ReloadableWeapon reloadableWeapon = weapon as ReloadableWeapon;
        if (fireableWeapon is not null)
        {
            if (reloadableWeapon is not null)
            {
                if (_rightWeaponReloading == false)
                {
                    if (reloadableWeapon.NeedReload() ||
                        _rightReload && reloadableWeapon.ShouldReload())
                    {
                        StartRightWeaponReload();
                    }
                }
            }

            if (_rightFire)
            {
                if (_rightWeaponReloading && _rightCanStopWeaponReloading)
                {
                    StopRightWeaponReload();
                }

                fireableWeapon.OnTriggerDown();
            }
            else
            {
                fireableWeapon.OnTriggerUp();
            }
        }
    }

    protected virtual void StartRightWeaponReload()
    {
    }

    protected virtual void StopRightWeaponReload()
    {
    }

    /// Right Weapon | END
    //////////////////////////////////////////////////////////////////

    /// Right Hand | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Character Inventory API | BEGIN

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
            if (weapon is not null)
            {
                weapon.OnDrop();
            }

            return true;
        }

        return false;
    }

    /// Character Inventory API | END
    //////////////////////////////////////////////////////////////////
}