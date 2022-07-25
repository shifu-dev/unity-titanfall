using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(CharacterInventory))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterInteraction))]
[RequireComponent(typeof(CharacterEquip))]
[RequireComponent(typeof(CharacterView))]
[RequireComponent(typeof(CharacterCapsule))]
[RequireComponent(typeof(CharacterAnimation))]
public class Character : Equipable
{
    private CharacterDataSource _source;
    public CharacterDataSource source => _source;

    //////////////////////////////////////////////////////////////////
    /// Behaviours | BEGIN

    protected CharacterBehaviour[] _behaviours;
    protected CharacterInitializer _charInitializer;
    protected CharacterBody _charBody;
    protected CharacterInventory _charInventory;
    protected CharacterMovement _charMovement;
    protected CharacterInteraction _charInteraction;
    protected CharacterEquip _charEquip;
    protected CharacterView _charView;
    protected CharacterCapsule _charCapsule;
    protected CharacterAnimation _charAnimation;

    public CharacterInitializer charInitializer => _charInitializer;
    public CharacterBody charBody => _charBody;
    public CharacterInventory charInventory => _charInventory;
    public CharacterMovement charMovement => _charMovement;
    public CharacterInteraction charInteraction => _charInteraction;
    public CharacterEquip charEquip => _charEquip;
    public CharacterView charView => _charView;
    public CharacterCapsule charCapsule => _charCapsule;
    public CharacterAnimation charAnimation => _charAnimation;

    /// Behaviours | END
    //////////////////////////////////////////////////////////////////

    // character mass
    protected float _mass;
    public float mass => _mass;
    public float scaledMass => transform.lossyScale.magnitude * _mass;

    // character directions
    public Quaternion rotation => transform.rotation;
    public Vector3 forward => transform.forward;
    public Vector3 back => -transform.forward;
    public Vector3 right => transform.right;
    public Vector3 left => -transform.right;
    public Vector3 up => transform.up;
    public Vector3 down => -transform.up;

    // controller controlling the character
    protected Controller _controller;
    public Controller controller => _controller;

    public Character()
    {
        _source = null;
        _behaviours = new CharacterBehaviour[0];
        _charInitializer = null;
        _charBody = null;
        _charInventory = null;
        _charMovement = null;
        _charInteraction = null;
        _charEquip = null;
        _charView = null;
        _charCapsule = null;
        _charAnimation = null;
        _controller = null;

        _mass = 0f;
    }

    //////////////////////////////////////////////////////////////////
    /// Events | BEGIN

    protected virtual void Awake()
    {
        CollectBehaviours();
        GetInitializer();

        OnInit();
        GenerateOnCharacterCreateEvents();

        if (_charInitializer != null && _charInitializer.destroyOnUse)
        {
            DestroyInitializer();
        }
    }

    protected virtual void Update()
    {
        GenerateOnCharacterPreUpdateEvents();
        GenerateOnCharacterUpdateEvents();
        GenerateOnCharacterPostUpdateEvents();
    }

    protected virtual void FixedUpdate()
    {
        GenerateOnCharacterFixedUpdateEvents();
    }

    protected virtual void OnInit()
    {
        if (_charInitializer != null)
        {
            _source = _charInitializer.source;
        }

        if (_source != null)
        {
            _mass = _source.characterMass;
        }
    }

    public virtual void Destroy()
    {
        GenerateOnCharacterDestroyEvents();
    }

    public virtual void Reset()
    {
        GenerateOnCharacterResetEvents();
    }

    public virtual void OnSpawn()
    {
        GenerateOnCharacterSpawnEvents();
    }

    public virtual void OnDespawn()
    {
        GenerateOnCharacterDespawnEvents();
    }

    public virtual void OnPossess(Controller controller)
    {
        _controller = controller;
        GenerateOnCharacterPossessEvents();
    }

    /// Events | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Behaviours | BEGIN

    public virtual T GetBehaviour<T>()
        where T : CharacterBehaviour
    {
        foreach (var behaviour in _behaviours)
        {
            T customBehaviour = behaviour as T;
            if (customBehaviour != null)
            {
                return customBehaviour;
            }
        }

        return null;
    }

    public virtual bool HasBehaviour<T>()
        where T : CharacterBehaviour
    {
        return GetBehaviour<T>() != null;
    }

    protected virtual void CollectBehaviours(params CharacterBehaviour[] exceptions)
    {
        List<CharacterBehaviour> behaviours = new List<CharacterBehaviour>();
        GetComponents<CharacterBehaviour>(behaviours);

        // no need to check for exceptional behaviours, if there are none
        if (exceptions.Length > 0)
        {
            behaviours.RemoveAll((CharacterBehaviour behaviour) =>
            {
                if (behaviour == null)
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
        _charBody = GetBehaviour<CharacterBody>();
        _charInventory = GetBehaviour<CharacterInventory>();
        _charMovement = GetBehaviour<CharacterMovement>();
        _charInteraction = GetBehaviour<CharacterInteraction>();
        _charEquip = GetBehaviour<CharacterEquip>();
        _charView = GetBehaviour<CharacterView>();
        _charCapsule = GetBehaviour<CharacterCapsule>();
        _charAnimation = GetBehaviour<CharacterAnimation>();
    }

    protected virtual void GetInitializer()
    {
        _charInitializer = GetComponent<CharacterInitializer>();
    }

    protected virtual void DestroyInitializer()
    {
        Destroy(_charInitializer);
    }

    protected virtual void GenerateOnCharacterCreateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterCreate(this, _charInitializer);
        }
    }

    protected virtual void GenerateOnCharacterSpawnEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterSpawn();
        }
    }

    protected virtual void GenerateOnCharacterPreUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterPreUpdate();
        }
    }

    protected virtual void GenerateOnCharacterUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterUpdate();
        }
    }

    protected virtual void GenerateOnCharacterPostUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterPostUpdate();
        }
    }

    protected virtual void GenerateOnCharacterFixedUpdateEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterFixedUpdate();
        }
    }

    protected virtual void GenerateOnCharacterDeadEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterDead();
        }
    }

    protected virtual void GenerateOnCharacterDespawnEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterDespawn();
        }
    }

    protected virtual void GenerateOnCharacterDestroyEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterDestroy();
        }
    }

    protected virtual void GenerateOnCharacterPossessEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterPossess(_controller);
        }
    }

    protected virtual void GenerateOnCharacterResetEvents()
    {
        foreach (var behaviour in _behaviours)
        {
            behaviour.OnCharacterReset();
        }
    }

    /// Behaviours | END
    //////////////////////////////////////////////////////////////////
}
