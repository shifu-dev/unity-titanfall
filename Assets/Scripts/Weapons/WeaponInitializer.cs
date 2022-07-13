using UnityEngine;

[DisallowMultipleComponent]
public class WeaponInitializer : MonoBehaviour
{
    public bool destroyAfterUse;
    public WeaponDataSource source;

    public WeaponInitializer()
    {
        destroyAfterUse = false;
        source = null;
    }
}