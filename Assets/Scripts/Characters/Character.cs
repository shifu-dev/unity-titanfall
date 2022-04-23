using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterCamera))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public class Character : MonoBehaviour
{
    public CharacterMovement CharacterMovement { get => _CharacterMovement; }
    public CharacterInteraction CharacterInteraction { get => _CharacterInteraction; }
    public CharacterWeapon CharacterWeapon { get => _CharacterWeapon; }
    public CharacterCamera CharacterCamera { get => _CharacterCamera; }
    public CharacterCapsule CharacterCapsule { get => _CharacterCapsule; }
    public CharacterAnimation CharacterAnimation { get => _CharacterAnimation; }
    public CharacterInputs CharacterInputs { get => _CharacterInputs; }

    [NonSerialized] protected CharacterMovement _CharacterMovement;
    [NonSerialized] protected CharacterInteraction _CharacterInteraction;
    [NonSerialized] protected CharacterWeapon _CharacterWeapon;
    [NonSerialized] protected CharacterCamera _CharacterCamera;
    [NonSerialized] protected CharacterCapsule _CharacterCapsule;
    [NonSerialized] protected CharacterAnimation _CharacterAnimation;
    [NonSerialized] protected CharacterInputs _CharacterInputs;

    void Awake()
    {
        _CharacterMovement = GetComponent<CharacterMovement>();
        _CharacterInteraction = GetComponent<CharacterInteraction>();
        _CharacterWeapon = GetComponent<CharacterWeapon>();
        _CharacterCamera = GetComponent<CharacterCamera>();
        _CharacterCapsule = GetComponent<CharacterCapsule>();
        _CharacterAnimation = GetComponent<CharacterAnimation>();

        _CharacterMovement.Init(this);
        _CharacterInteraction.Init(this);
        _CharacterWeapon.Init(this);
        _CharacterCamera.Init(this);
        _CharacterCapsule.Init(this);
        _CharacterAnimation.Init(this);
    }

    void Start()
    {
    }

    void Update()
    {
        _CharacterMovement.UpdateImpl();
        _CharacterInteraction.UpdateImpl();
        _CharacterWeapon.UpdateImpl();
        _CharacterCamera.UpdateImpl();
        _CharacterCapsule.UpdateImpl();
        _CharacterAnimation.UpdateImpl();
    }

    public void OnPossessed(Player Player)
    {
        if (Player == null)
            return;

        _CharacterInputs.InputChannel = Player.PlayerInputs;
        _CharacterMovement.MovementInputs = Player.PlayerInputs;
    }

    public void OnUnPossessed()
    {
        _CharacterInputs.InputChannel = null;
    }
}