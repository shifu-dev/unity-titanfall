using System;
using UnityEngine;

public class CharacterMovement : CharacterBehaviour
{
    protected const uint k_maxGroundMoveIterations = 10;
    protected const uint k_maxAirMoveIterations = 10;

    protected const float k_collisionOffset = .001f;
    protected const float k_recalculateNormalFallback = .01f;
    protected const float k_recalculateNormalAddon = .001f;

    protected const float k_minMoveAlongSurfaceForMaintainVelocity = .0001f;
    protected const float k_minGroundSlopeUpAngle = 0f;
    protected const float k_maxGroundSlopeUpAngle = 89.9f;

    protected const float k_gravityMultiplier = .05f;

    protected CharacterDataSource _char_data_asset;
    protected CharacterCapsule _char_capsule;
    protected CharacterInputs _char_inputs;
    protected CharacterView _char_view;

    [SerializeField, ReadOnly] private CharacterMovementStateImpl _movementState;
    public virtual CharacterMovementState movement_state
    {
        get => _movementState;
        protected set => _movementState = new CharacterMovementStateImpl(value.current);
    }

    [SerializeField, ReadOnly] protected float movement_state_weight;

    [SerializeField] protected CharacterMovementGroundResult _previousGroundResult;
    [SerializeField] protected CharacterMovementGroundResult _groundResult;

    [ReadOnly, SerializeField] protected Vector3 _velocity;
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

    //////////////////////////////////////////////////////////////////
    /// CharacterMovementData Copy
    //////////////////////////////////////////////////////////////////

    protected LayerMask groundLayer;

    protected float groundStandWalkSpeed, groundStandWalkAcceleration, groundStandRunSpeed,
        groundStandRunAcceleration, groundStandSprintSpeed, groundStandSprintAcceleration,
        groundStandSprintLeftAngleMax, groundStandSprintRightAngleMax, groundStandJumpForce,
        groundStandStepUpPercent, groundStandStepDownPercent, groundStandSlopeUpAngle,
        groundStandSlopeDownAngle, groundStandCapsuleHeight, groundStandCapsuleRadius,
        groundStandToCrouchTransitionSpeed, groundCrouchWalkSpeed, groundCrouchWalkAcceleration,
        groundCrouchRunSpeed, groundCrouchRunAcceleration, groundCrouchJumpForce,
        groundCrouchStepUpPercent, groundCrouchStepDownPercent, groundCrouchSlopeUpAngle,
        groundCrouchSlopeDownAngle, groundCrouchCapsuleHeight, groundCrouchCapsuleRadius,
        groundCrouchToStandTransitionSpeed, groundMinMoveDistance, groundCheckDepth,
        groundStandIdleAcceleration, groundStandIdleSpeed, groundCrouchIdleSpeed,
        groundCrouchIdleAcceleration, airGravityAcceleration, airGravityMaxSpeed,
        airMinMoveDistance, airMoveSpeed, airMoveAcceleration, airJumpPower;

    protected float groundStandStepUpHeight => _char_capsule.height * groundStandStepUpPercent / 100;
    protected float groundStandStepDownDepth => _char_capsule.height * groundStandStepDownPercent / 100;
    protected float groundCrouchStepUpHeight => _char_capsule.height * groundCrouchStepUpPercent / 100;
    protected float groundCrouchStepDownDepth => _char_capsule.height * groundCrouchStepUpPercent / 100;

    protected uint airMaxJumpCount;

    protected bool groundStandMaintainVelocityOnSurface, groundStandMaintainVelocityAlongSurface,
        groundCrouchAutoRiseToStandSprint, groundCrouchMaintainVelocityOnSurface,
        groundCrouchMaintainVelocityAlongSurface;

    protected Vector3 groundStandCapsuleCenter, groundCrouchCapsuleCenter;

    public CharacterMovement()
    {
        CreateMovementState();

        _previousGroundResult = CharacterMovementGroundResult.invalid;
        _groundResult = CharacterMovementGroundResult.invalid;
        _velocity = Vector3.zero;
    }

    //////////////////////////////////////////////////////////////////
    /// Update Calls
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        _char_data_asset = character.charDataSource;
        _char_inputs = character.charInputs;
        _char_capsule = character.charCapsule;
        _char_view = character.charView;

        // Copy Data from CharacterDataSource
        groundCheckDepth = _char_data_asset.groundCheckDepth;
        groundLayer = _char_data_asset.groundLayer;
        groundMinMoveDistance = _char_data_asset.groundMinMoveDistance;

        groundStandIdleSpeed = _char_data_asset.groundStandIdleSpeed;
        groundStandIdleAcceleration = _char_data_asset.groundStandIdleAcceleration;
        groundStandWalkSpeed = _char_data_asset.groundStandWalkSpeed;
        groundStandWalkAcceleration = _char_data_asset.groundStandWalkAcceleration;
        groundStandRunSpeed = _char_data_asset.groundStandRunSpeed;
        groundStandRunAcceleration = _char_data_asset.groundStandRunAcceleration;
        groundStandSprintSpeed = _char_data_asset.groundStandSprintSpeed;
        groundStandSprintAcceleration = _char_data_asset.groundStandSprintAcceleration;
        groundStandSprintLeftAngleMax = _char_data_asset.groundStandSprintLeftAngleMax;
        groundStandSprintRightAngleMax = _char_data_asset.groundStandSprintRightAngleMax;
        groundStandJumpForce = _char_data_asset.groundStandJumpForce;
        groundStandStepUpPercent = _char_data_asset.groundStandStepUpPercent;
        groundStandStepDownPercent = _char_data_asset.groundStandStepDownPercent;
        groundStandSlopeUpAngle = _char_data_asset.groundStandSlopeUpAngle;
        groundStandSlopeDownAngle = _char_data_asset.groundStandSlopeDownAngle;
        groundStandMaintainVelocityOnSurface = _char_data_asset.groundStandMaintainVelocityOnSurface;
        groundStandMaintainVelocityAlongSurface = _char_data_asset.groundStandMaintainVelocityAlongSurface;
        groundStandCapsuleCenter = _char_data_asset.groundStandCapsuleCenter;
        groundStandCapsuleHeight = _char_data_asset.groundStandCapsuleHeight;
        groundStandCapsuleRadius = _char_data_asset.groundStandCapsuleRadius;
        groundStandToCrouchTransitionSpeed = _char_data_asset.groundStandToCrouchTransitionSpeed;

        groundCrouchIdleSpeed = _char_data_asset.groundCrouchIdleSpeed;
        groundCrouchIdleAcceleration = _char_data_asset.groundCrouchIdleAcceleration;
        groundCrouchWalkSpeed = _char_data_asset.groundCrouchWalkSpeed;
        groundCrouchWalkAcceleration = _char_data_asset.groundCrouchWalkAcceleration;
        groundCrouchRunSpeed = _char_data_asset.groundCrouchRunSpeed;
        groundCrouchRunAcceleration = _char_data_asset.groundCrouchRunAcceleration;
        groundCrouchAutoRiseToStandSprint = _char_data_asset.groundCrouchAutoRiseToStandSprint;
        groundCrouchJumpForce = _char_data_asset.groundCrouchJumpForce;
        groundCrouchStepUpPercent = _char_data_asset.groundCrouchStepUpPercent;
        groundCrouchStepDownPercent = _char_data_asset.groundCrouchStepDownPercent;
        groundCrouchSlopeUpAngle = _char_data_asset.groundCrouchSlopeUpAngle;
        groundCrouchSlopeDownAngle = _char_data_asset.groundCrouchSlopeDownAngle;
        groundCrouchMaintainVelocityOnSurface = _char_data_asset.groundCrouchMaintainVelocityOnSurface;
        groundCrouchMaintainVelocityAlongSurface = _char_data_asset.groundCrouchMaintainVelocityAlongSurface;
        groundCrouchCapsuleCenter = _char_data_asset.groundCrouchCapsuleCenter;
        groundCrouchCapsuleHeight = _char_data_asset.groundCrouchCapsuleHeight;
        groundCrouchCapsuleRadius = _char_data_asset.groundCrouchCapsuleRadius;
        groundCrouchToStandTransitionSpeed = _char_data_asset.groundCrouchToStandTransitionSpeed;

        airGravityAcceleration = _char_data_asset.airGravityAcceleration;
        airGravityMaxSpeed = _char_data_asset.airGravityMaxSpeed;
        airMinMoveDistance = _char_data_asset.airMinMoveDistance;
        airMoveSpeed = _char_data_asset.airMoveSpeed;
        airMoveAcceleration = _char_data_asset.airMoveAcceleration;
        airJumpPower = _char_data_asset.airJumpPower;
        airMaxJumpCount = _char_data_asset.airMaxJumpCount;

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

    //////////////////////////////////////////////////////////////////
    /// Movement State
    //////////////////////////////////////////////////////////////////

    protected virtual void CreateMovementState()
    {
        movement_state = new CharacterMovementStateImpl(CharacterMovementState.NONE);
    }

    protected virtual void UpdateMovementState()
    {
        CharacterMovementState newState = new CharacterMovementStateImpl();

        if (_canGround && _groundResult.isValid)
        {
            if (_char_inputs.crouch)
            {
                if (_char_inputs.move.magnitude == 0)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_IDLE;
                }
                else if (_char_inputs.walk)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_WALK;
                }
                else if (_char_inputs.sprint && _char_inputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && _char_inputs.moveAngle < groundStandSprintRightAngleMax)
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
                if (_char_inputs.move.magnitude == 0)
                {
                    if (_char_inputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE;
                    }
                }
                else if (_char_inputs.walk)
                {
                    if (_char_inputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK;
                    }
                }
                else if (_char_inputs.sprint && _char_inputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && _char_inputs.moveAngle < groundStandSprintRightAngleMax)
                {
                    if (_char_inputs.jump)
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
                    if (_char_inputs.jump)
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

        movement_state = newState;
    }

    protected virtual bool SetMovementState(uint state)
    {
        if (state == movement_state.current)
            return false;

        if (CanEnterMovementState(state))
        {
            movement_state.current = state;
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

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void UpdatePhysicsData()
    {
        if (_groundResult.isValid)
        {
            var deltaPosition = _groundResult.collider.transform.position - _groundResult.basePosition;
            // var deltaRotation = m_groundResult.collider.transform.rotation.eulerAngles - m_groundResult.baseRotation.eulerAngles;

            _char_capsule.localPosition += deltaPosition;
            // charCapsule.localRotation = Quaternion.Euler(charCapsule.localRotation.eulerAngles + deltaRotation);
        }

        CapsuleResolvePenetration();
        GroundUpdateResult();
    }

    protected virtual void UpdatePhysicsState()
    {
        if (movement_state.isGrounded)
        {
            PhysGround();
        }
        else if (movement_state.isAir)
        {
            PhysAir();
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Capsule API
    //////////////////////////////////////////////////////////////////

    protected bool CapsuleCast(Vector3 move, out RaycastHit hit)
    {
        bool result = CapsuleCast(move, out RaycastHit smallHit, out RaycastHit bigHit);
        hit = smallHit.collider ? smallHit : bigHit;

        return result;
    }

    protected bool CapsuleCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        bigHit = _char_capsule.BigCapsuleCast(move);
        if (_char_capsule.skinWidth > 0f)
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

        smallHit = _char_capsule.SmallCapsuleCast(move);

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
        bigHit = _char_capsule.BigBaseSphereCast(move);
        if (_char_capsule.skinWidth > 0f)
        {
            if (bigHit.collider)
            {
                move = move.normalized * bigHit.distance;
            }

            smallHit = _char_capsule.SmallBaseSphereCast(move);
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
            distance = smallHit.distance - _char_capsule.skinWidth - k_collisionOffset;
        }
        else if (bigHit.collider)
        {
            distance = bigHit.distance - k_collisionOffset;
        }

        _char_capsule.localPosition += direction * distance;

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
        return _char_capsule.ResolvePenetrationForBigCapsule(k_collisionOffset);
    }

    protected void TeleportTo(Vector3 pos, Quaternion rot)
    {
        _char_capsule.position = pos;
        _char_capsule.rotation = rot;
    }

    protected bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
    {
        return hit.RecalculateNormalUsingRaycast(out normal,
            _char_capsule.layerMask, _char_capsule.triggerQuery);
    }

    protected bool RecalculateNormal(RaycastHit hit, Vector3 direction, out Vector3 normal)
    {
        if (hit.collider && direction != Vector3.zero)
        {
            Vector3 origin = hit.point + (-direction * k_recalculateNormalFallback);
            const float rayDistance = k_recalculateNormalFallback + k_recalculateNormalAddon;

            Physics.Raycast(origin, direction, out RaycastHit rayHit,
                rayDistance, _char_capsule.layerMask, _char_capsule.triggerQuery);

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

    //////////////////////////////////////////////////////////////////
    /// Ground Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysGround()
    {
        GroundCalculateValues();

        Vector3 move_input_raw = _char_inputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0, move_input_raw.y);
        move_input = Quaternion.Euler(0, _char_view.turnAngle, 0) * move_input.normalized;
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
        _currentMinMoveDist = groundMinMoveDistance;

        if (movement_state.isGroundStanding)
        {
            _currentMaintainVelocityOnSurface = groundStandMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = groundStandMaintainVelocityAlongSurface;
        }
        else if (movement_state.isGroundCrouching)
        {
            _currentMaintainVelocityOnSurface = groundCrouchMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = groundCrouchMaintainVelocityAlongSurface;
        }

        switch (movement_state.current)
        {
            case CharacterMovementState.GROUND_STAND_IDLE:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandIdleAcceleration;
                _currentMoveSpeed = groundStandIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE_JUMP:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandIdleAcceleration;
                _currentMoveSpeed = groundStandIdleSpeed;
                _currentJumpPower = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandWalkAcceleration;
                _currentMoveSpeed = groundStandWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_WALK_JUMP:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandWalkAcceleration;
                _currentMoveSpeed = groundStandWalkSpeed;
                _currentJumpPower = groundStandJumpForce;
                break;


            case CharacterMovementState.GROUND_STAND_RUN:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandRunAcceleration;
                _currentMoveSpeed = groundStandRunSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_RUN_JUMP:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandRunAcceleration;
                _currentMoveSpeed = groundStandRunSpeed;
                _currentJumpPower = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandSprintAcceleration;
                _currentMoveSpeed = groundStandSprintSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT_JUMP:
                _currentStepDownDepth = groundStandStepDownDepth;
                _currentStepUpHeight = groundStandStepUpHeight;
                _currentSlopeUpAngle = groundStandSlopeUpAngle;
                _currentSlopeDownAngle = groundStandSlopeDownAngle;
                _currentMoveAccel = groundStandSprintAcceleration;
                _currentMoveSpeed = groundStandSprintSpeed;
                _currentJumpPower = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                _currentStepDownDepth = groundCrouchStepDownDepth;
                _currentStepUpHeight = groundCrouchStepUpHeight;
                _currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = groundCrouchSlopeDownAngle;
                _currentMoveAccel = groundCrouchIdleAcceleration;
                _currentMoveSpeed = groundCrouchIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                _currentStepDownDepth = groundCrouchStepDownDepth;
                _currentStepUpHeight = groundCrouchStepUpHeight;
                _currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = groundCrouchSlopeDownAngle;
                _currentMoveAccel = groundCrouchWalkAcceleration;
                _currentMoveSpeed = groundCrouchWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                _currentStepDownDepth = groundCrouchStepDownDepth;
                _currentStepUpHeight = groundCrouchStepUpHeight;
                _currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = groundCrouchSlopeDownAngle;
                _currentMoveAccel = groundCrouchRunAcceleration;
                _currentMoveSpeed = groundCrouchRunSpeed;
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
        Vector3 last_pos = _char_capsule.localPosition;

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
                        // if we cannot step on this ground, revert the step up
                        // and continue the loop without stepping up this time
                        if (GroundCanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal, out float baseAngle) == false)
                        {
                            if (baseAngle < 90f)
                            {
                                _char_capsule.localPosition = positionBeforeStepUp;
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
                    positionBeforeStepUp = _char_capsule.localPosition;
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

        _velocity = _char_capsule.localPosition - last_pos;

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
        float weight = movement_state_weight;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (movement_state.isGroundCrouching)
        {
            targetCenter = groundCrouchCapsuleCenter;
            targetHeight = groundCrouchCapsuleHeight;
            targetRadius = groundCrouchCapsuleRadius;
            speed = groundStandToCrouchTransitionSpeed * delta_time;
        }
        else
        {
            targetCenter = groundStandCapsuleCenter;
            targetHeight = groundStandCapsuleHeight;
            targetRadius = groundStandCapsuleRadius;
            speed = groundCrouchToStandTransitionSpeed * delta_time;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        _char_capsule.localCenter = Vector3.Lerp(_char_capsule.localCenter, targetCenter, speed);
        _char_capsule.localHeight = Mathf.Lerp(_char_capsule.localHeight, targetHeight, speed);
        _char_capsule.localRadius = Mathf.Lerp(_char_capsule.localRadius, targetRadius, speed);

        weight = Mathf.Lerp(weight, 1f, speed);
        movement_state_weight = weight;
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
            _char_capsule.localPosition -= moved;
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

        return groundLayer.Contains(collider.gameObject.layer);
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
        GroundCast(groundCheckDepth, out _groundResult);
    }

    //////////////////////////////////////////////////////////////////
    /// Air Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysAir()
    {
        AirCalculateValues();

        Vector3 char_up = _char_up;
        Vector3 char_forward = character.forward;
        Vector3 char_right = character.right;
        float mass = character.mass;
        float gravity_speed = airGravityAcceleration * mass * delta_time * delta_time * k_gravityMultiplier;

        Vector3 velocity = _velocity * delta_time;
        Vector3 velocity_h = Vector3.ProjectOnPlane(velocity, char_up);
        Vector3 velocity_v = velocity - velocity_h;

        Vector3 move_v = velocity_v + (char_up * gravity_speed);
        Vector3 move_h = velocity_h;

        Vector3 move_h_x = Vector3.ProjectOnPlane(move_h, char_forward);
        Vector3 move_h_z = move_h - move_h_x;
        // processed move input
        Vector3 move_input_raw = _char_inputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0f, move_input_raw.y);
        move_input = Quaternion.Euler(0f, _char_view.turnAngle, 0f) * move_input;
        move_input = character.rotation * move_input;

        // helping movement in air
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
        if (_char_inputs.jump && _currentJumpCount < airMaxJumpCount)
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
        _currentMoveAccel = airMoveAcceleration;
        _currentMoveSpeed = airMoveSpeed;
        _currentJumpPower = airJumpPower;
        _currentMaxJumpCount = airMaxJumpCount;
        _currentMinMoveDist = airMinMoveDistance;

        // TODO: add this field in data asset
        _currentMaintainVelocityOnJump = false;
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        Vector3 lastPosition = _char_capsule.localPosition;
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

        _velocity = _char_capsule.localPosition - lastPosition;

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
