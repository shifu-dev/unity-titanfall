using UnityEngine;
using System.Collections;

public enum GrenadeCategory
{
    Unknown,
    FragGrenade,
    SmokeGrenade
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Interactable))]
public abstract class Grenade : MonoBehaviour
{
    public abstract GrenadeAsset grenadeAsset { get; }
    new public Rigidbody rigidbody { get; protected set; }
    public Interactable interactable { get; protected set; }

    [Header("GRENADE"), Space]

    [SerializeField] protected GrenadeCategory m_Category;
    public GrenadeCategory category => m_Category;

    [SerializeField, Min(0)] protected float m_TriggerTime;
    public float triggerTime => m_TriggerTime;

    [SerializeField] protected bool m_CanStopTrigger;
    [SerializeField] protected bool m_disablePhysicsOnEquip;

    [field: SerializeField, ReadOnly]
    public bool isTriggered { get; protected set; }

    [SerializeField, Space] protected Collider[] m_Colliders;

    //////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////

    public Grenade()
    {
        m_Category = GrenadeCategory.Unknown;
        m_TriggerTime = 0;
        m_CanStopTrigger = false;
        m_disablePhysicsOnEquip = true;
        isTriggered = false;
        m_Colliders = new Collider[0];
    }

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        interactable = GetComponent<Interactable>();
    }

    public virtual bool CanEquip()
    {
        if (isTriggered)
            return false;

        if (category == GrenadeCategory.Unknown)
            return false;

        return true;
    }

    public virtual void OnEquip()
    {
        Debug.Log("OnEquip");
        if (interactable)
        {
            interactable.canInteract = false;
        }

        if (m_disablePhysicsOnEquip)
        {
            DisablePhysics();
        }
    }

    public virtual void OnUnequip()
    {
        if (interactable)
        {
            interactable.canInteract = isTriggered ? false : true;
        }

        if (m_disablePhysicsOnEquip)
        {
            EnablePhysics();
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
        if (interactable)
        {
            interactable.canInteract = false;
        }

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

    //////////////////////////////////////////////////////////////////
    /// Physics & Geometry
    //////////////////////////////////////////////////////////////////

    protected virtual void DisablePhysics()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        foreach (var collider in m_Colliders)
        {
            collider.enabled = false;
        }
    }

    protected virtual void EnablePhysics()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = false;
        }

        foreach (var collider in m_Colliders)
        {
            collider.enabled = true;
        }
    }

    protected virtual void DisableGeometry()
    {
    }

    protected virtual void EnableGeometry()
    {
    }
}