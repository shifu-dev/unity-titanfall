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
    protected PlayerBehaviour[] _behaviours;
    protected PlayerInputs _playerInputs;
    protected PlayerState _PlayerState;
    protected PlayerCamera _playerCamera;
    protected PlayerInteraction _playerInteraction;
    protected PlayerHUD _playerHUD;

    public PlayerInputs playerInputs => _playerInputs;
    public PlayerState PlayerState => _PlayerState;
    public PlayerCamera playerCamera => _playerCamera;
    public PlayerInteraction playerInteraction => _playerInteraction;
    public PlayerHUD playerHUD => _playerHUD;

    protected override void Awake()
    {
        CollectBehaviours();

        GenerateOnPlayerCreateEvents();
    }

    protected override void Update()
    {
        GenerateOnPlayerPreUpdateEvents();
        GenerateOnPlayerUpdateEvents();
        GenerateOnPlayerPostUpdateEvents();
    }

    protected override void OnPossess(Character character)
    {
        GenerateOnPlayerPossessEvents();
    }

    //////////////////////////////////////////////////////////////////
    /// Behaviours | BEGIN

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

    protected virtual void CollectBehaviours(params PlayerBehaviour[] exceptions)
    {
        List<PlayerBehaviour> behaviours = new List<PlayerBehaviour>();
        GetComponents<PlayerBehaviour>(behaviours);

        // no need to check for exceptional behaviours, if there are none
        if (exceptions.Length > 0)
        {
            behaviours.RemoveAll((PlayerBehaviour behaviour) =>
            {
                if (behaviour is null)
                    return true;

                foreach (var exceptionBehaviour in exceptions)
                {
                    if (behaviour == exceptionBehaviour)
                        return true;
                }

                return false;
            });
        }

        _behaviours = behaviours.ToArray();

        // cache behaviours
        _playerInputs = GetBehaviour<PlayerInputs>();
        _PlayerState = GetBehaviour<PlayerState>();
        _playerCamera = GetBehaviour<PlayerCamera>();
        _playerHUD = GetBehaviour<PlayerHUD>();
    }

    protected virtual void GenerateOnPlayerCreateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerCreate(this);
        }
    }

    protected virtual void GenerateOnPlayerPreUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPreUpdate();
        }
    }

    protected virtual void GenerateOnPlayerUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerUpdate();
        }
    }

    protected virtual void GenerateOnPlayerPostUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPostUpdate();
        }
    }

    protected virtual void GenerateOnPlayerDestroyEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerDestroy();
        }
    }

    protected virtual void GenerateOnPlayerPossessEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerPossess(_character);
        }
    }

    protected virtual void GenerateOnPlayerResetEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnPlayerReset();
        }
    }

    /// Behaviours | END
    //////////////////////////////////////////////////////////////////
}