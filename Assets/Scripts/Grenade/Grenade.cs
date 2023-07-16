using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Projectile))]
public abstract class Grenade : Equipable
{
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
    //// Methods
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

    public virtual bool CanStopTrigger()
    {
        return _canStopTrigger && _isTriggered;
    }

    public virtual bool StopTrigger()
    {
        if (!CanStopTrigger())
            return false;

        _isTriggered = false;
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

    protected virtual void _OnTriggerFinish() { }

    protected virtual void _OnTriggerStop() { }

    protected virtual void _DisableGeometry() { }

    protected virtual void _EnableGeometry() { }

    //// -------------------------------------------------------------------------------------------
    //// Properties and Fields
    //// -------------------------------------------------------------------------------------------

    public bool isTriggered => _isTriggered;
    public float triggerTime => _triggerTime;

    protected Projectile _projectile;
    protected bool _isTriggered;
    protected float _triggerTime;
    protected bool _canStopTrigger;
}