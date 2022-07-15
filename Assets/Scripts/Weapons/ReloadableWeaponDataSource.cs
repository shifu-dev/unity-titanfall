using System;
using UnityEngine;

public abstract class ReloadableWeaponDataSource : FireableWeaponDataSource
{
    [Header("RELOADABLE WEAPON"), Space]

    [Label("Capacity"), SerializeField]    
    protected uint _capacity;
    public uint capacity => _capacity;

    [Label("Initial"), SerializeField]
    protected uint _initial;
    public uint initial => _initial;
}