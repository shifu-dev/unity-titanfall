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

    public bool isValid => interactable is not null;

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
    protected CharacterEquip _charEquip;

    [SerializeField] protected InteractableScanResult[] _interactables;
    [SerializeField, ReadOnly] protected int _interactablesCount;

    public int interactablesCapacity
    {
        get => _interactables.Length;
        set
        {
            if (value >= 0 && value != interactablesCapacity)
            {
                Array.Resize(ref _interactables, value);
                interactablesCount = Math.Min(value, interactablesCount);
            }
        }
    }
    public int interactablesCount
    {
        get => _interactablesCount;
        protected set
        {
            _interactablesCount = Math.Max(0, value);
        }
    }

    [Space]
    [SerializeField, Min(0)] protected float _updateInterval;
    [SerializeField] protected LayerMask _layerMask;
    [SerializeField] protected QueryTriggerInteraction _triggerQuery;
    [SerializeField, Label("On Added Event")] protected bool _generateOnAddedEvent;
    [SerializeField, Label("On Updated Event")] protected bool _generateOnUpdatedEvent;
    [SerializeField, Label("On Removed Event")] protected bool _generateOnRemovedEvent;
    protected float _lastUpdateTime;

    [Header("Overlaps"), Space]
    [SerializeField] protected bool _performOverlaps;
    [SerializeField] protected Vector3 _overlapSize;
    [SerializeField] protected Vector3 _overlapCenterOffset;

    [Header("Raycasts"), Space]
    [SerializeField] protected bool _performRaycasts;
    [SerializeField] protected Transform _raycastSource;
    [SerializeField, Min(0)] protected float _raycastLength;

    public CharacterInteraction()
    {
        _interactables = Array.Empty<InteractableScanResult>();
        interactablesCapacity = 0;

        _layerMask = default;
        _triggerQuery = QueryTriggerInteraction.Collide;
        _generateOnAddedEvent = true;
        _generateOnRemovedEvent = false;
        _updateInterval = .5f;
        _lastUpdateTime = 0;
        _performOverlaps = false;
        _overlapSize = new Vector3(1.8f, 1.5f, 2f);
        _overlapCenterOffset = new Vector3(0, 0, .2f);
        _performRaycasts = false;
        _raycastSource = null;
        _raycastLength = 3;
    }

    //////////////////////////////////////////////////////////////////
    /// UpdateLoop | BEGIN

    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        _charEquip = character.charEquip;
    }

    public override void OnCharacterUpdate()
    {
        base.OnCharacterUpdate();

        CollectInteractables();
    }

    public bool CollectInteractables(bool force = false)
    {
        if (force == false)
        {
            if (Time.time < _lastUpdateTime + _updateInterval)
            {
                return false;
            }
        }

        _lastUpdateTime = Time.time;

        FindInteractables();
        RemovePreviousScanResults();

        return true;
    }

    /// UpdateLoop | END
    //////////////////////////////////////////////////////////////////

    protected void FindInteractables()
    {
        if (interactablesCapacity == 0)
            return;

        InteractableScanResult raycastScanResult = InteractableScanResult.invalid;
        if (_performRaycasts)
        {
            if (_raycastSource is not null)
            {
                Camera camera = character.charView.camera;
                if (camera is not null)
                {
                    Transform source = camera.transform;
                    _raycastSource.position = source.position;
                    _raycastSource.rotation = source.rotation;
                }

                bool hit = Physics.Raycast(_raycastSource.position, _raycastSource.forward, out RaycastHit hitInfo,
                    _raycastLength, _layerMask, _triggerQuery);

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

        if (_performOverlaps)
        {
            /// @todo calculate halfExtents with respect to character size
            Vector3 halfExtents = _overlapSize * .5f;
            Vector3 center = _character.transform.position + (_character.up * halfExtents.y) + _overlapCenterOffset;
            Collider[] overlapResults = overlapResults = Physics.OverlapBox(center, halfExtents, Quaternion.identity,
                    _layerMask, _triggerQuery);

            foreach (var collider in overlapResults)
            {
                Interactable interactable = Interactable.GetInteractable(collider);
                if (interactable is not null)
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
            if (_interactables[i].interactable is null)
                continue;

            if (_interactables[i].HasThisFrame(thisFrame) == false)
            {
                RemoveScanResult(i);
                i--;
            }
        }
    }

    protected InteractableScanResult GenerateRaycastScanResult(Interactable interactable, RaycastHit hit)
    {
        if (interactable is null)
            return InteractableScanResult.invalid;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.raycasted = true;

        return scanResult;
    }

    protected InteractableScanResult GenerateOverlapScanResult(Interactable interactable)
    {
        if (interactable is null)
            return InteractableScanResult.invalid;

        InteractableScanResult scanResult = new InteractableScanResult();
        scanResult.interactable = interactable;
        scanResult.overlapped = true;

        return scanResult;
    }

    protected InteractableScanResult GenerateManualScanResult(Interactable interactable)
    {
        if (interactable is null)
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
        for (int i = 0; i < _interactablesCount; i++)
        {
            if (_interactables[i].interactable == scanResult.interactable)
            {
                wasPresent = true;
                scanResult = UpdateScanResult(_interactables[i], scanResult);

                // if previous scanResult exists, remove it
                _interactables.Remove(i, InteractableScanResult.invalid);
                _interactablesCount--;

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
            if (i < interactablesCount && CompareScanResult(_interactables[i], scanResult) <= 0)
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
                if (_generateOnUpdatedEvent)
                {
                    OnInteractableUpdated(scanResult);
                }
            }
            else if (_generateOnAddedEvent)
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

        _interactables.Insert(scanResult, index);
        _interactablesCount++;
    }

    protected void RemoveScanResult(int index)
    {
        var scanResult = _interactables[index];
        _interactables.Remove(index);
        _interactablesCount--;

        if (_generateOnRemovedEvent)
        {
            OnInteractableRemoved(scanResult.interactable);
        }
    }

    protected bool FilterScanResult(InteractableScanResult scanResult)
    {
        var interactable = scanResult.interactable;
        if (interactable is null)
            return false;

        if (interactable.canInteract == false)
            return false;

        if (interactable.requireRaycast && scanResult.raycasted == false)
            return false;

        return true;
    }

    protected int CompareScanResult(InteractableScanResult lhs, InteractableScanResult rhs)
    {
        if (lhs.interactable is null && rhs.interactable is null)
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
        if (interactable is null)
            return;

        Grenade grenade = interactable.GetComponent<Grenade>();
        if (grenade is not null)
        {
            // charEquip.OnGrenadeFound(scanResult, grenade);
            return;
        }

        Weapon weapon = interactable.GetComponent<Weapon>();
        if (weapon is not null)
        {
            // charEquip.OnWeaponFound(scanResult, weapon);
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
    /// Public API | BEGIN

    public void ForEachScanResult(Action<InteractableScanResult> action)
    {
        if (action is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                action(_interactables[i]);
            }
        }
    }

    public void ForEachInteractable(Action<Interactable> action)
    {
        if (action is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                action(_interactables[i].interactable);
            }
        }
    }

    public InteractableScanResult FindScanResult(Predicate<InteractableScanResult> pred)
    {
        if (pred is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(_interactables[i]))
                {
                    return _interactables[i];
                }
            }
        }

        return InteractableScanResult.invalid;
    }

    public Interactable FindInteractable(Predicate<InteractableScanResult> pred)
    {
        if (pred is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(_interactables[i]))
                {
                    return _interactables[i].interactable;
                }
            }
        }

        return null;
    }

    public Interactable FindInteractable(Predicate<Interactable> pred)
    {
        if (pred is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (pred(_interactables[i].interactable))
                {
                    return _interactables[i].interactable;
                }
            }
        }

        return null;
    }

    public InteractableScanResult GetScanResult(Interactable interactable)
    {
        if (interactable is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (_interactables[i].interactable == interactable)
                    return _interactables[i];
            }
        }

        return InteractableScanResult.invalid;
    }

    public bool HasInteractable(Interactable interactable)
    {
        if (interactable is not null)
        {
            for (int i = 0; i < interactablesCount; i++)
            {
                if (_interactables[i].interactable == interactable)
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
            if (_interactables[i].interactable &&
                _interactables[i].interactable.TryGetComponent<T>(out T foundComp))
            {
                comp = foundComp;
                return _interactables[i].interactable;
            }
        }

        comp = default;
        return null;
    }

    public Interactable GetInteractableWith<T>()
    {
        return GetInteractableWith<T>(out T comp);
    }

    /// Public API | END
    //////////////////////////////////////////////////////////////////
}