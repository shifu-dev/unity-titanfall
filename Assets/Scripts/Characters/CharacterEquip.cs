using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    [Serializable]
    protected struct EquipData
    {
        public static readonly EquipData empty;

        public Equipable equipable;
        public float weight;
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

    [SerializeField, ReadOnly] protected EquipData l_current;
    [SerializeField, ReadOnly] protected EquipData l_next;
    [SerializeField, ReadOnly] protected EquipStatus l_status;
    [SerializeField, ReadOnly] protected bool l_locked;
    [SerializeField, ReadOnly] protected bool l_supporting;

    [SerializeField, ReadOnly] protected EquipData r_current;
    [SerializeField, ReadOnly] protected EquipData r_next;
    [SerializeField, ReadOnly] protected EquipStatus r_status;
    [SerializeField, ReadOnly] protected bool r_locked;
    [SerializeField, ReadOnly] protected bool r_supporting;

    protected float deltaTime;

    public CharacterEquip()
    {
        l_current = EquipData.empty;
        l_next = EquipData.empty;
        l_status = EquipStatus.Unknown;
        l_locked = false;
        l_supporting = false;

        r_current = EquipData.empty;
        r_next = EquipData.empty;
        r_status = EquipStatus.Unknown;
        r_locked = false;
        r_supporting = false;
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
        deltaTime = Time.deltaTime;

        CheckEquipInputs();

        bool fire1 = _charInputs.use1;
        var right_equip_data = r_current;
        Weapon right_weapon = r_current.equipable == null ? null : r_current.equipable.weapon;
        int right_weapon_slot = r_current.slot;

        if (right_weapon && fire1 && r_current.weight > 0.9f)
        {
            right_weapon.OnPrimaryFire();
        }

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
            if (right_weapon_slot == slot && (r_status == EquipStatus.Equipping ||
                r_status == EquipStatus.EquipFinish))
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
            l_locked = false;
        }

        if (l_locked)
        {
            return false;
        }

        l_next = data;
        l_status = EquipStatus.UnequipStart;

        if (l_current.equipable != null &&
            l_current.equipable == l_next.equipable)
        {
            l_next.weight = l_current.weight;
            l_current = EquipData.empty;
            l_status = EquipStatus.Unknown;
        }

        return true;
    }

    protected virtual bool LeftHandEquipInstant(EquipData data, bool force = false)
    {
        if (LeftHandEquip(data, force))
        {
            // 0f speed is considered as instant
            l_next.equip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void LeftHandUpdate()
    {
        if (l_status == EquipStatus.Unknown && l_next.equipable != null)
        {
            l_current = l_next;
            l_status = EquipStatus.EquipStart;
            l_next = EquipData.empty;
        }

        switch (l_status)
        {
            case EquipStatus.Unknown:
            case EquipStatus.EquipFinish:
            case EquipStatus.UnequipFinish:
                return;

            case EquipStatus.EquipStart:
            case EquipStatus.Equipping:

                if (l_status == EquipStatus.EquipStart)
                {
                    OnLeftHandUpdate();
                    l_status = EquipStatus.Equipping;
                }

                // 0f speed is considered as instant
                if (l_current.equip_speed == 0f)
                {
                    l_current.weight = 1f;
                }
                else
                {
                    l_current.weight = Mathf.MoveTowards(l_current.weight,
                        1f, l_current.equip_speed * deltaTime);
                }

                if (l_current.weight == 1f)
                {
                    l_status = EquipStatus.EquipFinish;
                }

                OnLeftHandUpdate();

                break;

            case EquipStatus.UnequipStart:
            case EquipStatus.Unequipping:

                if (l_status == EquipStatus.UnequipStart)
                {
                    OnLeftHandUpdate();
                    l_status = EquipStatus.Unequipping;
                }

                // 0f speed is considered as instant
                if (l_current.unequip_speed == 0f)
                {
                    l_current.weight = 0f;
                }
                else
                {
                    l_current.weight = Mathf.MoveTowards(l_current.weight,
                        0f, l_current.unequip_speed * deltaTime);
                }

                if (l_current.weight == 0f)
                {
                    l_status = EquipStatus.UnequipFinish;
                }

                OnLeftHandUpdate();

                break;
        }

        // if current is unequipped, empty current
        // so that next can be processed
        if (l_status == EquipStatus.UnequipFinish)
        {
            l_current = EquipData.empty;
            l_status = EquipStatus.Unknown;
        }
    }

    protected virtual bool LeftHandUnequip(bool force = false)
    {
        if (force)
        {
            // open the look if forced
            l_locked = false;
        }

        if (l_locked)
        {
            return false;
        }

        l_status = EquipStatus.UnequipStart;
        return true;
    }

    protected virtual bool LeftHandUnequipInstant(bool force = false)
    {
        if (LeftHandUnequip(force))
        {
            l_current.unequip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void OnLeftHandUpdate()
    {
        Equipable equipable = l_current.equipable;
        if (equipable == null) return;

        switch (l_status)
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
            r_locked = false;
        }

        if (r_locked)
        {
            return false;
        }

        r_next = data;
        r_status = EquipStatus.UnequipStart;

        if (r_current.equipable != null &&
            r_current.equipable == r_next.equipable)
        {
            r_next.weight = r_current.weight;
            r_current = EquipData.empty;
            r_status = EquipStatus.Unknown;
        }

        return true;
    }

    protected virtual bool RightHandEquipInstant(EquipData data, bool force = false)
    {
        if (RightHandEquip(data, force))
        {
            // 0f speed is considered as instant
            r_next.equip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void RightHandUpdate()
    {
        if (r_status == EquipStatus.Unknown && r_next.equipable != null)
        {
            r_current = r_next;
            r_status = EquipStatus.EquipStart;
            r_next = EquipData.empty;
        }

        switch (r_status)
        {
            case EquipStatus.Unknown:
            case EquipStatus.EquipFinish:
            case EquipStatus.UnequipFinish:
                return;

            case EquipStatus.EquipStart:
            case EquipStatus.Equipping:

                if (r_status == EquipStatus.EquipStart)
                {
                    OnRightHandUpdate();
                    r_status = EquipStatus.Equipping;
                }

                // 0f speed is considered as instant
                if (r_current.equip_speed == 0f)
                {
                    r_current.weight = 1f;
                }
                else
                {
                    r_current.weight = Mathf.MoveTowards(r_current.weight,
                        1f, r_current.equip_speed * deltaTime);
                }

                if (r_current.weight == 1f)
                {
                    r_status = EquipStatus.EquipFinish;
                }

                OnRightHandUpdate();

                break;

            case EquipStatus.UnequipStart:
            case EquipStatus.Unequipping:

                if (r_status == EquipStatus.UnequipStart)
                {
                    OnRightHandUpdate();
                    r_status = EquipStatus.Unequipping;
                }

                // 0f speed is considered as instant
                if (r_current.unequip_speed == 0f)
                {
                    r_current.weight = 0f;
                }
                else
                {
                    r_current.weight = Mathf.MoveTowards(r_current.weight,
                        0f, r_current.unequip_speed * deltaTime);
                }

                if (r_current.weight == 0f)
                {
                    r_status = EquipStatus.UnequipFinish;
                }

                OnRightHandUpdate();

                break;
        }

        // if current is unequipped, empty current
        // so that next can be processed
        if (r_status == EquipStatus.UnequipFinish)
        {
            r_current = EquipData.empty;
            r_status = EquipStatus.Unknown;
        }
    }

    protected virtual bool RightHandUnequip(bool force = false)
    {
        if (force)
        {
            // open the look if forced
            r_locked = false;
        }

        if (r_locked)
        {
            return false;
        }

        r_status = EquipStatus.UnequipStart;
        return true;
    }

    protected virtual bool RightHandUnequipInstant(bool force = false)
    {
        if (RightHandUnequip(force))
        {
            r_current.unequip_speed = 0f;

            return true;
        }

        return false;
    }

    protected virtual void OnRightHandUpdate()
    {
        Equipable equipable = r_current.equipable;
        if (equipable == null) return;

        switch (r_status)
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

    protected virtual void OnRightWeaponUpdate()
    {
        Weapon weapon = RightHandWeapon();
        if (weapon == null) return;
    }

    protected virtual void OnRightWeaponEquipFinish()
    {
        var weapon = r_current.equipable.weapon;
        if (weapon)
        {
            weapon.OnEquipFinish();

            return;
        }
    }

    protected virtual void RightHand(out Weapon weapon, out int slot)
    {
        if (r_current.equipable != null &&
            r_current.equipable.weapon != null)
        {
            slot = r_current.slot;
            weapon = r_current.equipable.weapon;
            return;
        }

        slot = -1;
        weapon = null;
    }

    protected virtual Weapon RightHandWeapon()
    {
        if (r_current.equipable != null)
        {
            return r_current.equipable.weapon;
        }

        return null;
    }

    protected virtual int RightHandWeaponSlot()
    {
        if (r_current.equipable != null &&
            r_current.equipable.weapon != null)
        {
            return r_current.slot;
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