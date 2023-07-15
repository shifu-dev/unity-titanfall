using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyProjectile : Projectile
{
    private Rigidbody _rigidBody = null;
    new protected Rigidbody rigidbody
    {
        get
        {
            if (_rigidBody is null)
            {
                _rigidBody = GetComponent<Rigidbody>();
            }

            return _rigidBody;
        }

        set
        {
            _rigidBody = value;
        }
    }

    public override void Launch(Vector3 launchVector)
    {
        if (rigidbody)
        {
            rigidbody.AddForce(launchVector);
        }
    }

    public override void Stop()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = true;
        }
    }
}