using UnityEngine;

public class WeaponAutoInitializer : MonoBehaviour
{
    private void Awake()
    {
        Weapon weapon = GetComponent<Weapon>();
        weapon.Init();
    }
}