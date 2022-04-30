using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerHUD))]
public class Player : MonoBehaviour
{
    public PlayerInputs PlayerInputs { get; protected set; }
    public PlayerState PlayerState { get; protected set; }
    public PlayerCamera PlayerCamera { get; protected set; }
    public PlayerHUD PlayerHUD { get; protected set; }
    public Character Chararcter { get; protected set; }

    [ReadOnly]
    [SerializeField]
    protected Character _Character;

    void Awake()
    {
        PlayerInputs = GetComponent<PlayerInputs>();
        PlayerCamera = GetComponent<PlayerCamera>();
        PlayerState = GetComponent<PlayerState>();
        PlayerHUD = GetComponent<PlayerHUD>();

        PlayerInputs.Init(this);
        PlayerCamera.Init(this);
        PlayerState.Init(this);
        PlayerHUD.Init(this);
    }

    void Start()
    {
    }

    void Update()
    {
        PlayerInputs.UpdateImpl();
        PlayerCamera.UpdateImpl();
        PlayerState.UpdateImpl();
        PlayerHUD.UpdateImpl();
    }

    public virtual void Possess(Character character)
    {
        _Character = character;
        _Character.OnPossessed(this);

        this.OnPossessed(_Character);
        PlayerCamera.OnPossessed(character);
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