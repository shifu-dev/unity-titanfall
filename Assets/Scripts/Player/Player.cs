using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerHUD))]
public class Player : Controller
{
    protected override void Awake()
    {
        _CollectBehaviours();
        _DispatchCreateEvent();
    }

    protected override void Update()
    {
        _DispatchPreUpdateEvent();
        _DispatchUpdateEvent();
        _DispatchPostUpdateEvent();
    }

    protected override void OnPossess(Character character)
    {
        _DispatchPossessEvent();
    }

    //// -------------------------------------------------------------------------------------------
    //// Functions
    //// -------------------------------------------------------------------------------------------

    protected T _GetBehaviour<T>()
        where T : PlayerBehaviour
    {
        foreach (var behaviour in _behaviours)
        {
            T customBehaviour = behaviour as T;
            if (customBehaviour is not null)
            {
                return customBehaviour;
            }
        }

        return null;
    }

    protected void _CollectBehaviours()
    {
        _behaviours = GetComponents<PlayerBehaviour>();
        _playerInputs = _GetBehaviour<PlayerInputs>();
        _PlayerState = _GetBehaviour<PlayerState>();
        _playerCamera = _GetBehaviour<PlayerCamera>();
        _playerHUD = _GetBehaviour<PlayerHUD>();
    }

    protected void _DispatchCreateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerCreate(this);
        }
    }

    protected void _DispatchPreUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPreUpdate();
        }
    }

    protected void _DispatchUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerUpdate();
        }
    }

    protected void _DispatchPostUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPostUpdate();
        }
    }

    protected void _DispatchDestroyEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerDestroy();
        }
    }

    protected void _DispatchPossessEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPossess(_character);
        }
    }

    protected void _DispatchResetEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerReset();
        }
    }

    //// -------------------------------------------------------------------------------------------
    //// Properties and Fields
    //// -------------------------------------------------------------------------------------------

    public PlayerInputs playerInputs => _playerInputs;
    public PlayerState PlayerState => _PlayerState;
    public PlayerCamera playerCamera => _playerCamera;
    public PlayerInteraction playerInteraction => _playerInteraction;
    public PlayerHUD playerHUD => _playerHUD;

    protected PlayerBehaviour[] _behaviours;
    protected PlayerInputs _playerInputs;
    protected PlayerState _PlayerState;
    protected PlayerCamera _playerCamera;
    protected PlayerInteraction _playerInteraction;
    protected PlayerHUD _playerHUD;
}