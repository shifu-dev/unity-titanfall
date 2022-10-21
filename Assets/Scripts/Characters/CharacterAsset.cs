using System;
using UnityEngine;

[CreateAssetMenu(menuName = MENU_PATH + MENU_NAME, fileName = FILE_NAME)]
public partial class CharacterAsset : DataAsset
{
    public const string MENU_PATH = "Character/";
    public const string MENU_NAME = "Character Asset";
    public const string FILE_NAME = "Character Asset";

    public CharacterAsset()
    {
        _instanceHandler = new InstanceHandler(this);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        _instanceHandler.CreatePool();
    }

    public override void OnUnload()
    {
        base.OnUnload();

        _instanceHandler.DisposePool();
    }

    [Space]

    [Label("Character Name"), SerializeField]
    protected string _characterName;
    public string characterName => _characterName;

    [Label("Character TPP Body"), SerializeField]
    protected GameObject _tppBody;
    public GameObject tppBody => _tppBody;

    [Label("Character FPP Body"), SerializeField]
    protected GameObject _fppBody;
    public GameObject fppBody => _fppBody;

    [Label("Character TPP Prefab"), SerializeField]
    protected GameObject _tppPrefab;
    public GameObject tppPrefab => _tppPrefab;

    [Label("Character FPP Prefab"), SerializeField]
    protected GameObject _fppPrefab;
    public GameObject fppPrefab => _fppPrefab;

    [Label("Character Mass"), SerializeField, Min(0)]
    protected float _characterMass;
    public float characterMass => _characterMass;

    [Space, Header("CHARACTER MOVEMENT")]

    [SerializeField, Label("Ground Module Source")]
    protected CharacterMovementGroundModuleSource mGroundModuleSource;
    public CharacterMovementGroundModuleSource groundModuleSource => mGroundModuleSource;

    [SerializeField, Label("Air Module Source")]
    protected CharacterMovementAirModuleSource mAirModuleSource;
    public CharacterMovementAirModuleSource airModuleSource => mAirModuleSource;

    //////////////////////////////////////////////////////////////////
    /// Animations | BEGIN

    [Space, Header("CHARACTER ANIMATION")]

    [Label("Avatar"), SerializeField]
    protected Avatar _avatar;
    public Avatar avatar => _avatar;

    [Label("Avatar Mask Upper Body"), SerializeField]
    protected Avatar _avatarMaskUpperBody;
    public Avatar AvatarMaskUpperBody => _avatarMaskUpperBody;

    [Label("Avatar Mask Lower Body"), SerializeField]
    protected Avatar _avatarMaskLowerBody;
    public Avatar AvatarMaskLowerBody => _avatarMaskLowerBody;

    [Header("Ground Stand Animations")]

    [Label("Idle"), SerializeField]
    protected AnimationClip _animGroundStandIdle;
    public AnimationClip animGroundStandIdle => _animGroundStandIdle;

    //////////////////////////////////////////////////////////////////

    [Label("Walk Forward"), SerializeField]
    protected AnimationClip _animGroundStandWalkForward;
    public AnimationClip animGroundStandWalkForward => _animGroundStandWalkForward;

    [Label("Walk Forward Left"), SerializeField]
    protected AnimationClip _animGroundStandWalkForwardLeft;
    public AnimationClip animGroundStandWalkForwardLeft => _animGroundStandWalkForwardLeft;

    [Label("Walk Forward Right"), SerializeField]
    protected AnimationClip _animGroundStandWalkForwardRight;
    public AnimationClip animGroundStandWalkForwardRight => _animGroundStandWalkForwardRight;

    [Label("Walk Left"), SerializeField]
    protected AnimationClip _animGroundStandWalkLeft;
    public AnimationClip animGroundStandWalkLeft => _animGroundStandWalkLeft;

    [Label("Walk Right"), SerializeField]
    protected AnimationClip _animGroundStandWalkRight;
    public AnimationClip animGroundStandWalkRight => _animGroundStandWalkRight;

    [Label("Walk Backward"), SerializeField]
    protected AnimationClip _animGroundStandWalkBackward;
    public AnimationClip animGroundStandWalkBackward => _animGroundStandWalkBackward;

    [Label("Walk Backward Left"), SerializeField]
    protected AnimationClip _animGroundStandWalkBackwardLeft;
    public AnimationClip animGroundStandWalkBackwardLeft => _animGroundStandWalkBackwardLeft;

    [Label("Walk Backward Right"), SerializeField]
    protected AnimationClip _animGroundStandWalkBackwardRight;
    public AnimationClip animGroundStandWalkBackwardRight => _animGroundStandWalkBackwardRight;

    //////////////////////////////////////////////////////////////////

    [Label("Run Forward"), SerializeField]
    protected AnimationClip _animGroundStandRunForward;
    public AnimationClip animGroundStandRunForward => _animGroundStandRunForward;

    [Label("Run Forward Left"), SerializeField]
    protected AnimationClip _animGroundStandRunForwardLeft;
    public AnimationClip animGroundStandRunForwardLeft => _animGroundStandRunForwardLeft;

    [Label("Run Forward Right"), SerializeField]
    protected AnimationClip _animGroundStandRunForwardRight;
    public AnimationClip animGroundStandRunForwardRight => _animGroundStandRunForwardRight;

    [Label("Run Left"), SerializeField]
    protected AnimationClip _animGroundStandRunLeft;
    public AnimationClip animGroundStandRunLeft => _animGroundStandRunLeft;

    [Label("Run Right"), SerializeField]
    protected AnimationClip _animGroundStandRunRight;
    public AnimationClip animGroundStandRunRight => _animGroundStandRunRight;

    [Label("Run Backward"), SerializeField]
    protected AnimationClip _animGroundStandRunBackward;
    public AnimationClip animGroundStandRunBackward => _animGroundStandRunBackward;

    [Label("Run Backward Left"), SerializeField]
    protected AnimationClip _animGroundStandRunBackwardLeft;
    public AnimationClip animGroundStandRunBackwardLeft => _animGroundStandRunBackwardLeft;

    [Label("Run Backward Right"), SerializeField]
    protected AnimationClip _animGroundStandRunBackwardRight;
    public AnimationClip animGroundStandRunBackwardRight => _animGroundStandRunBackwardRight;

    //////////////////////////////////////////////////////////////////

    [Label("Sprint Forward"), SerializeField]
    protected AnimationClip _animGroundStandSprintForward;
    public AnimationClip animGroundStandSprintForward => _animGroundStandSprintForward;

    /// Stand Animations | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Crouch Animations | BEGIN

    [Header("Ground Crouch Animations")]

    [Label("Idle"), SerializeField]
    protected AnimationClip _animGroundCrouchIdle;
    public AnimationClip animGroundCrouchIdle => _animGroundCrouchIdle;

    //////////////////////////////////////////////////////////////////

    [Label("Walk Forward"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkForward;
    public AnimationClip animGroundCrouchWalkForward => _animGroundCrouchWalkForward;

    [Label("Walk Forward Left"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkForwardLeft;
    public AnimationClip animGroundCrouchWalkForwardLeft => _animGroundCrouchWalkForwardLeft;

    [Label("Walk Forward Right"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkForwardRight;
    public AnimationClip animGroundCrouchWalkForwardRight => _animGroundCrouchWalkForwardRight;

    [Label("Walk Left"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkLeft;
    public AnimationClip animGroundCrouchWalkLeft => _animGroundCrouchWalkLeft;

    [Label("Walk Right"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkRight;
    public AnimationClip animGroundCrouchWalkRight => _animGroundCrouchWalkRight;

    [Label("Walk Backward"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkBackward;
    public AnimationClip animGroundCrouchWalkBackward => _animGroundCrouchWalkBackward;

    [Label("Walk Backward Left"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkBackwardLeft;
    public AnimationClip animGroundCrouchWalkBackwardLeft => _animGroundCrouchWalkBackwardLeft;

    [Label("Walk Backward Right"), SerializeField]
    protected AnimationClip _animGroundCrouchWalkBackwardRight;
    public AnimationClip animGroundCrouchWalkBackwardRight => _animGroundCrouchWalkBackwardRight;

    //////////////////////////////////////////////////////////////////

    [Label("Run Forward"), SerializeField]
    protected AnimationClip _animGroundCrouchRunForward;
    public AnimationClip animGroundCrouchRunForward => _animGroundCrouchRunForward;

    [Label("Run Forward Left"), SerializeField]
    protected AnimationClip _animGroundCrouchRunForwardLeft;
    public AnimationClip animGroundCrouchRunForwardLeft => _animGroundCrouchRunForwardLeft;

    [Label("Run Forward Right"), SerializeField]
    protected AnimationClip _animGroundCrouchRunForwardRight;
    public AnimationClip animGroundCrouchRunForwardRight => _animGroundCrouchRunForwardRight;

    [Label("Run Left"), SerializeField]
    protected AnimationClip _animGroundCrouchRunLeft;
    public AnimationClip animGroundCrouchRunLeft => _animGroundCrouchRunLeft;

    [Label("Run Right"), SerializeField]
    protected AnimationClip _animGroundCrouchRunRight;
    public AnimationClip animGroundCrouchRunRight => _animGroundCrouchRunRight;

    [Label("Run Backward"), SerializeField]
    protected AnimationClip _animGroundCrouchRunBackward;
    public AnimationClip animGroundCrouchRunBackward => _animGroundCrouchRunBackward;

    [Label("Run Backward Left"), SerializeField]
    protected AnimationClip _animGroundCrouchRunBackwardLeft;
    public AnimationClip animGroundCrouchRunBackwardLeft => _animGroundCrouchRunBackwardLeft;

    [Label("Run Backward Right"), SerializeField]
    protected AnimationClip _animGroundCrouchRunBackwardRight;
    public AnimationClip animGroundCrouchRunBackwardRight => _animGroundCrouchRunBackwardRight;

    /// Crouch Animations | END
    //////////////////////////////////////////////////////////////////

    /// Animations | END
    //////////////////////////////////////////////////////////////////
}