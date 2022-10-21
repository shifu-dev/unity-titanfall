using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public partial class CharacterMovement : CharacterBehaviour
{
    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        mCollider = GetComponent<CapsuleCollider>();
        mVelocity = Vector3.zero;

        CharacterAsset source = _character.source;
        if (source is not null)
        {
            mCapsule = new VirtualCapsule();
            mCapsule.position = Vector3.zero;
            mCapsule.rotation = Quaternion.identity;
            mCapsule.height = 2f;
            mCapsule.radius = .5f;
            // mCapsule.layerMask = source.layerMask;
            mCapsule.queryTrigger = QueryTriggerInteraction.Ignore;

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

        mCapsule.position = transform.position;
        mCapsule.rotation = transform.rotation;
    }

    public override void OnCharacterUpdate()
    {
        mDeltaTime = Time.deltaTime;
        if (mDeltaTime <= 0)
        {
            return;
        }

        base.OnCharacterUpdate();

        UpdateModules();

        mLastPosition = mCapsule.position;
        RunModulePhysics();

        Vector3 moved = mCapsule.position - mLastPosition;
        mVelocity = moved / mDeltaTime;

        PostUpdateModules();
    }

    protected virtual void UpdateModules()
    {
        foreach (var module in mModules)
        {
            module.Update();
        }
    }

    protected virtual void RunModulePhysics()
    {
        mPreviousModule = mActiveModule;
        mActiveModule = null;
        foreach (var module in mModules)
        {
            if (module.ShouldRun())
            {
                mActiveModule = module;
                break;
            }
        }

        if (mPreviousModule != mActiveModule)
        {
            if (mPreviousModule is not null)
            {
                mPreviousModule.StopPhysics();
            }

            if (mActiveModule is not null)
            {
                mActiveModule.StartPhysics();
            }
        }

        if (mActiveModule is not null)
        {
            mActiveModule.RunPhysics();
        }
    }

    protected virtual void PostUpdateModules()
    {
        foreach (var module in mModules)
        {
            module.PostUpdate();
        }
    }

    protected CharacterMovementModule[] mModules;
    public IReadOnlyCollection<CharacterMovementModule> modules => mModules;

    protected CharacterMovementModule mActiveModule;
    protected CharacterMovementModule mPreviousModule;
    public CharacterMovementModule activeModule => mActiveModule;
    public CharacterMovementModule previousModule => mPreviousModule;

    protected CapsuleCollider mCollider;

    protected float mSkinWidth;
    protected VirtualCapsule mCapsule;
    public VirtualCapsule capsule => mCapsule;

    protected Vector3 mVelocity;
    protected Vector3 mLastPosition;
    public Vector3 velocity => mVelocity;

    protected float mDeltaTime = 0f;
}