using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyProjectile : Projectile
{
    private Rigidbody m_rigidBody = null;
    new protected Rigidbody rigidbody
    {
        get
        {
            if (m_rigidBody == null)
            {
                m_rigidBody = GetComponent<Rigidbody>();
            }

            return m_rigidBody;
        }

        set
        {
            m_rigidBody = value;
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