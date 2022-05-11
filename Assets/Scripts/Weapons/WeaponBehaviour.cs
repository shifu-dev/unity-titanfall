using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Weapon))]
public abstract class WeaponBehaviour : MonoBehaviour
{
    public Weapon weapon { get; protected set; }

    public virtual void OnInitWeapon(Weapon weapon, WeaponInitializer initializer = null)
    {
        this.weapon = weapon;
    }

    public virtual void OnUpdateWeapon()
    {
    }

    public virtual void OnFixedUpdateWeapon()
    {
    }

    public virtual void OnDestroyWeapon()
    {
    }
}