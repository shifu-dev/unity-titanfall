using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Projectile))]
public abstract class Grenade : Equipable
{
    protected Projectile _projectile;

    protected bool _isTriggered;
    public bool isTriggered => _isTriggered;

    [Header("GRENADE"), Space]
    [SerializeField, Min(0)]
    protected float _triggerTime;
    public float triggerTime => _triggerTime;

    [SerializeField]
    protected bool _canStopTrigger;

    public Grenade()
    {
        _triggerTime = 0;
        _canStopTrigger = false;
        _isTriggered = false;
    }

    protected virtual void Awake()
    {
        _projectile = GetComponent<Projectile>();
    }

    //////////////////////////////////////////////////////////////////
    /// Trigger | START

    public virtual bool CanStartTrigger()
    {
        return _canStopTrigger;
    }

    public virtual bool StartTrigger()
    {
        canInteract = false;

        if (CanStartTrigger() == false)
            return false;

        if (_triggerTime == 0)
        {
            OnTriggerFinish();
            return true;
        }

        StartCoroutine(StartTriggerCoroutine());
        return true;
    }

    protected IEnumerator StartTriggerCoroutine()
    {
        yield return new WaitForSeconds(_triggerTime);

        if (_isTriggered)
        {
            OnTriggerFinish();
        }
    }

    protected virtual void OnTriggerFinish()
    {
    }

    public virtual bool CanStopTrigger()
    {
        return _canStopTrigger && _isTriggered;
    }

    public virtual bool StopTrigger()
    {
        if (CanStopTrigger() == false)
            return false;

        _isTriggered = false;
        return true;
    }

    protected virtual void OnTriggerStop()
    {
    }

    /// Trigger | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Physics & Geometry | BEGIN

    protected virtual void DisableGeometry()
    {
    }

    protected virtual void EnableGeometry()
    {
    }

    /// Physics & Geometry | END
    //////////////////////////////////////////////////////////////////
}