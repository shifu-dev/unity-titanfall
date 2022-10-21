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
}