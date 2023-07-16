using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyProjectile : Projectile
{
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public sealed override void Launch(Vector3 launchVector)
    {
        _rigidbody.AddForce(launchVector);
    }

    public sealed override void Stop()
    {
        _rigidbody.isKinematic = true;
    }

    protected Rigidbody _rigidbody;
}