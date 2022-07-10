using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInputs))]
[RequireComponent(typeof(CharacterInventory))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterEquip))]
[RequireComponent(typeof(CharacterView))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public class Character : Equipable
{
    //////////////////////////////////////////////////////////////////
    /// Equipable
    //////////////////////////////////////////////////////////////////

    public override EquipableType type => EquipableType.Character;

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////
    [field: Label("Data Asset"), SerializeField]
    public CharacterDataAsset charDataAsset { get; protected set; }
    public CharacterBehaviour[] charBehaviours { get; protected set; }
    public CharacterInputs charInputs { get; protected set; }
    public CharacterInventory charInventory { get; protected set; }
    public CharacterMovement charMovement { get; protected set; }
    public CharacterInteraction charInteraction { get; protected set; }
    public CharacterEquip charEquip { get; protected set; }
    public CharacterView charView { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterAnimation charAnimation { get; protected set; }

    public float mass => charDataAsset.characterMass;
    public float scaledMass
    {
        get => transform.lossyScale.magnitude * mass;
    }

    public Quaternion rotation => transform.rotation;
    public Vector3 forward => transform.forward;
    public Vector3 back => -transform.forward;
    public Vector3 right => transform.right;
    public Vector3 left => -transform.right;
    public Vector3 up => transform.up;
    public Vector3 down => -transform.up;

    void Awake()
    {
        CollectBehaviours();

        charInputs = GetBehaviour<CharacterInputs>();
        charInventory = GetBehaviour<CharacterInventory>();
        charMovement = GetBehaviour<CharacterMovement>();
        charInteraction = GetBehaviour<CharacterInteraction>();
        charEquip = GetBehaviour<CharacterEquip>();
        charView = GetBehaviour<CharacterView>();
        charCapsule = GetBehaviour<CharacterCapsule>();
        charAnimation = GetBehaviour<CharacterAnimation>();

        var initializer = GetComponent<CharacterInitializer>();
        InitBehaviours(initializer);
        Destroy(initializer);
    }

    void Update()
    {
        UpdateBehaviours();
    }

    void FixedUpdate()
    {
        FixedUpdateBehaviours();
    }

    void OnDisable()
    {
        DestroyBehaviours();
    }

    //////////////////////////////////////////////////////////////////
    /// Behaviours
    //////////////////////////////////////////////////////////////////

    public virtual T GetBehaviour<T>()
        where T : CharacterBehaviour
    {
        foreach (var behaviour in charBehaviours)
        {
            T customBehaviour = behaviour as T;
            if (customBehaviour != null)
            {
                return customBehaviour;
            }
        }

        return null;
    }

    public virtual bool HasBehaviour<T>()
        where T : CharacterBehaviour
    {
        return GetBehaviour<T>() != null;
    }

    protected virtual void CollectBehaviours(params CharacterBehaviour[] exceptions)
    {
        List<CharacterBehaviour> weaponBehavioursList = new List<CharacterBehaviour>();
        GetComponents<CharacterBehaviour>(weaponBehavioursList);

        // no need to check for exceptional behaviours, if there are none
        if (exceptions.Length > 0)
        {
            weaponBehavioursList.RemoveAll((CharacterBehaviour behaviour) =>
            {
                foreach (var exceptionBehaviour in exceptions)
                {
                    if (behaviour == exceptionBehaviour)
                        return true;
                }

                return false;
            });
        }

        charBehaviours = weaponBehavioursList.ToArray();
    }

    protected virtual void InitBehaviours(CharacterInitializer initializer)
    {
        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnInitCharacter(this, initializer);
        }
    }

    protected virtual void UpdateBehaviours()
    {
        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnUpdateCharacter();
        }
    }

    protected virtual void FixedUpdateBehaviours()
    {
        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnFixedUpdateCharacter();
        }
    }

    protected virtual void DestroyBehaviours()
    {
        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnDestroyCharacter();
        }
    }

    public void OnPossessed(Player Player)
    {
        if (Player == null)
            return;

        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnPossessed(Player);
        }
    }

    public void OnUnPossessed()
    {
        foreach (var behaviour in charBehaviours)
        {
            behaviour.OnUnPossessed();
        }
    }
}