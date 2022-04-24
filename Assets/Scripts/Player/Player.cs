using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerHUD))]
public class Player : MonoBehaviour
{
    public PlayerInputs PlayerInputs { get => _PlayerInputs; }
    public PlayerState PlayerState { get => _PlayerState; }
    public PlayerCamera PlayerCamera { get => _PlayerCamera; }
    public PlayerHUD PlayerHUD { get => _PlayerHUD; }
    public Character Chararcter { get => _Character; }

    [NonSerialized] protected PlayerInputs _PlayerInputs;
    [NonSerialized] protected PlayerState _PlayerState;
    [NonSerialized] protected PlayerCamera _PlayerCamera;
    [NonSerialized] protected PlayerHUD _PlayerHUD;
    [NonSerialized] protected Character _Character;

    void Awake()
    {
        _PlayerInputs = GetComponent<PlayerInputs>();
        _PlayerCamera = GetComponent<PlayerCamera>();
        _PlayerState = GetComponent<PlayerState>();
        _PlayerHUD = GetComponent<PlayerHUD>();

        _PlayerInputs.Init(this);
        _PlayerCamera.Init(this);
        _PlayerState.Init(this);
        _PlayerHUD.Init(this);
    }

    void Start()
    {
    }

    void Update()
    {
        _PlayerInputs.UpdateImpl();
        _PlayerCamera.UpdateImpl();
        _PlayerState.UpdateImpl();
        _PlayerHUD.UpdateImpl();
    }

    public virtual void Possess(Character character)
    {
        _Character = character;
        _Character.OnPossessed(this);

        this.OnPossessed(_Character);
        _PlayerCamera.OnPossessed(character);
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