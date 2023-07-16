using UnityEngine;

public class FragGrenade : Grenade
{
    protected sealed override void _OnTriggerFinish()
    {
        // _DisablePhysics();
        _DisableColliders();
        _DisableGeometry();

        // Process colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius);
        foreach (Collider collider in colliders)
        {
            // Cast a ray to check if there is no obstacle between collider and explosion center.
            Vector3 thisToCollider = collider.transform.position - transform.position;
            bool hit = Physics.Raycast(transform.position, thisToCollider.normalized,
                out RaycastHit hitInfo, thisToCollider.magnitude, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            // Check if there is no collider other than the collider itself.
            if (hit && hitInfo.collider != collider)
            {
                continue;
            }

            // Add force to rigidbody.
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.AddExplosionForce(_force, transform.position, _radius, 0.1f, ForceMode.Impulse);
            }

            // Add damage to health component.
            Health health = collider.GetComponent<Health>();
            if (health)
            {
                float damage = _damage;
                health.health -= damage;
            }
        }

        if (_effect is not null)
        {
            // Run visual effects
            _effect.Play();
            Destroy(gameObject, _effect.main.duration);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected ParticleSystem _effect;
    protected float _force;
    protected float _damage;
    protected float _radius;
}