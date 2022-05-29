using System;
using UnityEngine;

[Serializable]
public struct InteractableScanResult
{
    public static readonly InteractableScanResult invalid;

    public Interactable interactable;

    public int manualFrame;
    public bool manual
    {
        get => manualFrame > 0;
        set
        {
            manualFrame = value ? Time.frameCount : 0;
        }
    }

    public int raycastFrame;
    public bool raycasted
    {
        get => raycastFrame > 0;
        set
        {
            raycastFrame = value ? Time.frameCount : 0;
        }
    }

    public int overlapFrame;
    public bool overlapped
    {
        get => overlapFrame > 0;
        set
        {
            overlapFrame = value ? Time.frameCount : 0;
        }
    }

    public bool isValid => interactable != null;

    public bool HasThisFrame(int frame)
    {
        if (manualFrame == frame)
            return true;

        if (raycastFrame == frame)
            return true;

        if (overlapFrame == frame)
            return true;

        return false;
    }
}

public class CharacterInteraction : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    public CharacterInputs charInputs { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterEquip charEquip { get; protected set; }

    [SerializeField] protected InteractableScanResult[] m_interactables;
    [SerializeField, ReadOnly] protected int m_interactablesCount;

    public int interactablesCapacity
    {
        get => m_interactables.Length;
        set
        {
            if (value >= 0 && value != interactablesCapacity)
            {
                Array.Resize(ref m_interactables, value);
                interactablesCount = Math.Min(value, interactablesCount);
            }
        }
    }
    public int interactablesCount
    {
        get => m_interactablesCount;
        protected set
        {
            m_interactablesCount = Math.Max(0, value);
        }
    }

    [Space]
    [SerializeField, Min(0)] protected float m_updateInterval;
    [SerializeField] protected LayerMask m_layerMask;
    [SerializeField] protected QueryTriggerInteraction m_triggerQuery;
    [SerializeField, Label("On Added Event")] protected bool m_generateOnAddedEvent;
    [SerializeField, Label("On Updated Event")] protected bool m_generateOnUpdatedEvent;
    [SerializeField, Label("On Removed Event")] protected bool m_generateOnRemovedEvent;
    protected float m_lastUpdateTime;

    [Header("Overlaps"), Space]
    [SerializeField] protected bool m_performOverlaps;
    [SerializeField] protected Vector3 m_overlapSize;
    [SerializeField] protected Vector3 m_overlapCenterOffset;

    [Header("Raycasts"), Space]
    [SerializeField] protected bool m_performRaycasts;
    [SerializeField] protected Transform m_raycastSource;
    [SerializeField, Min(0)] protected float m_raycastLength;

    public CharacterInteraction()
    {
        m_interactables = Array.Empty<InteractableScanResult>();
        interactablesCapacity = 0;

        m_layerMask = default;
        m_triggerQuery = QueryTriggerInteraction.Collide;
        m_generateOnAddedEvent = true;
        m_generateOnRemovedEvent = false;
        m_updateInterval = .5f;
        m_lastUpdateTime = 0;
        m_performOverlaps = false;
        m_overlapSize = new Vector3(1.8f, 1.5f, 2f);
        m_overlapCenterOffset = new Vector3(0, 0, .2f);
        m_performRaycasts = false;
        m_raycastSource = null;
        m_raycastLength = 3;
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.charInputs;
        charCapsule = character.charCapsule;
        charEquip = character.charEquip;
    }

    public override void OnUpdateCharacter()
    {
        CollectInteractables();
    }

    public bool CollectInteractables(bool force = false)
    {
        if (force == false)
        {
            if (Time.time < m_lastUpdateTime + m_updateInterval)
            {
                return false;
            }
        }

        m_lastUpdateTime = Time.time;

        FindInteractables();
        RemovePreviousScanResults();

        return true;
    }

    protected void FindInteractables()
    {
        if (interactablesCapacity == 0)
            return;

        InteractableScanResult raycastScanResult = InteractableScanResult.invalid;
        if (m_performRaycasts)
        {
            if (m_raycastSource != null)
            {
                bool hit = Physics.Raycast(m_raycastSource.position, m_raycastSource.forward, out RaycastHit hitInfo,
                    m_raycastLength, m_layerMask, m_triggerQuery);

                if (hit)
                {
                    Interactable interactable = Interactable.GetInteractable(hitInfo.collider);
                    if (interactable)
                    {
                        raycastScanResult = GenerateRaycastScanResult(interactable, hitInfo);
                    }
                }
            }
        }

        if (m_performOverlaps)
        {
            /// @todo calculate halfExtents with respect to character size
            Vector3 halfExtents = m_overlapSize * .5f;
            Vector3 center = charCapsule.basePosition + (charCapsule.up * halfExtents.y) + m_overlapCenterOffset;
            Collider[] overlapResults = overlapResults = Physics.OverlapBox(center, halfExtents, Quaternion.identity,
                    m_layerMask, m_triggerQuery);

            foreach (var collider in overlapResults)
            {
                Interactable interactable = Interactable.GetInteractable(collider);
                if (interactable != null)
                {
                    var scanResult = GenerateOverlapScanResult(interactable);

                    if (raycastScanResult.isValid && raycastScanResult.interactable == interactable)
                    {
                        // @todo merge scan results

                        // invalidate raycastResult to notify that it has been used
                        raycastScanResult = InteractableScanResult.invalid;
                    }

                    AddScanResult(scanResult);
                }
            }
        }

        if (raycastScanResult.isValid)
        {
            AddScanResult(raycastScanResult);
        }
    }

    protected void RemovePreviousScanResults()
    {
        var thisFrame = Time.frameCount;
        for (int i = 0; i < interactablesCapacity; i++)
        {
            if (m_interactables[i].interactable == null)
                continue;

            if (m_interactables[i].HasThisFrame(thisFrame) == false)
            {
                RemoveScanResult(i);
                i--;
            }
        }
    }

    protected InteractableScanResult GenerateRaycastScanResult(Interactable interactable, RaycastHit hit)
    {
        if (interactable == null)
            return InteractableScanResult.invalid;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.raycasted = true;

        return scanResult;
    }

    protected InteractableScanResult GenerateOverlapScanResult(Interactable interactable)
    {
        if (interactable == null)
            return InteractableScanResult.invalid;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.overlapped = true;

        return scanResult;
    }

    protected InteractableScanResult GenerateManualScanResult(Interactable interactable)
    {
        if (interactable == null)
            return InteractableScanResult.invalid;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.manual = true;

        return scanResult;
    }

    protected bool AddScanResult(InteractableScanResult scanResult)
    {
        if (scanResult.isValid == false)
            return false;

        // find if scanResult with this interactable already exists
        bool wasPresent = false;
        for (int i = 0; i < m_interactablesCount; i++)
        {
            if (m_interactables[i].interactable == scanResult.interactable)
            {
                wasPresent = true;
                scanResult = UpdateScanResult(m_interactables[i], scanResult);

                // if previous scanResult exists, remove it
                m_interactables.Remove(i, InteractableScanResult.invalid);
                m_interactablesCount--;

                break;
            }
        }

        // filter scanResult after updating it
        if (FilterScanResult(scanResult) == false)
        {
            return false;
        }

        // find sorted position position for the scanResult (based on priority)
        for (int i = 0; i < interactablesCapacity; i++)
        {
            if (i < interactablesCount && CompareScanResult(m_interactables[i], scanResult) <= 0)
            {
                continue;
            }

            if (i < interactablesCount)
            {
            }
            else
            {
            }

            // add the scanResult
            InsertScanResult(scanResult, i);

            if (wasPresent)
            {
                if (m_generateOnUpdatedEvent)
                {
                    OnInteractableUpdated(scanResult);
                }
            }
            else if (m_generateOnAddedEvent)
            {
                OnInteractableAdded(scanResult);
            }

            return true;
        }

        // no appropriate position found for the scanResult
        return false;
    }

    protected void InsertScanResult(InteractableScanResult scanResult, int index)
    {
        // if full remove the last scanResult
        if (interactablesCount == interactablesCapacity)
        {
            RemoveScanResult(interactablesCount - 1);
        }

        m_interactables.Insert(scanResult, index);
        m_interactablesCount++;
    }

    protected void RemoveScanResult(int index)
    {
        var scanResult = m_interactables[index];
        m_interactables.Remove(index);
        m_interactablesCount--;

        if (m_generateOnRemovedEvent)
        {
            OnInteractableRemoved(scanResult.interactable);
        }
    }

    protected bool FilterScanResult(InteractableScanResult scanResult)
    {
        var interactable = scanResult.interactable;
        if (interactable == null)
            return false;

        if (interactable.canInteract == false)
            return false;

        if (interactable.requireRaycast && scanResult.raycasted == false)
            return false;

        return true;
    }

    protected int CompareScanResult(InteractableScanResult lhs, InteractableScanResult rhs)
    {
        if (lhs.interactable == null && rhs.interactable == null)
            return 0;

        if (lhs.interactable && rhs.interactable)
        {
            if (lhs.manual == rhs.manual)
            {
                if (lhs.raycasted == rhs.raycasted)
                {
                    if (lhs.overlapped == rhs.overlapped)
                    {
                        return 0;
                    }

                    return rhs.overlapped ? 1 : -1;
                }

                return rhs.raycasted ? 1 : -1;
            }

            return rhs.manual ? 1 : -1;
        }

        return rhs.interactable ? 1 : -1;
    }

    protected InteractableScanResult UpdateScanResult(InteractableScanResult previous, InteractableScanResult present)
    {
        var result = present;
        result.manual = present.manual || previous.manual;
        result.raycasted = present.raycasted || previous.raycasted;
        result.overlapped = present.overlapped || previous.overlapped;

        return result;
    }

    protected virtual void OnInteractableAdded(InteractableScanResult scanResult)
    {
        var interactable = scanResult.interactable;
        if (interactable == null)
            return;

        Grenade grenade = interactable.GetComponent<Grenade>();
        if (grenade != null)
        {
            charEquip.OnGrenadeFound(grenade);
            return;
        }

        Weapon weapon = interactable.GetComponent<Weapon>();
        if (weapon != null)
        {
            charEquip.OnWeaponFound(scanResult, weapon);
            return;
        }
    }

    protected virtual void OnInteractableUpdated(InteractableScanResult scanResult)
    {
    }

    protected virtual void OnInteractableRemoved(Interactable interactable)
    {
    }

    //////////////////////////////////////////////////////////////////
    /// Public API
    //////////////////////////////////////////////////////////////////

    public void ForEachScanResult(Action<InteractableScanResult> action)
    {
        if (action != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                action(m_interactables[i]);
            }
        }
    }

    public void ForEachInteractable(Action<Interactable> action)
    {
        if (action != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                action(m_interactables[i].interactable);
            }
        }
    }

    public InteractableScanResult FindScanResult(Predicate<InteractableScanResult> pred)
    {
        if (pred != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(m_interactables[i]))
                {
                    return m_interactables[i];
                }
            }
        }

        return InteractableScanResult.invalid;
    }

    public Interactable FindInteractable(Predicate<InteractableScanResult> pred)
    {
        if (pred != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(m_interactables[i]))
                {
                    return m_interactables[i].interactable;
                }
            }
        }

        return null;
    }

    public Interactable FindInteractable(Predicate<Interactable> pred)
    {
        if (pred != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(m_interactables[i].interactable))
                {
                    return m_interactables[i].interactable;
                }
            }
        }

        return null;
    }

    public InteractableScanResult GetScanResult(Interactable interactable)
    {
        if (interactable != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (m_interactables[i].interactable == interactable)
                    return m_interactables[i];
            }
        }

        return InteractableScanResult.invalid;
    }

    public bool HasInteractable(Interactable interactable)
    {
        if (interactable != null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (m_interactables[i].interactable == interactable)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Interactable GetInteractableWith<T>(out T comp)
    {
        for (int i = 0; i < interactablesCount; i++)
        {
            if (m_interactables[i].interactable &&
                m_interactables[i].interactable.TryGetComponent<T>(out T foundComp))
            {
                comp = foundComp;
                return m_interactables[i].interactable;
            }
        }

        comp = default;
        return null;
    }

    public Interactable GetInteractableWith<T>()
    {
        return GetInteractableWith<T>(out T comp);
    }
}