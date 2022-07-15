using System;
using UnityEngine;

public class CharacterMovement : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Constants | BEGIN

    protected const uint k_maxGroundMoveIterations = 10;
    protected const uint k_maxAirMoveIterations = 10;

    protected const float k_collisionOffset = .001f;
    protected const float k_recalculateNormalFallback = .01f;
    protected const float k_recalculateNormalAddon = .001f;

    protected const float k_minMoveAlongSurfaceForMaintainVelocity = .0001f;
    protected const float k_minGroundSlopeUpAngle = 0f;
    protected const float k_maxGroundSlopeUpAngle = 89.9f;

    protected const float k_gravityMultiplier = .05f;

    /// Constants | END
    //////////////////////////////////////////////////////////////////

    protected CharacterDataSource _source;
    protected CharacterCapsule _charCapsule;
    protected CharacterInputs _charInputs;
    protected CharacterView _charView;

    [Space]
    [SerializeField, ReadOnly] private CharacterMovementStateImpl _movementState;
    public virtual CharacterMovementState movementState
    {
        get => _movementState;
        protected set => _movementState = new CharacterMovementStateImpl(value.current);
    }

    [SerializeField, ReadOnly] protected float _movementStateWeight;

    //////////////////////////////////////////////////////////////////
    /// Current Values | BEGIN

    protected Vector3 _velocity;
    protected Vector3 _char_up = Vector3.up;
    protected Vector3 _char_right = Vector3.right;
    protected Vector3 _char_forward = Vector3.forward;
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
    protected bool _canGround = true;

    /// Current Values | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Data | BEGIN

    protected CharacterMovementGroundResult _previousGroundResult;
    protected CharacterMovementGroundResult _groundResult;

    protected LayerMask _groundLayer;

    protected float _groundStandWalkSpeed;
    protected float _groundStandWalkAcceleration;
    protected float _groundStandRunSpeed;
    protected float _groundStandRunAcceleration;
    protected float _groundStandSprintSpeed;
    protected float _groundStandSprintAcceleration;
    protected float _groundStandSprintLeftAngleMax;
    protected float _groundStandSprintRightAngleMax;
    protected float _groundStandJumpForce;
    protected float _groundStandStepUpPercent;
    protected float _groundStandStepUpHeight => _charCapsule.height * _groundStandStepUpPercent / 100;
    protected float _groundStandStepDownPercent;
    protected float _groundStandStepDownDepth => _charCapsule.height * _groundStandStepDownPercent / 100;
    protected float _groundStandSlopeUpAngle;
    protected float _groundStandSlopeDownAngle;
    protected float _groundStandCapsuleHeight;
    protected float _groundStandCapsuleRadius;
    protected float _groundStandToCrouchTransitionSpeed;
    protected float _groundCrouchWalkSpeed;
    protected float _groundCrouchWalkAcceleration;
    protected float _groundCrouchRunSpeed;
    protected float _groundCrouchRunAcceleration;
    protected float _groundCrouchJumpForce;
    protected float _groundCrouchStepUpPercent;
    protected float _groundCrouchStepUpHeight => _charCapsule.height * _groundCrouchStepUpPercent / 100;
    protected float _groundCrouchStepDownPercent;
    protected float _groundCrouchStepDownDepth => _charCapsule.height * _groundCrouchStepUpPercent / 100;
    protected float _groundCrouchSlopeUpAngle;
    protected float _groundCrouchSlopeDownAngle;
    protected float _groundCrouchCapsuleHeight;
    protected float _groundCrouchCapsuleRadius;
    protected float _groundCrouchToStandTransitionSpeed;
    protected float _groundMinMoveDistance;
    protected float _groundCheckDepth;
    protected float _groundStandIdleAcceleration;
    protected float _groundStandIdleSpeed;
    protected float _groundCrouchIdleSpeed;
    protected float _groundCrouchIdleAcceleration;

    protected bool _groundStandMaintainVelocityOnSurface;
    protected bool _groundStandMaintainVelocityAlongSurface;
    protected bool _groundCrouchAutoRiseToStandSprint;
    protected bool _groundCrouchMaintainVelocityOnSurface;
    protected bool _groundCrouchMaintainVelocityAlongSurface;

    protected Vector3 _groundStandCapsuleCenter;
    protected Vector3 _groundCrouchCapsuleCenter;

    /// Ground Data | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Air Data | BEGIN

    protected float _airGravityAcceleration;
    protected float _airGravityMaxSpeed;
    protected float _airMinMoveDistance;
    protected float _airMoveSpeed;
    protected float _airMoveAcceleration;
    protected float _airJumpPower;

    protected uint _airMaxJumpCount;

    /// Air Data | END
    //////////////////////////////////////////////////////////////////

    public CharacterMovement()
    {
        CreateMovementState();

        _previousGroundResult = CharacterMovementGroundResult.invalid;
        _groundResult = CharacterMovementGroundResult.invalid;
        _velocity = Vector3.zero;
    }

    //////////////////////////////////////////////////////////////////
    // Events | BEGIN

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        _source = character.charDataSource;
        _charInputs = character.charInputs;
        _charCapsule = character.charCapsule;
        _charView = character.charView;

        // Cache Data from CharacterDataSource
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

        _velocity = Vector3.zero;
    }

    public override void OnUpdateCharacter()
    {
        base.OnUpdateCharacter();

        UpdatePhysicsData();
        UpdateMovementState();
        UpdatePhysicsState();
    }

    public override void OnPossessed(Player player)
    {
    }

    /// Events | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Movement State | BEGIN

    protected virtual void CreateMovementState()
    {
        movementState = new CharacterMovementStateImpl(CharacterMovementState.NONE);
    }

    protected virtual void UpdateMovementState()
    {
        CharacterMovementState newState = new CharacterMovementStateImpl();

        if (_canGround && _groundResult.isValid)
        {
            if (_charInputs.crouch)
            {
                if (_charInputs.move.magnitude == 0)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_IDLE;
                }
                else if (_charInputs.walk)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_WALK;
                }
                else if (_charInputs.sprint && _charInputs.moveAngle > _groundStandSprintLeftAngleMax
                                                      && _charInputs.moveAngle < _groundStandSprintRightAngleMax)
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
                if (_charInputs.move.magnitude == 0)
                {
                    if (_charInputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE;
                    }
                }
                else if (_charInputs.walk)
                {
                    if (_charInputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK;
                    }
                }
                else if (_charInputs.sprint && _charInputs.moveAngle > _groundStandSprintLeftAngleMax
                                                      && _charInputs.moveAngle < _groundStandSprintRightAngleMax)
                {
                    if (_charInputs.jump)
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
                    if (_charInputs.jump)
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

        movementState = newState;
    }

    protected virtual bool SetMovementState(uint state)
    {
        if (state == movementState.current)
            return false;

        if (CanEnterMovementState(state))
        {
            movementState.current = state;
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

            _charCapsule.localPosition += deltaPosition;
            // charCapsule.localRotation = Quaternion.Euler(charCapsule.localRotation.eulerAngles + deltaRotation);
        }

        CapsuleResolvePenetration();
        GroundUpdateResult();
    }

    protected virtual void UpdatePhysicsState()
    {
        if (movementState.isGrounded)
        {
            PhysGround();
        }
        else if (movementState.isAir)
        {
            PhysAir();
        }
    }

    /// Physics | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Capsule API | BEGIN

    protected bool CapsuleCast(Vector3 move, out RaycastHit hit)
    {
        bool result = CapsuleCast(move, out RaycastHit smallHit, out RaycastHit bigHit);
        hit = smallHit.collider ? smallHit : bigHit;

        return result;
    }

    protected bool CapsuleCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        bigHit = _charCapsule.BigCapsuleCast(move);
        if (_charCapsule.skinWidth > 0f)
        {
            if (bigHit.collider)
            {
                move = move.normalized * bigHit.distance;
            }
        }
        else
        {
            smallHit = bigHit;
        }

        smallHit = _charCapsule.SmallCapsuleCast(move);

        return smallHit.collider || bigHit.collider;
    }

    protected bool BaseSphereCast(Vector3 move, out RaycastHit hit)
    {
        bool result = BaseSphereCast(move, out RaycastHit smallHit, out RaycastHit bigHit);
        hit = smallHit.collider ? smallHit : bigHit;

        return result;
    }

    protected bool BaseSphereCast(Vector3 move, out RaycastHit hit, out Vector3 hitNormal)
    {
        bool didHit = BaseSphereCast(move, out hit);
        RecalculateNormal(hit, move.normalized, out hitNormal);

        return didHit;
    }

    protected bool BaseSphereCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        bigHit = _charCapsule.BigBaseSphereCast(move);
        if (_charCapsule.skinWidth > 0f)
        {
            if (bigHit.collider)
            {
                move = move.normalized * bigHit.distance;
            }

            smallHit = _charCapsule.SmallBaseSphereCast(move);
        }
        else
        {
            smallHit = bigHit;
        }

        return smallHit.collider || bigHit.collider;
    }

    protected Vector3 CapsuleMove(Vector3 move)
    {
        return CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit);
    }

    protected Vector3 CapsuleMove(Vector3 move, out RaycastHit hit)
    {
        Vector3 moved = CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit);
        hit = smallHit.collider ? smallHit : bigHit;

        return moved;
    }

    protected Vector3 CapsuleMove(Vector3 move, out RaycastHit hit, out Vector3 hitNormal)
    {
        var moved = CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit, out hitNormal);
        hit = smallHit.collider ? smallHit : bigHit;

        return moved;
    }

    protected Vector3 CapsuleMove(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        if (move.magnitude == 0f)
        {
            smallHit = new RaycastHit();
            bigHit = new RaycastHit();

            CapsuleResolvePenetration();

            return Vector3.zero;
        }

        CapsuleCast(move, out smallHit, out bigHit);

        Vector3 direction = move.normalized;
        float distance = move.magnitude;

        if (smallHit.collider)
        {
            distance = smallHit.distance - _charCapsule.skinWidth - k_collisionOffset;
        }
        else if (bigHit.collider)
        {
            distance = bigHit.distance - k_collisionOffset;
        }

        _charCapsule.localPosition += direction * distance;

        CapsuleResolvePenetration();

        return direction * Math.Max(0f, distance);
    }

    protected Vector3 CapsuleMove(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit, out Vector3 hitNormal)
    {
        Vector3 moved = CapsuleMove(move, out smallHit, out bigHit);
        RaycastHit hit = smallHit.collider ? smallHit : bigHit;

        RecalculateNormal(hit, move.normalized, out hitNormal);

        return moved;
    }

    protected Vector3 CapsuleResolvePenetration()
    {
        return _charCapsule.ResolvePenetrationForBigCapsule(k_collisionOffset);
    }

    protected void TeleportTo(Vector3 pos, Quaternion rot)
    {
        _charCapsule.position = pos;
        _charCapsule.rotation = rot;
    }

    protected bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
    {
        return hit.RecalculateNormalUsingRaycast(out normal,
            _charCapsule.layerMask, _charCapsule.triggerQuery);
    }

    protected bool RecalculateNormal(RaycastHit hit, Vector3 direction, out Vector3 normal)
    {
        if (hit.collider && direction != Vector3.zero)
        {
            Vector3 origin = hit.point + (-direction * k_recalculateNormalFallback);
            const float rayDistance = k_recalculateNormalFallback + k_recalculateNormalAddon;

            Physics.Raycast(origin, direction, out RaycastHit rayHit,
                rayDistance, _charCapsule.layerMask, _charCapsule.triggerQuery);

            if (rayHit.collider && rayHit.collider == hit.collider)
            {
                normal = rayHit.normal;
                return true;
            }
        }

        if (RecalculateNormal(hit, out normal))
        {
            return true;
        }

        normal = Vector3.zero;
        return false;
    }

    protected bool RecalculateNormalIfZero(RaycastHit hit, ref Vector3 normal)
    {
        if (normal == Vector3.zero)
        {
            return RecalculateNormal(hit, out normal);
        }

        return true;
    }

    /// Capsule API | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Physics | BEGIN

    protected virtual void PhysGround()
    {
        GroundCalculateValues();

        Vector3 move_input_raw = _charInputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0, move_input_raw.y);
        move_input = Quaternion.Euler(0, _charView.turnAngle, 0) * move_input.normalized;
        move_input = character.rotation * move_input;

        _velocity = Vector3.ProjectOnPlane(_velocity, _char_up);

        Vector3 move = move_input * _currentMoveSpeed * delta_time;
        move = Vector3.MoveTowards(_velocity * delta_time, move, _currentMoveAccel * delta_time);

        move += _char_up * _currentJumpPower * delta_time;

        GroundMove(move);
    }

    protected virtual void GroundCalculateValues()
    {
        _char_up = character.up;
        _char_right = character.right;
        _char_forward = character.forward;
        _currentJumpCount = 0;
        _currentMinMoveDist = _groundMinMoveDistance;

        if (movementState.isGroundStanding)
        {
            _currentMaintainVelocityOnSurface = _groundStandMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = _groundStandMaintainVelocityAlongSurface;
        }
        else if (movementState.isGroundCrouching)
        {
            _currentMaintainVelocityOnSurface = _groundCrouchMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = _groundCrouchMaintainVelocityAlongSurface;
        }

        switch (movementState.current)
        {
            case CharacterMovementState.GROUND_STAND_IDLE:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandIdleAcceleration;
                _currentMoveSpeed = _groundStandIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandIdleAcceleration;
                _currentMoveSpeed = _groundStandIdleSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandWalkAcceleration;
                _currentMoveSpeed = _groundStandWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_WALK_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandWalkAcceleration;
                _currentMoveSpeed = _groundStandWalkSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;


            case CharacterMovementState.GROUND_STAND_RUN:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandRunAcceleration;
                _currentMoveSpeed = _groundStandRunSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_RUN_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandRunAcceleration;
                _currentMoveSpeed = _groundStandRunSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandSprintAcceleration;
                _currentMoveSpeed = _groundStandSprintSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandSprintAcceleration;
                _currentMoveSpeed = _groundStandSprintSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchIdleAcceleration;
                _currentMoveSpeed = _groundCrouchIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchWalkAcceleration;
                _currentMoveSpeed = _groundCrouchWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchRunAcceleration;
                _currentMoveSpeed = _groundCrouchRunSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_SLIDE:
            case CharacterMovementState.GROUND_ROLL:
            default:
                _currentStepDownDepth = 0f;
                _currentStepUpHeight = 0f;
                _currentSlopeUpAngle = 0f;
                _currentSlopeDownAngle = 0f;
                _currentMoveAccel = 0f;
                _currentMoveSpeed = 0f;
                _currentJumpPower = 0f;
                break;
        }
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 last_pos = _charCapsule.localPosition;

        GroundResizeCapsule();

        Vector3 move_h = Vector3.ProjectOnPlane(originalMove, _char_up);
        Vector3 move_v = originalMove - move_h;
        float move_v_mag = move_v.magnitude;

        Vector3 move_rem = move_h;

        // perform the vertical move (usually jump)
        if (move_v_mag > 0f)
        {
            CapsuleMove(move_v);
            _canGround = false;
        }

        if (move_rem.magnitude > _currentMinMoveDist)
        {
            var stepUpHeight = 0f;
            var canStepUp = move_v_mag == 0f;
            var didStepUp = false;
            var didStepUpRecover = false;
            var positionBeforeStepUp = Vector3.zero;
            var moveBeforeStepUp = Vector3.zero;

            CapsuleResolvePenetration();

            for (uint it = 0; it < k_maxGroundMoveIterations; it++)
            {
                move_rem -= CapsuleMove(move_rem, out RaycastHit moveHit, out Vector3 moveHitNormal);

                // perform step up recover
                if (didStepUp && !didStepUpRecover)
                {
                    didStepUp = false;
                    didStepUpRecover = true;

                    CapsuleMove(character.down * stepUpHeight, out RaycastHit stepUpRecoverHit, out Vector3 stepUpRecoverHitNormal);

                    if (stepUpRecoverHit.collider)
                    {
                        // if we cannot step on this _ground, revert the step up
                        // and continue the loop without stepping up this time
                        if (GroundCanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal, out float baseAngle) == false)
                        {
                            if (baseAngle < 90f)
                            {
                                _charCapsule.localPosition = positionBeforeStepUp;
                                move_rem = moveBeforeStepUp;
                                canStepUp = false;

                                continue;
                            }
                        }
                    }
                }

                // if there is no collision (no obstacle or remainingMove == 0)
                // break the loop
                if (moveHit.collider == null)
                {
                    break;
                }

                // try sliding on the obstacle
                if (GroundSlideOnSurface(originalMove, ref move_rem, moveHit, moveHitNormal))
                {
                    continue;
                }

                // step up the first time, we hit an obstacle
                if (canStepUp && didStepUp == false)
                {
                    canStepUp = false;
                    didStepUp = true;
                    didStepUpRecover = false;
                    positionBeforeStepUp = _charCapsule.localPosition;
                    moveBeforeStepUp = move_rem;

                    stepUpHeight = CapsuleMove(_char_up * _currentStepUpHeight).magnitude;

                    continue;
                }

                // try sliding along the obstacle
                if (GroundSlideAlongSurface(originalMove, ref move_rem, moveHit, moveHitNormal))
                {
                    continue;
                }

                // there's nothing we can do now, so stop the move
                move_rem = Vector3.zero;
            }
        }

        GroundStepDown(originalMove);
        CapsuleResolvePenetration();

        _velocity = _charCapsule.localPosition - last_pos;

        if (delta_time != 0f)
        {
            _velocity = _velocity / delta_time;
        }

        if (move_v_mag == 0f)
        {
            _velocity = Vector3.ProjectOnPlane(_velocity, _char_up);
        }
    }

    protected virtual void GroundResizeCapsule()
    {
        float weight = _movementStateWeight;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (movementState.isGroundCrouching)
        {
            targetCenter = _groundCrouchCapsuleCenter;
            targetHeight = _groundCrouchCapsuleHeight;
            targetRadius = _groundCrouchCapsuleRadius;
            speed = _groundStandToCrouchTransitionSpeed * delta_time;
        }
        else
        {
            targetCenter = _groundStandCapsuleCenter;
            targetHeight = _groundStandCapsuleHeight;
            targetRadius = _groundStandCapsuleRadius;
            speed = _groundCrouchToStandTransitionSpeed * delta_time;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        _charCapsule.localCenter = Vector3.Lerp(_charCapsule.localCenter, targetCenter, speed);
        _charCapsule.localHeight = Mathf.Lerp(_charCapsule.localHeight, targetHeight, speed);
        _charCapsule.localRadius = Mathf.Lerp(_charCapsule.localRadius, targetRadius, speed);

        weight = Mathf.Lerp(weight, 1f, speed);
        _movementStateWeight = weight;
    }

    protected virtual bool GroundSlideOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (remainingMove == Vector3.zero)
            return false;

        if (GroundCanStandOn(hit, hitNormal, out float slope_angle))
        {
            if (slope_angle == 0f)
            {
                return false;
            }

            Plane plane = new Plane(hitNormal, hit.point);
            Ray ray = new Ray(hit.point + remainingMove, _char_up);
            plane.Raycast(ray, out float enter);

            Vector3 slope_move = remainingMove + (_char_up * enter);

            if (_currentMaintainVelocityOnSurface == false)
            {
                slope_move = slope_move.normalized * remainingMove.magnitude;
            }

            remainingMove = slope_move;
            return true;
        }

        return false;
    }

    protected virtual bool GroundSlideAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        float remainingMoveSize = remainingMove.magnitude;

        if (hit.collider == null || remainingMoveSize == 0f)
            return false;

        RecalculateNormalIfZero(hit, ref hitNormal);

        Vector3 hitProject = Vector3.ProjectOnPlane(hitNormal, _char_up);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMoveSize, hitProject);
        if (_currentMaintainVelocityAlongSurface)
        {
            // to avoid sliding along perpendicular surface for very small values,
            // may be a result of small miscalculations
            if (slideMove.magnitude > k_minMoveAlongSurfaceForMaintainVelocity)
            {
                slideMove = slideMove.normalized * remainingMoveSize;
            }
        }

        remainingMove = slideMove;
        return true;
    }

    protected virtual bool GroundStepDown(Vector3 originalMove)
    {
        var verticalMove = Vector3.Project(originalMove, _char_up).magnitude;
        if (verticalMove != 0f || _currentStepDownDepth <= 0)
            return false;

        CapsuleResolvePenetration();

        var moved = CapsuleMove(character.down * _currentStepDownDepth, out RaycastHit hit, out Vector3 hitNormal);

        if (GroundCanStandOn(hit, hitNormal) == false)
        {
            _charCapsule.localPosition -= moved;
            return false;
        }

        return true;
    }

    protected virtual bool GroundCheck(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        return _groundLayer.Contains(collider.gameObject.layer);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit)
    {
        return GroundCanStandOn(hit, Vector3.zero);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit, Vector3 slopeNormal)
    {
        return GroundCanStandOn(hit, slopeNormal, out float slopeAngle);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        slopeAngle = 0f;

        if (hit.collider)
        {
            if (GroundCheck(hit.collider) == false)
            {
                return false;
            }

            RecalculateNormalIfZero(hit, ref slopeNormal);

            float maxSlopeAngle = Math.Clamp(_currentSlopeUpAngle,
                k_minGroundSlopeUpAngle, k_maxGroundSlopeUpAngle);

            slopeAngle = Vector3.Angle(_char_up, slopeNormal);

            if (FloatExtensions.IsInRange(slopeAngle, k_minGroundSlopeUpAngle, maxSlopeAngle))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool GroundCast(float depth, out CharacterMovementGroundResult result)
    {
        BaseSphereCast(character.down * depth, out RaycastHit hit, out Vector3 hitNormal);
        if (hitNormal == Vector3.zero)
        {
            hitNormal = hit.normal;
        }

        result = new CharacterMovementGroundResult();

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle) == false)
        {
            if (slopeAngle < 90f)
            {
                return false;
            }
        }

        result.collider = hit.collider;
        result.direction = character.down;
        result.distance = hit.distance;

        result.angle = slopeAngle;

        result.basePosition = result.collider.transform.position;
        result.baseRotation = result.collider.transform.rotation;

        result.edgeDistance = default;

        return true;
    }

    protected virtual void GroundUpdateResult()
    {
        _previousGroundResult = _groundResult;
        GroundCast(_groundCheckDepth, out _groundResult);
    }

    /// Ground Physics | END
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Air Physics | BEGIN

    protected virtual void PhysAir()
    {
        AirCalculateValues();

        Vector3 char_up = _char_up;
        Vector3 char_forward = character.forward;
        Vector3 char_right = character.right;
        float mass = character.mass;
        float gravity_speed = _airGravityAcceleration * mass * delta_time * delta_time * k_gravityMultiplier;

        Vector3 velocity = _velocity * delta_time;
        Vector3 velocity_h = Vector3.ProjectOnPlane(velocity, char_up);
        Vector3 velocity_v = velocity - velocity_h;

        Vector3 move_v = velocity_v + (char_up * gravity_speed);
        Vector3 move_h = velocity_h;

        Vector3 move_h_x = Vector3.ProjectOnPlane(move_h, char_forward);
        Vector3 move_h_z = move_h - move_h_x;
        // processed move input
        Vector3 move_input_raw = _charInputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0f, move_input_raw.y);
        move_input = Quaternion.Euler(0f, _charView.turnAngle, 0f) * move_input;
        move_input = character.rotation * move_input;

        // helping movement in _air
        Vector3 move_help_h = _currentMoveSpeed * move_input * delta_time;
        Vector3 move_help_h_x = Vector3.ProjectOnPlane(move_help_h, char_forward);
        Vector3 move_help_h_z = move_help_h - move_help_h_x;

        if (move_help_h_x.magnitude > 0f)
        {
            if (move_h_x.normalized == move_help_h_x.normalized)
            {
                if (move_help_h_x.magnitude > move_h_x.magnitude)
                {
                    move_h_x = move_help_h_x;
                }
            }
            else
            {
                move_h_x = move_help_h_x;
            }
        }

        if (move_help_h_z.magnitude > 0f)
        {
            if (move_h_z.normalized == move_help_h_z.normalized)
            {
                if (move_help_h_z.magnitude > move_h_z.magnitude)
                {
                    move_h_z = move_help_h_z;
                }
            }
            else
            {
                move_h_z = move_help_h_z;
            }
        }

        move_h = move_h_x + move_h_z;

        // process character jump
        if (_charInputs.jump && _currentJumpCount < _airMaxJumpCount)
        {
            _currentJumpCount++;

            if (_currentMaintainVelocityOnJump == false)
            {
                move_v = Vector3.zero;
            }

            move_v = char_up * _currentJumpPower;
        }

        Vector3 move = move_h + move_v;

        AirMove(move);
    }

    protected virtual void AirCalculateValues()
    {
        _currentMoveAccel = _airMoveAcceleration;
        _currentMoveSpeed = _airMoveSpeed;
        _currentJumpPower = _airJumpPower;
        _currentMaxJumpCount = _airMaxJumpCount;
        _currentMinMoveDist = _airMinMoveDistance;

        // TODO: add this field in data asset
        _currentMaintainVelocityOnJump = false;
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        Vector3 lastPosition = _charCapsule.localPosition;
        Vector3 remainingMove = originalMove;

        for (int i = 0; i < k_maxAirMoveIterations; i++)
        {
            remainingMove -= CapsuleMove(remainingMove, out RaycastHit hit, out Vector3 hitNormal);

            if (hit.collider == null)
            {
                // no collision, so end the move
                remainingMove = Vector3.zero;
                break;
            }

            AirMoveAlongSurface(originalMove, ref remainingMove, hit, hitNormal);
        }

        _velocity = _charCapsule.localPosition - lastPosition;

        if (delta_time != 0f)
        {
            _velocity = _velocity / delta_time;
        }
    }

    protected virtual void AirMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (hit.collider == null || remainingMove == global::UnityEngine.Vector3.zero)
            return;

        RecalculateNormalIfZero(hit, ref hitNormal);

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle))
        {
            remainingMove = Vector3.zero;
            _canGround = true;
            return;
        }

        // hit.normal gives normal respective to capsule's body,
        // useful for sliding off on corners
        Vector3 slideMove = Vector3.ProjectOnPlane(remainingMove, hit.normal);
        if (_currentMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
    }

    /// Air Physics | END
    //////////////////////////////////////////////////////////////////
}

public struct CharacterMovementGroundResult
{
    public static readonly CharacterMovementGroundResult invalid = new CharacterMovementGroundResult();

    public GameObject gameObject => collider ? collider.gameObject : null;
    public Collider collider;
    public int layer => gameObject ? gameObject.layer : 0;

    public Vector3 direction;
    public float distance;
    public float angle;
    public float edgeDistance;

    public Vector3 basePosition;
    public Quaternion baseRotation;

    public bool isValid
    {
        get => collider != null;
    }
}
