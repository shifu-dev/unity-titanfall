using System;
using UnityEngine;

public enum EquipStatus
{
    Unknown,
    Equip,
    Equipped,
    Unequip,
    Unequipped
}

public interface IEquipable
{
    GameObject gameObject { get; }
}