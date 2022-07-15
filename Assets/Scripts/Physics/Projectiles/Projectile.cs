using UnityEngine;

[DisallowMultipleComponent]
public abstract class Projectile : MonoBehaviour
{
    public void Launch(float force)
    {
        Launch(transform.forward * force);
    }

    public abstract void Launch(Vector3 launchVector);

    public abstract void Stop();
}