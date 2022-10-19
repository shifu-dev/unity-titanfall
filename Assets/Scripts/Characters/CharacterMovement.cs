﻿using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public partial class CharacterMovement : CharacterBehaviour
{
    public CharacterMovement()
    {
        mVelocity = Vector3.zero;
    }

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
            mCapsule.layerMask = source.layerMask;
            mCapsule.queryTrigger = QueryTriggerInteraction.Ignore;
            mSkinWidth = source.skinWidth;
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
    }

    protected virtual void UpdateModules()
    {
        foreach (var module in mModules)
        {
            if (module.ShouldUpdate())
            {
                module.Update();
                break;
            }
        }
    }

    public Vector3 Velocity => mVelocity;
    public VirtualCapsule capsule => mCapsule;

    protected CharacterMovementModule[] mModules;

    protected CapsuleCollider mCollider;
    protected VirtualCapsule mCapsule;
    protected float mSkinWidth;
    protected Vector3 mVelocity;
    protected float mDeltaTime = 0f;
}