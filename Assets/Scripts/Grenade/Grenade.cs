using UnityEngine;
using System.Collections;

public enum GrenadeCategory
{
    FragGrenade,
    SmokeGrenade
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public abstract class Grenade : MonoBehaviour
{
    public abstract GrenadeAsset GrenadeAsset { get; }
    public Rigidbody Rigidbody { get; protected set; }

    [Header("GRENADE"), Space]

    [SerializeField] protected GrenadeCategory m_Category;
    public GrenadeCategory Category => m_Category;

    [SerializeField, Min(0)] protected float m_TriggerTime;
    public float TriggerTime => m_TriggerTime;

    [SerializeField] protected bool m_CanStopTrigger;

    [field: SerializeField, ReadOnly]
    public bool isTriggered { get; protected set; }

    [SerializeField, Space] protected Collider[] m_Colliders;

    //////////////////////////////////////////////////////////////////
    /// CharacterEvents
    //////////////////////////////////////////////////////////////////

    public virtual void OnEquip()
    {
        Rigidbody = GetComponent<Rigidbody>();

        foreach (var collider in m_Colliders)
        {
            collider.enabled = false;
        }
    }

    public virtual void OnUnequip()
    {
        foreach (var collider in m_Colliders)
        {
            collider.enabled = true;
        }
    }

    //////////////////////////////////////////////////////////////////
    /// StartTrigger
    //////////////////////////////////////////////////////////////////

    public virtual bool CanStartTrigger()
    {
        return m_CanStopTrigger;
    }

    public virtual bool StartTrigger()
    {
        if (CanStartTrigger() == false)
            return false;

        if (m_TriggerTime == 0)
        {
            OnTriggerFinish();
            return true;
        }

        StartCoroutine(StartTriggerCoroutine());
        return true;
    }

    protected IEnumerator StartTriggerCoroutine()
    {
        yield return new WaitForSeconds(m_TriggerTime);

        if (isTriggered)
        {
            OnTriggerFinish();
        }
    }

    protected virtual void OnTriggerFinish()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// StopTrigger
    //////////////////////////////////////////////////////////////////

    public virtual bool CanStopTrigger()
    {
        return m_CanStopTrigger && isTriggered;
    }

    public virtual bool StopTrigger()
    {
        if (CanStopTrigger() == false)
            return false;

        isTriggered = false;
        return true;
    }

    protected virtual void OnTriggerStop()
    {
    }
}