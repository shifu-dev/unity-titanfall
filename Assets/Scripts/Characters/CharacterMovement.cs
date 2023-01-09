using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public partial class CharacterMovement : CharacterBehaviour
{
    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        _collider = GetComponent<CapsuleCollider>();
        _velocity = Vector3.zero;

        CharacterAsset source = _character.source;
        if (source is not null)
        {
            _capsule = new VirtualCapsule();
            _capsule.position = Vector3.zero;
            _capsule.rotation = Quaternion.identity;
            _capsule.height = 2f;
            _capsule.radius = .5f;
            // _capsule.layerMask = source.layerMask;
            _capsule.queryTrigger = QueryTriggerInteraction.Ignore;

            var moduleList = new List<CharacterMovementModule>();
            if (source.groundModuleSource is not null)
            {
                var groundModule = source.groundModuleSource.GetModule();
                if (groundModule is not null)
                {
                    moduleList.Add(groundModule);
                }
            }

            if (source.airModuleSource is not null)
            {
                var airModule = source.airModuleSource.GetModule();
                if (airModule is not null)
                {
                    moduleList.Add(airModule);
                }
            }
        }
    }

    public override void OnCharacterSpawn()
    {
        base.OnCharacterSpawn();

        _capsule.position = transform.position;
        _capsule.rotation = transform.rotation;
    }

    public override void OnCharacterUpdate()
    {
        _deltaTime = Time.deltaTime;
        if (_deltaTime <= 0)
        {
            return;
        }

        base.OnCharacterUpdate();

        UpdateModules();

        _lastPosition = _capsule.position;
        RunModulePhysics();

        Vector3 moved = _capsule.position - _lastPosition;
        _velocity = moved / _deltaTime;

        PostUpdateModules();
    }

    protected virtual void UpdateModules()
    {
        foreach (var module in _modules)
        {
            module.Update();
        }
    }

    protected virtual void RunModulePhysics()
    {
        _previousModule = _activeModule;
        _activeModule = null;
        foreach (var module in _modules)
        {
            if (module.ShouldRun())
            {
                _activeModule = module;
                break;
            }
        }

        if (_previousModule != _activeModule)
        {
            if (_previousModule is not null)
            {
                _previousModule.StopPhysics();
            }

            if (_activeModule is not null)
            {
                _activeModule.StartPhysics();
            }
        }

        if (_activeModule is not null)
        {
            _activeModule.RunPhysics();
        }
    }

    protected virtual void PostUpdateModules()
    {
        foreach (var module in _modules)
        {
            module.PostUpdate();
        }
    }

    protected CharacterMovementModule[] _modules;
    public IReadOnlyCollection<CharacterMovementModule> modules => _modules;

    protected CharacterMovementModule _activeModule;
    protected CharacterMovementModule _previousModule;
    public CharacterMovementModule activeModule => _activeModule;
    public CharacterMovementModule previousModule => _previousModule;

    protected CapsuleCollider _collider;

    protected float _skinWidth;
    protected VirtualCapsule _capsule;
    public VirtualCapsule capsule => _capsule;

    protected Vector3 _velocity;
    protected Vector3 _lastPosition;
    public Vector3 velocity => _velocity;

    protected float _deltaTime = 0f;
}