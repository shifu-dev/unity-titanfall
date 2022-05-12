using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInputs))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
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

    public CharacterInputs characterInputs { get; protected set; }
    public CharacterMovement characterMovement { get; protected set; }
    public CharacterInteraction characterInteraction { get; protected set; }
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
        characterInputs = GetComponent<CharacterInputs>();
        characterMovement = GetComponent<CharacterMovement>();
        characterInteraction = GetComponent<CharacterInteraction>();
        characterWeapon = GetComponent<CharacterWeapon>();
        characterGrenade = GetComponent<CharacterGrenade>();
        characterCamera = GetComponent<CharacterCamera>();
        characterCapsule = GetComponent<CharacterCapsule>();
        characterAnimation = GetComponent<CharacterAnimation>();

        // get initializer
        var initializer = GetComponent<CharacterInitializer>();

        characterInputs.OnInitCharacter(this, initializer);
        characterMovement.OnInitCharacter(this, initializer);
        characterCamera.OnInitCharacter(this, initializer);
        characterCapsule.OnInitCharacter(this, initializer);
        characterInteraction.OnInitCharacter(this, initializer);
        characterWeapon.OnInitCharacter(this, initializer);
        characterGrenade.OnInitCharacter(this, initializer);
        characterAnimation.OnInitCharacter(this, initializer);

        // destroy initializer
        Destroy(initializer);
    }

    void Update()
    {
        characterInputs.OnUpdateCharacter();
        characterMovement.OnUpdateCharacter();
        characterCapsule.OnUpdateCharacter();
        characterCamera.OnUpdateCharacter();
        characterInteraction.OnUpdateCharacter();
        characterWeapon.OnUpdateCharacter();
        characterGrenade.OnUpdateCharacter();
        characterAnimation.OnUpdateCharacter();
    }

    void FixedUpdate()
    {
        characterInputs.OnFixedUpdateCharacter();
        characterMovement.OnFixedUpdateCharacter();
        characterCapsule.OnFixedUpdateCharacter();
        characterCamera.OnFixedUpdateCharacter();
        characterInteraction.OnFixedUpdateCharacter();
        characterWeapon.OnFixedUpdateCharacter();
        characterGrenade.OnFixedUpdateCharacter();
        characterAnimation.OnFixedUpdateCharacter();
    }

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