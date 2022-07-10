using UnityEngine;
using System.Collections;

public enum GrenadeCategory
{
    Unknown,
    FragGrenade,
    SmokeGrenade
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Projectile))]
public abstract class Grenade : Equipable
{
    public override EquipableType type => EquipableType.Grenade;
    public override Grenade grenade => this;

    public abstract GrenadeAsset grenadeAsset { get; }
    public Projectile projectile { get; protected set; }

    public abstract GrenadeCategory category { get; }

    public bool isTriggered { get; protected set; }

    [field: Header("GRENADE"), Space, SerializeField, Min(0)]
    public float triggerTime { get; protected set; }

    [SerializeField] protected bool canStopTrigger;

    //////////////////////////////////////////////////////////////////
    /// Events
    //////////////////////////////////////////////////////////////////

    public Grenade()
    {
        triggerTime = 0;
        canStopTrigger = false;
        isTriggered = false;
    }

    void Awake()
    {
        projectile = GetComponent<Projectile>();
    }

    public virtual bool CanEquip()
    {
        if (isTriggered)
            return false;

        if (category == GrenadeCategory.Unknown)
            return false;

        return true;
    }

    public override void OnUnequipStart()
    {
        base.OnUnequipStart();

        canInteract = isTriggered ? false : true;
    }

    //////////////////////////////////////////////////////////////////
    /// StartTrigger
    //////////////////////////////////////////////////////////////////

    public virtual bool CanStartTrigger()
    {
        return canStopTrigger;
    }

    public virtual bool StartTrigger()
    {
        canInteract = false;

        if (CanStartTrigger() == false)
            return false;

        if (triggerTime == 0)
        {
            OnTriggerFinish();
            return true;
        }

        StartCoroutine(StartTriggerCoroutine());
        return true;
    }

    protected IEnumerator StartTriggerCoroutine()
    {
        yield return new WaitForSeconds(triggerTime);

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
        return canStopTrigger && isTriggered;
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

    protected virtual void DisableGeometry()
    {
    }

    protected virtual void EnableGeometry()
    {
    }
}