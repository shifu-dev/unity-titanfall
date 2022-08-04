using UnityEngine;

[DisallowMultipleComponent]
public class WeaponInitializer : MonoBehaviour
{
    public bool destroyAfterUse;
    public WeaponSource source;
    public WeaponBulletSource bulletDataSource;

    public WeaponInitializer()
    {
        destroyAfterUse = false;
        source = null;
    }
}