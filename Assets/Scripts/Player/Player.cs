using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerHUD))]
public class Player : MonoBehaviour
{
    public PlayerInputs playerInputs { get; protected set; }
    public PlayerState PlayerState { get; protected set; }
    public PlayerCamera playerCamera { get; protected set; }
    public PlayerHUD playerHUD { get; protected set; }
    public Character character { get; protected set; }

    [ReadOnly]
    [SerializeField]
    protected Character _Character;

    void Awake()
    {
        playerInputs = GetComponent<PlayerInputs>();
        playerCamera = GetComponent<PlayerCamera>();
        PlayerState = GetComponent<PlayerState>();
        playerHUD = GetComponent<PlayerHUD>();

        playerInputs.Init(this);
        playerCamera.Init(this);
        PlayerState.Init(this);
        playerHUD.Init(this);
    }

    void Start()
    {
    }

    void Update()
    {
        playerInputs.UpdateImpl();
        playerCamera.UpdateImpl();
        PlayerState.UpdateImpl();
        playerHUD.UpdateImpl();
    }

    public virtual void Possess(Character character)
    {
        _Character = character;
        _Character.OnPossessed(this);

        this.OnPossessed(_Character);
        playerCamera.OnPossessed(character);
    }

    protected virtual void OnPossessed(Character character)
    {
    }

    public virtual void Unpossess()
    {
        if (_Character)
        {
            _Character.OnUnPossessed();
            OnUnpossessed(_Character);
        }
    }

    protected virtual void OnUnpossessed(Character character)
    {
    }
}