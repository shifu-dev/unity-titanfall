using UnityEngine;

[DisallowMultipleComponent]
public abstract class Projectile : MonoBehaviour
{
    public abstract void Launch(Vector3 launchVector);

    public abstract void Stop();
}