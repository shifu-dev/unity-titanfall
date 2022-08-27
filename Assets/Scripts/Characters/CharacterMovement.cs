using System;
using UnityEngine;

public partial class CharacterMovement : CharacterBehaviour
{
    public CharacterMovement()
    {
        CreateMovementState();

        _groundPreviousResult = GroundResult.invalid;
        _groundResult = GroundResult.invalid;
        _velocity = Vector3.zero;
    }

    public override void OnCharacterCreate(Character character, CharacterInitializer initializer)
    {
        base.OnCharacterCreate(character, initializer);

        _source = _character.source;
        _charView = _character.charView;

        _collider = GetComponent<CapsuleCollider>();

        // cache Data from CharacterDataSource
        if (_source is not null)
        {
            _capsule = new VirtualCapsule();
            _capsule.position = Vector3.zero;
            _capsule.rotation = Quaternion.identity;
            _capsule.height = 2f;
            _capsule.radius = .5f;
            _capsule.layerMask = _source.layerMask;
            _capsule.queryTrigger = QueryTriggerInteraction.Ignore;
            _skinWidth = _source.skinWidth;

            _groundCheckDepth = _source.groundCheckDepth;
            _groundLayer = _source.groundLayer;
            _groundMinMoveDistance = _source.groundMinMoveDistance;

            _groundStandIdleSpeed = _source.groundStandIdleSpeed;
            _groundStandIdleAcceleration = _source.groundStandIdleAcceleration;
            _groundStandWalkSpeed = _source.groundStandWalkSpeed;
            _groundStandWalkAcceleration = _source.groundStandWalkAcceleration;
            _groundStandRunSpeed = _source.groundStandRunSpeed;
            _groundStandRunAcceleration = _source.groundStandRunAcceleration;
            _groundStandSprintSpeed = _source.groundStandSprintSpeed;
            _groundStandSprintAcceleration = _source.groundStandSprintAcceleration;
            _groundStandSprintLeftAngleMax = _source.groundStandSprintLeftAngleMax;
            _groundStandSprintRightAngleMax = _source.groundStandSprintRightAngleMax;
            _groundStandJumpForce = _source.groundStandJumpForce;
            _groundStandStepUpPercent = _source.groundStandStepUpPercent;
            _groundStandStepDownPercent = _source.groundStandStepDownPercent;
            _groundStandSlopeUpAngle = _source.groundStandSlopeUpAngle;
            _groundStandSlopeDownAngle = _source.groundStandSlopeDownAngle;
            _groundStandMaintainVelocityOnSurface = _source.groundStandMaintainVelocityOnSurface;
            _groundStandMaintainVelocityAlongSurface = _source.groundStandMaintainVelocityAlongSurface;
            _groundStandCapsuleCenter = _source.groundStandCapsuleCenter;
            _groundStandCapsuleHeight = _source.groundStandCapsuleHeight;
            _groundStandCapsuleRadius = _source.groundStandCapsuleRadius;
            _groundStandToCrouchTransitionSpeed = _source.groundStandToCrouchTransitionSpeed;

            _groundCrouchIdleSpeed = _source.groundCrouchIdleSpeed;
            _groundCrouchIdleAcceleration = _source.groundCrouchIdleAcceleration;
            _groundCrouchWalkSpeed = _source.groundCrouchWalkSpeed;
            _groundCrouchWalkAcceleration = _source.groundCrouchWalkAcceleration;
            _groundCrouchRunSpeed = _source.groundCrouchRunSpeed;
            _groundCrouchRunAcceleration = _source.groundCrouchRunAcceleration;
            _groundCrouchAutoRiseToStandSprint = _source.groundCrouchAutoRiseToStandSprint;
            _groundCrouchJumpForce = _source.groundCrouchJumpForce;
            _groundCrouchStepUpPercent = _source.groundCrouchStepUpPercent;
            _groundCrouchStepDownPercent = _source.groundCrouchStepDownPercent;
            _groundCrouchSlopeUpAngle = _source.groundCrouchSlopeUpAngle;
            _groundCrouchSlopeDownAngle = _source.groundCrouchSlopeDownAngle;
            _groundCrouchMaintainVelocityOnSurface = _source.groundCrouchMaintainVelocityOnSurface;
            _groundCrouchMaintainVelocityAlongSurface = _source.groundCrouchMaintainVelocityAlongSurface;
            _groundCrouchCapsuleCenter = _source.groundCrouchCapsuleCenter;
            _groundCrouchCapsuleHeight = _source.groundCrouchCapsuleHeight;
            _groundCrouchCapsuleRadius = _source.groundCrouchCapsuleRadius;
            _groundCrouchToStandTransitionSpeed = _source.groundCrouchToStandTransitionSpeed;

            _airGravityAcceleration = _source.airGravityAcceleration;
            _airGravityMaxSpeed = _source.airGravityMaxSpeed;
            _airMinMoveDistance = _source.airMinMoveDistance;
            _airMoveSpeed = _source.airMoveSpeed;
            _airMoveAcceleration = _source.airMoveAcceleration;
            _airJumpPower = _source.airJumpPower;
            _airMaxJumpCount = _source.airMaxJumpCount;
        }

        _velocity = Vector3.zero;
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

        UpdatePhysicsData();
        UpdateMovementState();
        UpdatePhysicsState();

        FlushCapsuleMove();
    }

    public override void OnCharacterPostUpdate()
    {
        base.OnCharacterPostUpdate();

        ConsumeInputs();
    }

    //////////////////////////////////////////////////////////////////
    /// Controls | BEGIN

    public virtual void SetMoveVector(Vector2 move)
    {
        _move = move;
    }

    public virtual void StartWalk()
    {
        _walk = true;
    }

    public virtual void StartSprint()
    {
        _sprint = true;
    }

    public virtual void StartCrouch()
    {
        _crouch = true;
    }

    public virtual void StartProne()
    {
        _prone = true;
    }

    public virtual void StartJump()
    {
        _jump = true;
    }

    protected virtual void ConsumeInputs()
    {
        _move = Vector2.zero;
        _walk = false;
        _sprint = false;
        _crouch = false;
        _prone = false;
        _jump = false;
    }

    /// Controls | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Movement State | BEGIN

    protected virtual void CreateMovementState()
    {
        MovementState = new CharacterMovementStateImpl(CharacterMovementState.NONE);
    }

    protected virtual void UpdateMovementState()
    {
        float _moveAngle = Vector2.SignedAngle(_move, Vector2.up);

        CharacterMovementState newState = new CharacterMovementStateImpl();

        if (_canGround && _groundResult.isValid)
        {
            if (_crouch)
            {
                if (_move.magnitude == 0)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_IDLE;
                }
                else if (_walk)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_WALK;
                }
                else if (_sprint && _moveAngle > _groundStandSprintLeftAngleMax
                                                      && _moveAngle < _groundStandSprintRightAngleMax)
                {
                    newState.current = CharacterMovementState.GROUND_STAND_SPRINT;
                }
                else
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_RUN;
                }
            }
            else    // Standing
            {
                if (_move.magnitude == 0)
                {
                    if (_jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE;
                    }
                }
                else if (_walk)
                {
                    if (_jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK;
                    }
                }
                else if (_sprint && _moveAngle > _groundStandSprintLeftAngleMax
                                                      && _moveAngle < _groundStandSprintRightAngleMax)
                {
                    if (_jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_SPRINT_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_SPRINT;
                    }
                }
                else
                {
                    if (_jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_RUN_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_RUN;
                    }
                }
            }
        }
        else
        {
            newState.current = CharacterMovementState.AIR_IDLE;
        }

        MovementState = newState;
    }

    protected virtual bool SetMovementState(uint state)
    {
        if (state == MovementState.current)
            return false;

        if (CanEnterMovementState(state))
        {
            MovementState.current = state;
            OnMovementStateUpdated();

            return true;
        }

        return false;
    }

    protected virtual bool CanEnterMovementState(uint state)
    {
        return true;
    }

    protected virtual void OnMovementStateUpdated()
    {
    }

    /// Movement State | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Physics | BEGIN

    protected virtual void UpdatePhysicsData()
    {
        if (_groundResult.isValid)
        {
            var deltaPosition = _groundResult.collider.transform.position - _groundResult.basePosition;
            // var deltaRotation = m_groundResult.collider.transform.rotation.eulerAngles - m_groundResult.baseRotation.eulerAngles;

            _capsule.position += deltaPosition;
            // charCapsule.localRotation = Quaternion.Euler(charCapsule.localRotation.eulerAngles + deltaRotation);
        }

        CapsuleResolvePenetration();
        GroundUpdateResult();
    }

    protected virtual void UpdatePhysicsState()
    {
        if (MovementState.isGrounded)
        {
            PhysGround();
        }
        else if (MovementState.isAir)
        {
            PhysAir();
        }
    }

    protected virtual void SetVelocityByMove(Vector3 moved)
    {
        moved = moved / Time.deltaTime;

        _velocity = moved;
    }

    /// Physics | END
    //////////////////////////////////////////////////////////////////

    public Vector3 Velocity => _velocity;

    protected CharacterAsset _source;
    protected CharacterView _charView;

    private CharacterMovementStateImpl _movementState;
    public virtual CharacterMovementState MovementState
    {
        get => _movementState;
        protected set => _movementState = new CharacterMovementStateImpl(value.current);
    }

    protected float _movementStateWeight;

    protected Vector2 _move;
    protected bool _walk;
    protected bool _sprint;
    protected bool _crouch;
    protected bool _prone;
    protected bool _jump;

    protected Vector3 _velocity;
    protected float _deltaTime = 0f;
    protected Vector3 _charUp = Vector3.up;
    protected Vector3 _charRight = Vector3.right;
    protected Vector3 _charForward = Vector3.forward;
    protected float _currentMinMoveDist = 0;
    protected float _currentMoveSpeed = 0;
    protected float _currentMoveAccel = 0;
    protected float _currentJumpPower = 0;
    protected float _currentStepUpHeight = 0;
    protected float _currentStepDownDepth = 0;
    protected float _currentSlopeUpAngle = 0;
    protected float _currentSlopeDownAngle = 0;
    protected uint _currentJumpCount = 0;
    protected uint _currentMaxJumpCount = 0;
    protected bool _currentMaintainVelocityOnJump = false;
    protected bool _currentMaintainVelocityOnSurface = true;
    protected bool _currentMaintainVelocityAlongSurface = true;
}