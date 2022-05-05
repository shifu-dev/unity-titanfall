using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInputs))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterCamera))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public class Character : MonoBehaviour
{
    public CharacterInputs CharacterInputs { get => _CharacterInputs; }
    public CharacterMovement CharacterMovement { get => _CharacterMovement; }
    public CharacterInteraction CharacterInteraction { get => _CharacterInteraction; }
    public CharacterWeapon CharacterWeapon { get => _CharacterWeapon; }
    public CharacterCamera CharacterCamera { get => _CharacterCamera; }
    public CharacterCapsule CharacterCapsule { get => _CharacterCapsule; }
    public CharacterAnimation CharacterAnimation { get => _CharacterAnimation; }

    [NonSerialized] protected CharacterInputs _CharacterInputs;
    [NonSerialized] protected CharacterMovement _CharacterMovement;
    [NonSerialized] protected CharacterInteraction _CharacterInteraction;
    [NonSerialized] protected CharacterWeapon _CharacterWeapon;
    [NonSerialized] protected CharacterCamera _CharacterCamera;
    [NonSerialized] protected CharacterCapsule _CharacterCapsule;
    [NonSerialized] protected CharacterAnimation _CharacterAnimation;

    public Vector3 GetForward => transform.forward;
    public Vector3 GetBack => -transform.forward;
    public Vector3 GetRight => transform.right;
    public Vector3 GetLeft => -transform.right;
    public Vector3 GetUp => transform.up;
    public Vector3 GetDown => -transform.up;

    void Awake()
    {
        _CharacterInputs = GetComponent<CharacterInputs>();
        _CharacterMovement = GetComponent<CharacterMovement>();
        _CharacterInteraction = GetComponent<CharacterInteraction>();
        _CharacterWeapon = GetComponent<CharacterWeapon>();
        _CharacterCamera = GetComponent<CharacterCamera>();
        _CharacterCapsule = GetComponent<CharacterCapsule>();
        _CharacterAnimation = GetComponent<CharacterAnimation>();

        _CharacterInputs.Init(this);
        _CharacterMovement.Init(this);
        _CharacterCamera.Init(this);
        _CharacterCapsule.Init(this);
        _CharacterInteraction.Init(this);
        _CharacterWeapon.Init(this);
        _CharacterAnimation.Init(this);
    }

    void Start()
    {
    }

    void Update()
    {
        _CharacterInputs.UpdateImpl();
        _CharacterMovement.UpdateImpl();
        _CharacterCapsule.UpdateImpl();
        _CharacterCamera.UpdateImpl();
        _CharacterInteraction.UpdateImpl();
        _CharacterWeapon.UpdateImpl();
        _CharacterAnimation.UpdateImpl();
    }

    void FixedUpdate()
    {
        _CharacterMovement.FixedUpdateImpl();
    }

    public void OnPossessed(Player Player)
    {
        if (Player == null)
            return;

        _CharacterInputs.PlayerInputs = Player.PlayerInputs;
    }

    public void OnUnPossessed()
    {
        _CharacterInputs.PlayerInputs = null;
    }
}