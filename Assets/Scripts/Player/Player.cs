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

        _DispatchPlayerCreateEvent();
    }

    protected override void Update()
    {
        _DispatchPlayerPreUpdateEvent();
        _DispatchPlayerUpdateEvent();
        _DispatchPlayerPostUpdateEvent();
    }

    protected override void OnPossess(Character character)
    {
        _DispatchPlayerPossessEvent();
    }

    //// -------------------------------------------------------------------------------------------
    //// Behaviours Begin
    //// -------------------------------------------------------------------------------------------

    public virtual T GetBehaviour<T>()
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

    public virtual bool HasBehaviour<T>()
        where T : PlayerBehaviour
    {
        return GetBehaviour<T>() is not null;
    }

    protected void _CollectBehaviours(params PlayerBehaviour[] filter)
    {
        List<PlayerBehaviour> behaviours = new List<PlayerBehaviour>();
        GetComponents<PlayerBehaviour>(behaviours);

        // no need to check for filter behaviours, if there are none
        if (filter.Length > 0)
        {
            behaviours.RemoveAll((PlayerBehaviour behaviour) =>
            {
                if (behaviour is null)
                    return true;

                foreach (var filterBehaviour in filter)
                {
                    if (behaviour == filterBehaviour)
                        return true;
                }

                return false;
            });
        }

        _behaviours = behaviours.ToArray();
        _playerInputs = GetBehaviour<PlayerInputs>();
        _PlayerState = GetBehaviour<PlayerState>();
        _playerCamera = GetBehaviour<PlayerCamera>();
        _playerHUD = GetBehaviour<PlayerHUD>();
    }

    protected void _DispatchPlayerCreateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerCreate(this);
        }
    }

    protected void _DispatchPlayerPreUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPreUpdate();
        }
    }

    protected void _DispatchPlayerUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerUpdate();
        }
    }

    protected void _DispatchPlayerPostUpdateEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPostUpdate();
        }
    }

    protected void _DispatchPlayerDestroyEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerDestroy();
        }
    }

    protected void _DispatchPlayerPossessEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPossess(_character);
        }
    }

    protected void _DispatchPlayerResetEvent()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerReset();
        }
    }

    //// -------------------------------------------------------------------------------------------
    //// Behaviours End
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