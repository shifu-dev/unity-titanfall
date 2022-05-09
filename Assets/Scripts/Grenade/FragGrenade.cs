using UnityEngine;

public class FragGrenade : Grenade
{
    public override GrenadeAsset GrenadeAsset => m_FragGrenadeAsset;
    public FragGrenadeAsset FragGrenadeAsset => m_FragGrenadeAsset;

    [Header("FRAG GRENADE"), Space]

    [SerializeField] protected FragGrenadeAsset m_FragGrenadeAsset;
    [SerializeField] protected GameObject m_Effect;
    [SerializeField, Min(0)] protected float m_Force;
    [SerializeField, Min(0)] protected float m_Radius;

    protected override void OnTriggerFinish()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_Radius);
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(m_Force, transform.position, m_Radius, 0.1f, ForceMode.Impulse);
            }
        }

        Instantiate(m_Effect, transform.position, transform.rotation);
        Destroy(gameObject, 0.1f);
    }
}