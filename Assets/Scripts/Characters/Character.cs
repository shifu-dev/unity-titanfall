using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInputs))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterEquip))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterGrenade))]
[RequireComponent(typeof(CharacterCamera))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public class Character : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////
    public CharacterBehaviour[] characterBehaviours { get; protected set; }
    public CharacterInputs characterInputs { get; protected set; }
    public CharacterMovement characterMovement { get; protected set; }
    public CharacterInteraction characterInteraction { get; protected set; }
    public CharacterEquip characterEquip { get; protected set; }
    public CharacterWeapon characterWeapon { get; protected set; }
    public CharacterGrenade characterGrenade { get; protected set; }
    public CharacterCamera characterCamera { get; protected set; }
    public CharacterCapsule characterCapsule { get; protected set; }
    public CharacterAnimation characterAnimation { get; protected set; }

    public float Mass { get; set; } = 80f;
    public float ScaledMass
    {
        get => transform.lossyScale.magnitude * Mass;
        set => Mass = value / transform.lossyScale.magnitude;
    }

    public Quaternion GetRotation => transform.rotation;
    public Vector3 GetForward => transform.forward;
    public Vector3 GetBack => -transform.forward;
    public Vector3 GetRight => transform.right;
    public Vector3 GetLeft => -transform.right;
    public Vector3 GetUp => transform.up;
    public Vector3 GetDown => -transform.up;

    void Awake()
    {
        CollectBehaviours();

        characterInputs = GetBehaviour<CharacterInputs>();
        characterMovement = GetBehaviour<CharacterMovement>();
        characterInteraction = GetBehaviour<CharacterInteraction>();
        characterEquip = GetBehaviour<CharacterEquip>();
        characterWeapon = GetBehaviour<CharacterWeapon>();
        characterGrenade = GetBehaviour<CharacterGrenade>();
        characterCamera = GetBehaviour<CharacterCamera>();
        characterCapsule = GetBehaviour<CharacterCapsule>();
        characterAnimation = GetBehaviour<CharacterAnimation>();

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
        foreach (var behaviour in characterBehaviours)
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

        characterBehaviours = weaponBehavioursList.ToArray();
    }

    protected virtual void InitBehaviours(CharacterInitializer initializer)
    {
        foreach (var behaviour in characterBehaviours)
        {
            behaviour.OnInitCharacter(this, initializer);
        }
    }

    protected virtual void UpdateBehaviours()
    {
        foreach (var behaviour in characterBehaviours)
        {
            behaviour.OnUpdateCharacter();
        }
    }

    protected virtual void FixedUpdateBehaviours()
    {
        foreach (var behaviour in characterBehaviours)
        {
            behaviour.OnFixedUpdateCharacter();
        }
    }

    protected virtual void DestroyBehaviours()
    {
        foreach (var behaviour in characterBehaviours)
        {
            behaviour.OnDestroyCharacter();
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Player
    //////////////////////////////////////////////////////////////////

    public void OnPossessed(Player Player)
    {
        if (Player == null)
            return;

        characterInputs.playerInputs = Player.PlayerInputs;
    }

    public void OnUnPossessed()
    {
        characterInputs.playerInputs = null;
    }
}