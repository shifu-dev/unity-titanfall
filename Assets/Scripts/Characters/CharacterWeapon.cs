// using System;
// using UnityEngine;

// public class CharacterWeapon : CharacterBehaviour
// {
//     //////////////////////////////////////////////////////////////////
//     /// Variables
//     //////////////////////////////////////////////////////////////////

//     public CharacterInputs charInputs { get; protected set; }
//     public CharacterEquip charEquip { get; protected set; }

//     [SerializeField, ReadOnly] protected Weapon m_leftWeapon;
//     [SerializeField, ReadOnly] protected int m_leftWeaponSlotId;
//     [SerializeField, ReadOnly] protected Weapon m_rightWeapon;
//     [SerializeField, ReadOnly] protected int m_rightWeaponSlotId;
//     [SerializeField] protected WeaponSlot[] m_weaponSlots;

//     public CharacterWeapon()
//     {
//         m_leftWeapon = null;
//         m_leftWeaponSlotId = -1;

//         m_rightWeapon = null;
//         m_rightWeaponSlotId = -1;

//         m_weaponSlots = new WeaponSlot[]
//         {
//             new WeaponSlot(WeaponCategory.LIGHT | WeaponCategory.ASSAULT |
//                 WeaponCategory.HEAVY | WeaponCategory.SNIPER | WeaponCategory.SPECIAL),

//             new WeaponSlot(WeaponCategory.LIGHT | WeaponCategory.ASSAULT |
//                 WeaponCategory.HEAVY | WeaponCategory.SNIPER | WeaponCategory.SPECIAL),

//             new WeaponSlot(WeaponCategory.MELEE | WeaponCategory.PISTOL)
//         };
//     }

//     //////////////////////////////////////////////////////////////////
//     /// WeaponBehaviour
//     //////////////////////////////////////////////////////////////////

//     public override void OnInitCharacter(Character character, CharacterInitializer initializer)
//     {
//         base.OnInitCharacter(character, initializer);

//         charInputs = character.characterInputs;
//         charEquip = character.characterEquip;
//     }

//     public override void OnUpdateCharacter()
//     {
//         return;
//         int slotToEquip = ProcessEquipInput();
//         if (slotToEquip > 0)
//         {
//             EquipSlot(charEquip.rightHand.id, slotToEquip);
//         }
//     }

//     //////////////////////////////////////////////////////////////////
//     /// Weapon Commands
//     //////////////////////////////////////////////////////////////////

//     public void HandleWeapon(Weapon weapon, EquipStatus status, float weight)
//     {
//     }

//     protected int ProcessEquipInput()
//     {
//         if (charInputs.weapon1)
//         {
//             if (m_rightWeaponSlotId == 1)
//             {
//                 return 0;
//             }

//             return 1;
//         }

//         if (charInputs.weapon2)
//         {
//             if (m_rightWeaponSlotId == 2)
//             {
//                 return 0;
//             }

//             return 2;
//         }

//         if (charInputs.weapon3)
//         {
//             if (m_rightWeaponSlotId == 3)
//             {
//                 return 0;
//             }

//             return 3;
//         }

//         return -1;
//     }

//     protected virtual bool EquipSlot(int handId, int slotId)
//     {
//         if (m_rightWeaponSlotId == slotId || slotId > m_weaponSlots.Length || slotId < 0)
//         {
//             return false;
//         }

//         WeaponSlot slot = m_weaponSlots[m_rightWeaponSlotId];
//         if (EquipWeapon(handId, slot.weapon) == false)
//         {
//             return false;
//         }

//         if (handId < 0)
//         {
//             m_leftWeaponSlotId = slotId;
//             return true;
//         }

//         if (handId > 0)
//         {
//             m_rightWeaponSlotId = slotId;
//             return true;
//         }

//         return false;
//     }

//     protected virtual bool UnequipSlot(int handId, int slotId)
//     {
//         if (m_rightWeaponSlotId == slotId || slotId > m_weaponSlots.Length || slotId < 0)
//         {
//             return false;
//         }

//         WeaponSlot slot = m_weaponSlots[m_rightWeaponSlotId];
//         if (EquipWeapon(handId, slot.weapon) == false)
//         {
//             return false;
//         }

//         if (handId < 0)
//         {
//             m_leftWeaponSlotId = slotId;
//             return true;
//         }

//         if (handId > 0)
//         {
//             m_rightWeaponSlotId = slotId;
//             return true;
//         }

//         return false;
//     }

//     protected virtual bool PickWeapon(Weapon weapon)
//     {
//         return true;
//     }

//     protected virtual bool DropWeapon(Weapon weapon)
//     {
//         return true;
//     }

//     protected virtual bool EquipWeapon(int handId, Weapon weapon)
//     {
//         if (weapon == null)
//         {
//             return false;
//         }

//         var charHand = charEquip.GetHand(handId);
//         if (charHand.id == 0)
//         {
//             return false;
//         }

//         EquipData equipData = new EquipData();
//         equipData.equipable = weapon;
//         equipData.equipableObject = weapon.gameObject;
//         equipData.equipSpeed = .2f;
//         equipData.unequipSpeed = .2f;
//         equipData.parentOnUnequip = null;
//         // equipData.OnUpdate = OnUpdate;
//         if (charHand.Equip(equipData) == false)
//         {
//             return false;
//         }

//         if (handId < 0)
//         {
//             m_leftWeaponSlotId = -1;
//             return true;
//         }

//         if (handId > 0)
//         {
//             m_rightWeaponSlotId = -1;
//             return true;
//         }

//         return true;
//     }

//     protected virtual bool UnequipWeapon(Weapon weapon)
//     {
//         return true;
//     }

//     //////////////////////////////////////////////////////////////////
//     /// Weapon Events
//     //////////////////////////////////////////////////////////////////

//     public virtual void OnWeaponFound(Weapon weapon)
//     {
//     }

//     protected virtual void OnWeaponPicked(Weapon weapon)
//     {
//     }

//     protected virtual void OnWeaponDropped(Weapon weapon)
//     {
//     }

//     protected virtual void OnWeaponEquipped(Weapon weapon)
//     {
//     }

//     protected virtual void OnWeaponUnequipped(Weapon weapon)
//     {
//     }
// }