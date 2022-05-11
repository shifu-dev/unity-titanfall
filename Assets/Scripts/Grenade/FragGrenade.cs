using UnityEngine;

public class FragGrenade : Grenade
{
    public override GrenadeAsset GrenadeAsset => m_FragGrenadeAsset;
    public FragGrenadeAsset FragGrenadeAsset => m_FragGrenadeAsset;

    [Header("FRAG GRENADE"), Space]

    [SerializeField] protected FragGrenadeAsset m_FragGrenadeAsset;
    [SerializeField] protected ParticleSystem m_Effect;
    [SerializeField, Min(0)] protected float m_Force;
    [SerializeField, Min(0)] protected float m_Damage;
    [SerializeField, Min(0)] protected float m_Radius;

    protected override void OnTriggerFinish()
    {
        // disable physics
        Rigidbody.isKinematic = true;
        foreach (var collider in m_Colliders)
        {
            collider.enabled = false;
        }

        // process colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_Radius);
        foreach (Collider collider in colliders)
        {
            Vector3 thisToCollider = collider.transform.position - transform.position;
            bool hit = Physics.Raycast(transform.position, thisToCollider.normalized, out RaycastHit hitInfo,
                thisToCollider.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            if (hit && hitInfo.collider != collider)
            {
                continue;
            }

            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.AddExplosionForce(m_Force, transform.position, m_Radius, 0.1f, ForceMode.Impulse);
            }

            Health health = collider.GetComponent<Health>();
            if (health)
            {
                float damage = m_Damage;
                health.health -= damage;
            }
        }

        // run visual effects
        m_Effect.Play();
        Destroy(gameObject, m_Effect.main.duration);
    }
}