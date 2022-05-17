// using System;
// using UnityEngine;

// public class CharacterGrenade : CharacterBehaviour
// {
//     protected CharacterInputs m_characterInputs;
//     protected CharacterEquip m_characterEquip;

//     [SerializeField] protected GrenadeSlots m_grenadeSlots1;
//     [SerializeField] protected GrenadeSlots m_grenadeSlots2;
//     [SerializeField, ReadOnly] protected Grenade m_grenadeBeingEquipped;
//     [SerializeField, ReadOnly] protected Grenade m_grenadeEquipped;

//     public override void OnInitCharacter(Character character, CharacterInitializer initializer)
//     {
//         m_characterInputs = character.characterInputs;
//         m_characterEquip = character.characterEquip;

//         m_grenadeSlots1.Init();
//         m_grenadeSlots2.Init();
//     }

//     public override void OnUpdateCharacter()
//     {
//         // process the inputs
//         Grenade grenadeToEquip = null;
//         if (m_characterInputs.grenade1)
//         {
//             grenadeToEquip = m_grenadeSlots1.Get();
//         }
//         else if (m_characterInputs.grenade2)
//         {
//             grenadeToEquip = m_grenadeSlots2.Get();
//         }

//         // check if we can equip the grenade
//         bool canEquip(Grenade grenade)
//         {
//             if (grenade == null)
//                 return false;

//             // check if a grenade with same category is already being equipped
//             if (m_grenadeBeingEquipped && grenadeToEquip.category == m_grenadeBeingEquipped.category)
//                 return false;

//             // check if a grenade with same category is already equipped
//             if (m_grenadeEquipped && grenadeToEquip.category == m_grenadeEquipped.category)
//                 return false;

//             return true;
//         };

//         // equip the grenade
//         if (canEquip(grenadeToEquip))
//         {
//             EquipData equipData = new EquipData();
//             equipData.equipable = grenadeToEquip;
//             equipData.equipableObject = grenadeToEquip.gameObject;
//             equipData.OnUpdate = this.OnUpdateEquipStatus;
//             equipData.equipSpeed = .5f;
//             equipData.unequipSpeed = .5f;
//             equipData.parentOnUnequip = null;

//             if (m_characterEquip.leftHand.Equip(equipData))
//             {
//                 m_grenadeBeingEquipped = grenadeToEquip;
//             }
//         }

//         // fire the grenade
//         if (m_characterInputs.fire1)
//         {
//             ThrowGrenade(m_grenadeEquipped);
//             return;
//         }
//     }

//     protected bool AddGrenade(Grenade grenade)
//     {
//         if (grenade == null)
//             return false;

//         if (m_grenadeSlots1.Add(grenade))
//         {
//             grenade.OnEquip();
//             return true;
//         }

//         if (m_grenadeSlots2.Add(grenade))
//         {
//             grenade.OnEquip();
//             return true;
//         }

//         return false;
//     }

//     protected Grenade GetGrenade(GrenadeCategory category)
//     {
//         if (category == GrenadeCategory.Unknown)
//             return null;

//         Grenade grenade = null;

//         if (m_grenadeSlots1.category == category)
//         {
//             grenade = m_grenadeSlots1.Get();
//             if (grenade != null)
//             {
//                 return grenade;
//             }
//         }

//         if (m_grenadeSlots2.category == category)
//         {
//             grenade = m_grenadeSlots2.Get();
//             if (grenade != null)
//             {
//                 return grenade;
//             }
//         }

//         return grenade;
//     }

//     protected Grenade PopGrenade(GrenadeCategory category)
//     {
//         if (category == GrenadeCategory.Unknown)
//             return null;

//         Grenade grenade = null;

//         if (m_grenadeSlots1.category == category)
//         {
//             grenade = m_grenadeSlots1.Pop();
//             if (grenade != null)
//             {
//                 return grenade;
//             }
//         }

//         if (m_grenadeSlots2.category == category)
//         {
//             grenade = m_grenadeSlots2.Pop();
//             if (grenade != null)
//             {
//                 return grenade;
//             }
//         }

//         return grenade;
//     }

//     protected void EquipGrenade(GrenadeCategory category)
//     {

//     }

//     protected void UnequipGrenade()
//     {
//     }

//     protected void ThrowGrenade(Grenade grenade)
//     {
//     }

//     public void OnGrenadeFound(Grenade grenade)
//     {
//         AddGrenade(grenade);
//     }

//     public void HandleGrenade(Grenade grenade, EquipStatus status, float weight)
//     {
//     }

//     //////////////////////////////////////////////////////////////////
//     /// CharacterEquip update evenets
//     //////////////////////////////////////////////////////////////////

//     protected void OnUpdateEquipStatus(IEquipable equipable, EquipStatus equipStatus, float meter)
//     {
//         if (equipStatus == EquipStatus.Unequip || equipStatus == EquipStatus.Unequipped)
//         {
//             m_grenadeBeingEquipped = null;
//             return;
//         }

//         if (equipStatus == EquipStatus.Equipped)
//         {
//             m_grenadeEquipped = m_grenadeBeingEquipped;
//             m_grenadeBeingEquipped = null;
//         }
//     }
// }