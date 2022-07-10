using UnityEngine;

public class FragGrenade : Grenade
{
    public override GrenadeCategory category => GrenadeCategory.FragGrenade;

    public override GrenadeAsset grenadeAsset => m_fragGrenadeAsset;
    public FragGrenadeAsset fragGrenadeAsset => m_fragGrenadeAsset;

    [Header("FRAG GRENADE"), Space]

    [SerializeField] protected FragGrenadeAsset m_fragGrenadeAsset;
    [SerializeField] protected ParticleSystem m_effect;
    [SerializeField, Min(0)] protected float m_force;
    [SerializeField, Min(0)] protected float m_damage;
    [SerializeField, Min(0)] protected float m_radius;

    protected override void OnTriggerFinish()
    {
        // DisablePhysics();
        DisableColliders();
        DisableGeometry();

        // process colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_radius);
        foreach (Collider collider in colliders)
        {
            // cast a ray to check if there is no obstacle between collider adn explosion center
            Vector3 thisToCollider = collider.transform.position - transform.position;
            bool hit = Physics.Raycast(transform.position, thisToCollider.normalized, out RaycastHit hitInfo,
                thisToCollider.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            // check if there is no collider other than the collider itself
            if (hit && hitInfo.collider != collider)
            {
                continue;
            }

            // add force to rigidbody
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.AddExplosionForce(m_force, transform.position, m_radius, 0.1f, ForceMode.Impulse);
            }

            // add damage to health component
            Health health = collider.GetComponent<Health>();
            if (health)
            {
                float damage = m_damage;
                health.health -= damage;
            }
        }

        if (m_effect == null)
        {
            Destroy(gameObject);
            return;
        }

        // run visual effects
        m_effect.Play();
        Destroy(gameObject, m_effect.main.duration);
        return;
    }
}