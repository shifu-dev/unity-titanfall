using System;
using UnityEngine;

[Serializable]
public struct InteractableScanResult
{
    public static readonly InteractableScanResult invalid;

    public Interactable interactable;

    public uint manualFrame;
    public bool manual
    {
        get => manualFrame > 0;
        set
        {
            manualFrame = value ? (uint)Time.frameCount : 0;
        }
    }

    public uint raycastFrame;
    public bool raycasted
    {
        get => raycastFrame > 0;
        set
        {
            raycastFrame = value ? (uint)Time.frameCount : 0;
        }
    }

    public uint overlapFrame;
    public bool overlapped
    {
        get => overlapFrame > 0;
        set
        {
            overlapFrame = value ? (uint)Time.frameCount : 0;
        }
    }

    public bool isValid => interactable != null;

    public bool HasThisFrame(uint frame)
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

    [SerializeField, ReadOnly] protected InteractableScanResult[] m_interactables;
    [SerializeField, ReadOnly] protected uint m_interactablesCount;
    protected InteractableScanResult[] m_oldInteractables;
    protected Collider[] m_overlappedColliders;

    [Header("Main"), Space]
    [SerializeField] protected LayerMask m_layerMask;
    [SerializeField] protected bool m_generateFoundEvents;
    [SerializeField] protected bool m_generateLostEvents;
    [SerializeField, Min(0)] protected float m_updateInterval;
    [SerializeField, ReadOnly] protected float m_lastUpdateTime;

    [Header("Overlaps"), Space]
    [SerializeField] protected bool m_performOverlaps;
    [SerializeField] protected Vector3 m_overlapSize;
    [SerializeField] protected Vector3 m_overlapCenterOffset;

    [Header("Raycasts"), Space]
    [SerializeField] protected bool m_performRaycasts;
    [SerializeField] protected Transform m_raycastSource;
    [SerializeField, Min(0)] protected float m_raycastLength;

    public uint interactablesScanLimit
    {
        get => (uint)m_interactables.Length;
        set
        {
            var oldInteractables = m_interactables;
            m_interactables = new InteractableScanResult[value];

            Array.Copy(oldInteractables, m_interactables, value);

            if (overlapScanLimit < interactablesScanLimit)
            {
                overlapScanLimit = interactablesScanLimit;
            }
        }
    }
    public uint interactablesCount => m_interactablesCount;

    public uint overlapScanLimit
    {
        get => (uint)m_overlappedColliders.Length;
        set
        {
            if (value < interactablesScanLimit)
            {
                return;
            }

            m_overlappedColliders = new Collider[value];
        }
    }

    public CharacterInteraction()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charInputs = character.characterInputs;
        charCapsule = character.characterCapsule;
        charEquip = character.characterEquip;
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
        if (interactablesScanLimit == 0)
        {
            return false;
        }

        m_oldInteractables = m_interactables;
        m_interactables.Clear();
        m_interactablesCount = 0;

        if (m_performRaycasts)
        {
            RaycastInteractable();
        }

        if (m_performOverlaps)
        {
            OverlapInteractables();
        }

        if (m_generateFoundEvents)
        {
            GenerateOnInteractableFoundEvents();
        }

        if (m_generateLostEvents)
        {
            GenerateOnInteractableLostEvents();
        }

        return true;
    }

    protected void RaycastInteractable()
    {
        if (m_raycastSource != null)
        {
            bool hit = Physics.Raycast(m_raycastSource.position, m_raycastSource.forward, out RaycastHit hitInfo,
                m_raycastLength, m_layerMask, QueryTriggerInteraction.Collide);

            if (hit)
            {
                Interactable interactable = Interactable.GetInteractable(hitInfo.collider);
                if (interactable)
                {
                    AddRaycastInteractable(interactable);
                }
            }
        }
    }

    protected void OverlapInteractables()
    {
        /// @todo calculate halfExtents with respect to character size
        Vector3 halfExtents = m_overlapSize * .5f;
        Vector3 center = charCapsule.GetBasePosition + (charCapsule.GetUpVector * halfExtents.y) + m_overlapCenterOffset;
        var colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity,
            m_layerMask, QueryTriggerInteraction.Collide);

        foreach (var collider in colliders)
        {
            Interactable interactable = Interactable.GetInteractable(collider);
            if (interactable != null)
            {
                AddOverlapInteractable(interactable);
            }
        }
    }

    protected void AddManualInteractable(Interactable interactable)
    {
        if (interactable == null)
            return;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.manual = true;

        AddScanResult(scanResult);
    }

    protected void AddRaycastInteractable(Interactable interactable)
    {
        if (interactable == null)
            return;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.raycasted = true;

        AddScanResult(scanResult);
    }

    protected void AddOverlapInteractable(Interactable interactable)
    {
        if (interactable == null)
            return;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.overlapped = true;

        AddScanResult(scanResult);
    }

    protected bool AddScanResult(InteractableScanResult scanResult)
    {
        // find if scanResult with this interactable already exists
        int currentIndex = -1;
        for (int i = 0; i < m_interactables.Length; i++)
        {
            if (m_interactables[i].interactable == scanResult.interactable)
            {
                currentIndex = i;
                break;
            }
        }

        // if exists compute new values based on existing scanResult
        if (currentIndex >= 0)
        {
            if (scanResult.manual == false && m_interactables[currentIndex].manual == true)
            {
                scanResult.manualFrame = m_interactables[currentIndex].manualFrame;
            }

            if (scanResult.raycasted == false && m_interactables[currentIndex].raycasted == true)
            {
                scanResult.raycastFrame = m_interactables[currentIndex].raycastFrame;
            }

            if (scanResult.overlapped == false && m_interactables[currentIndex].overlapped == true)
            {
                scanResult.overlapFrame = m_interactables[currentIndex].overlapFrame;
            }
        }

        // filter result after computing new values
        if (FilterScanResult(scanResult) == false)
        {
            // remove scanResult
            if (currentIndex >= 0)
            {
                m_interactables.Remove(currentIndex);
            }

            return false;
        }

        // set new values after filtering the scanResult
        if (currentIndex >= 0)
        {
            m_interactables[currentIndex] = scanResult;
        }

        // find sorted position position for new scanResult (base on priority)
        int targetIndex = -1;
        for (int i = 0; i < m_interactables.Length; i++)
        {
            if (CompareScanResult(m_interactables[i], scanResult) >= 0)
            {
                targetIndex = i;
                break;
            }
        }

        // if targetIndex found
        if (targetIndex >= 0)
        {
            if (currentIndex >= 0)
            {
                // if the scanResult already exists, just move it
                m_interactables.Move(currentIndex, targetIndex);
                return true;
            }

            // add the new scanResult
            m_interactables.Insert(scanResult, targetIndex);
            m_interactablesCount++;
            return true;
        }

        // no appropriate position found for the new scanResult
        return false;
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

        if (lhs.interactable == rhs.interactable)
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

    protected void GenerateOnInteractableFoundEvents()
    {
        foreach (var scanResult in m_interactables)
        {
            bool found = false;
            if (scanResult.isValid == false)
            {
                break;
            }

            foreach (var oldScanResult in m_oldInteractables)
            {
                if (oldScanResult.isValid == false)
                {
                    break;
                }

                if (scanResult.interactable == oldScanResult.interactable)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                OnInteractableFound(scanResult);
            }
        }
    }

    protected void GenerateOnInteractableLostEvents()
    {
        foreach (var oldScanResult in m_oldInteractables)
        {
            bool found = false;
            if (oldScanResult.isValid == false)
            {
                break;
            }

            foreach (var scanResult in m_interactables)
            {
                if (scanResult.isValid == false)
                {
                    break;
                }

                if (scanResult.interactable == oldScanResult.interactable)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                OnInteractableLost(oldScanResult.interactable);
            }
        }
    }

    protected virtual void OnInteractableFound(InteractableScanResult scanResult)
    {
        var interactable = scanResult.interactable;
        Debug.Log($"Interactable Found | {interactable.name}");
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
            Debug.Log("Weapon Found");
            charEquip.OnWeaponFound(weapon);
            return;
        }
    }

    protected virtual void OnInteractableLost(Interactable interactable)
    {
        Debug.Log($"Interactable Lost | {interactable.name}");
    }

    //////////////////////////////////////////////////////////////////
    /// Public API
    //////////////////////////////////////////////////////////////////

    public void ForEachScanResult(Action<InteractableScanResult> action)
    {
        if (action != null)
        {
            foreach (var scanResult in m_interactables)
            {
                action(scanResult);
            }
        }
    }

    public void ForEachInteractable(Action<Interactable> action)
    {
        if (action != null)
        {
            foreach (var scanResult in m_interactables)
            {
                action(scanResult.interactable);
            }
        }
    }

    public InteractableScanResult FindScanResult(Predicate<InteractableScanResult> pred)
    {
        if (pred != null)
        {
            foreach (var scanResult in m_interactables)
            {
                if (pred(scanResult))
                {
                    return scanResult;
                }
            }
        }

        return InteractableScanResult.invalid;
    }

    public Interactable FindInteractable(Predicate<InteractableScanResult> pred)
    {
        if (pred != null)
        {
            foreach (var scanResult in m_interactables)
            {
                if (pred(scanResult))
                {
                    return scanResult.interactable;
                }
            }
        }

        return null;
    }

    public Interactable FindInteractable(Predicate<Interactable> pred)
    {
        if (pred != null)
        {
            foreach (var scanResult in m_interactables)
            {
                if (pred(scanResult.interactable))
                {
                    return scanResult.interactable;
                }
            }
        }

        return null;
    }

    public InteractableScanResult GetScanResult(Interactable interactable)
    {
        if (interactable != null)
        {
            foreach (var scanResult in m_interactables)
            {
                if (scanResult.interactable == interactable)
                    return scanResult;
            }
        }

        return InteractableScanResult.invalid;
    }

    public bool HasInteractable(Interactable interactable)
    {
        if (interactable != null)
        {
            foreach (var scanResult in m_interactables)
            {
                if (scanResult.interactable == interactable)
                {
                    return true;
                }
            }
        }

        return false;
    }
}