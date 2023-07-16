using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Projectile))]
public abstract class Grenade : Equipable
{
    protected Projectile _projectile;

    protected bool _isTriggered;

    [Header("GRENADE"), Space]
    [SerializeField, Min(0)]
    protected float _triggerTime;

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

    //// -------------------------------------------------------------------------------------------
    //// Trigger Begin
    //// -------------------------------------------------------------------------------------------

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
            _OnTriggerFinish();
            return true;
        }

        StartCoroutine(_TriggerCoroutine());
        return true;
    }

    protected IEnumerator _TriggerCoroutine()
    {
        yield return new WaitForSeconds(_triggerTime);

        if (_isTriggered)
        {
            _OnTriggerFinish();
        }
    }

    protected virtual void _OnTriggerFinish()
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

    protected virtual void _OnTriggerStop()
    {
    }

    //// -------------------------------------------------------------------------------------------
    //// Trigger End
    //// -------------------------------------------------------------------------------------------

    //// -------------------------------------------------------------------------------------------
    //// Physics and Geometry Begin
    //// -------------------------------------------------------------------------------------------

    protected virtual void _DisableGeometry()
    {
    }

    protected virtual void _EnableGeometry()
    {
    }

    //// -------------------------------------------------------------------------------------------
    //// Physics and Geometry End
    //// -------------------------------------------------------------------------------------------

    //// -------------------------------------------------------------------------------------------
    //// Properties and Fields
    //// -------------------------------------------------------------------------------------------

    public bool isTriggered => _isTriggered;
    public float triggerTime => _triggerTime;
}