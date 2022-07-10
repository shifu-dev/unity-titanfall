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

    protected const float k_gravityMultiplier = .075f;

    public CharacterDataAsset charDataAsset { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterInputs charInputs { get; protected set; }
    public CharacterView charView { get; protected set; }

    [SerializeField] private CharacterMovementStateImpl _movementState;
    public virtual CharacterMovementState movementState
    {
        get => _movementState;
        protected set => _movementState = new CharacterMovementStateImpl(value.current, value.weight);
    }

    [SerializeField] protected CharacterMovementGroundResult _previousGroundResult;
    [SerializeField] protected CharacterMovementGroundResult _groundResult;

    // protected float delta_time = 1f;
    protected float _delta_time;
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

    protected float groundStandStepUpHeight => charCapsule.height * groundStandStepUpPercent / 100;
    protected float groundStandStepDownDepth => charCapsule.height * groundStandStepDownPercent / 100;
    protected float groundCrouchStepUpHeight => charCapsule.height * groundCrouchStepUpPercent / 100;
    protected float groundCrouchStepDownDepth => charCapsule.height * groundCrouchStepUpPercent / 100;

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
        _delta_time = 1f;
    }

    //////////////////////////////////////////////////////////////////
    /// Update Calls
    //////////////////////////////////////////////////////////////////

    public override void OnInitCharacter(Character character, CharacterInitializer initializer)
    {
        base.OnInitCharacter(character, initializer);

        charDataAsset = character.charDataAsset;
        charInputs = character.charInputs;
        charCapsule = character.charCapsule;
        charView = character.charView;

        // Copy Data from CharacterDataAsset
        groundCheckDepth = charDataAsset.groundCheckDepth;
        groundLayer = charDataAsset.groundLayer;
        groundMinMoveDistance = charDataAsset.groundMinMoveDistance;

        groundStandIdleSpeed = charDataAsset.groundStandIdleSpeed;
        groundStandIdleAcceleration = charDataAsset.groundStandIdleAcceleration;
        groundStandWalkSpeed = charDataAsset.groundStandWalkSpeed;
        groundStandWalkAcceleration = charDataAsset.groundStandWalkAcceleration;
        groundStandRunSpeed = charDataAsset.groundStandRunSpeed;
        groundStandRunAcceleration = charDataAsset.groundStandRunAcceleration;
        groundStandSprintSpeed = charDataAsset.groundStandSprintSpeed;
        groundStandSprintAcceleration = charDataAsset.groundStandSprintAcceleration;
        groundStandSprintLeftAngleMax = charDataAsset.groundStandSprintLeftAngleMax;
        groundStandSprintRightAngleMax = charDataAsset.groundStandSprintRightAngleMax;
        groundStandJumpForce = charDataAsset.groundStandJumpForce;
        groundStandStepUpPercent = charDataAsset.groundStandStepUpPercent;
        groundStandStepDownPercent = charDataAsset.groundStandStepDownPercent;
        groundStandSlopeUpAngle = charDataAsset.groundStandSlopeUpAngle;
        groundStandSlopeDownAngle = charDataAsset.groundStandSlopeDownAngle;
        groundStandMaintainVelocityOnSurface = charDataAsset.groundStandMaintainVelocityOnSurface;
        groundStandMaintainVelocityAlongSurface = charDataAsset.groundStandMaintainVelocityAlongSurface;
        groundStandCapsuleCenter = charDataAsset.groundStandCapsuleCenter;
        groundStandCapsuleHeight = charDataAsset.groundStandCapsuleHeight;
        groundStandCapsuleRadius = charDataAsset.groundStandCapsuleRadius;
        groundStandToCrouchTransitionSpeed = charDataAsset.groundStandToCrouchTransitionSpeed;

        groundCrouchIdleSpeed = charDataAsset.groundCrouchIdleSpeed;
        groundCrouchIdleAcceleration = charDataAsset.groundCrouchIdleAcceleration;
        groundCrouchWalkSpeed = charDataAsset.groundCrouchWalkSpeed;
        groundCrouchWalkAcceleration = charDataAsset.groundCrouchWalkAcceleration;
        groundCrouchRunSpeed = charDataAsset.groundCrouchRunSpeed;
        groundCrouchRunAcceleration = charDataAsset.groundCrouchRunAcceleration;
        groundCrouchAutoRiseToStandSprint = charDataAsset.groundCrouchAutoRiseToStandSprint;
        groundCrouchJumpForce = charDataAsset.groundCrouchJumpForce;
        groundCrouchStepUpPercent = charDataAsset.groundCrouchStepUpPercent;
        groundCrouchStepDownPercent = charDataAsset.groundCrouchStepDownPercent;
        groundCrouchSlopeUpAngle = charDataAsset.groundCrouchSlopeUpAngle;
        groundCrouchSlopeDownAngle = charDataAsset.groundCrouchSlopeDownAngle;
        groundCrouchMaintainVelocityOnSurface = charDataAsset.groundCrouchMaintainVelocityOnSurface;
        groundCrouchMaintainVelocityAlongSurface = charDataAsset.groundCrouchMaintainVelocityAlongSurface;
        groundCrouchCapsuleCenter = charDataAsset.groundCrouchCapsuleCenter;
        groundCrouchCapsuleHeight = charDataAsset.groundCrouchCapsuleHeight;
        groundCrouchCapsuleRadius = charDataAsset.groundCrouchCapsuleRadius;
        groundCrouchToStandTransitionSpeed = charDataAsset.groundCrouchToStandTransitionSpeed;

        airGravityAcceleration = charDataAsset.airGravityAcceleration;
        airGravityMaxSpeed = charDataAsset.airGravityMaxSpeed;
        airMinMoveDistance = charDataAsset.airMinMoveDistance;
        airMoveSpeed = charDataAsset.airMoveSpeed;
        airMoveAcceleration = charDataAsset.airMoveAcceleration;
        airJumpPower = charDataAsset.airJumpPower;
        airMaxJumpCount = charDataAsset.airMaxJumpCount;

        _velocity = Vector3.zero;
    }

    public override void OnUpdateCharacter()
    {
        _delta_time = Time.deltaTime;

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
        movementState = new CharacterMovementStateImpl(CharacterMovementState.NONE);
    }

    protected virtual void UpdateMovementState()
    {
        CharacterMovementState newState = new CharacterMovementStateImpl();

        if (_canGround && _groundResult.isValid)
        {
            if (charInputs.crouch)
            {
                if (charInputs.move.magnitude == 0)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_IDLE;
                }
                else if (charInputs.walk)
                {
                    newState.current = CharacterMovementState.GROUND_CROUCH_WALK;
                }
                else if (charInputs.sprint && charInputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && charInputs.moveAngle < groundStandSprintRightAngleMax)
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
                if (charInputs.move.magnitude == 0)
                {
                    if (charInputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_IDLE;
                    }
                }
                else if (charInputs.walk)
                {
                    if (charInputs.jump)
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK_JUMP;
                    }
                    else
                    {
                        newState.current = CharacterMovementState.GROUND_STAND_WALK;
                    }
                }
                else if (charInputs.sprint && charInputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && charInputs.moveAngle < groundStandSprintRightAngleMax)
                {
                    if (charInputs.jump)
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
                    if (charInputs.jump)
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

    //////////////////////////////////////////////////////////////////
    /// Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void UpdatePhysicsData()
    {
        if (_groundResult.isValid)
        {
            var deltaPosition = _groundResult.collider.transform.position - _groundResult.basePosition;
            // var deltaRotation = m_groundResult.collider.transform.rotation.eulerAngles - m_groundResult.baseRotation.eulerAngles;

            charCapsule.localPosition += deltaPosition;
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
        bigHit = charCapsule.BigCapsuleCast(move);
        if (charCapsule.skinWidth > 0f)
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

        smallHit = charCapsule.SmallCapsuleCast(move);

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
        bigHit = charCapsule.BigBaseSphereCast(move);
        if (charCapsule.skinWidth > 0f)
        {
            if (bigHit.collider)
            {
                move = move.normalized * bigHit.distance;
            }

            smallHit = charCapsule.SmallBaseSphereCast(move);
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
            distance = smallHit.distance - charCapsule.skinWidth - k_collisionOffset;
        }
        else if (bigHit.collider)
        {
            distance = bigHit.distance - k_collisionOffset;
        }

        charCapsule.localPosition += direction * distance;

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
        return charCapsule.ResolvePenetrationForBigCapsule(k_collisionOffset);
    }

    protected void TeleportTo(Vector3 pos, Quaternion rot)
    {
        charCapsule.position = pos;
        charCapsule.rotation = rot;
    }

    protected bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
    {
        return hit.RecalculateNormalUsingRaycast(out normal,
            charCapsule.layerMask, charCapsule.triggerQuery);
    }

    protected bool RecalculateNormal(RaycastHit hit, Vector3 direction, out Vector3 normal)
    {
        if (hit.collider && direction != Vector3.zero)
        {
            Vector3 origin = hit.point + (-direction * k_recalculateNormalFallback);
            const float rayDistance = k_recalculateNormalFallback + k_recalculateNormalAddon;

            Physics.Raycast(origin, direction, out RaycastHit rayHit,
                rayDistance, charCapsule.layerMask, charCapsule.triggerQuery);

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

        Vector3 move_input_raw = charInputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0, move_input_raw.y);
        move_input = Quaternion.Euler(0, charView.turnAngle, 0) * move_input.normalized;
        move_input = character.rotation * move_input;

        _velocity = Vector3.ProjectOnPlane(_velocity, _char_up);

        Vector3 move = move_input * _currentMoveSpeed * _delta_time;
        move = Vector3.MoveTowards(_velocity * _delta_time, move, _currentMoveAccel * _delta_time);

        move += _char_up * _currentJumpPower * _delta_time;

        GroundMove(move);
    }

    protected virtual void GroundCalculateValues()
    {
        _char_up = character.up;
        _char_right = character.right;
        _char_forward = character.forward;
        _currentJumpCount = 0;
        _currentMinMoveDist = groundMinMoveDistance;

        if (movementState.isGroundStanding)
        {
            _currentMaintainVelocityOnSurface = groundStandMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = groundStandMaintainVelocityAlongSurface;
        }
        else if (movementState.isGroundCrouching)
        {
            _currentMaintainVelocityOnSurface = groundCrouchMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = groundCrouchMaintainVelocityAlongSurface;
        }

        switch (movementState.current)
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
        Vector3 last_pos = charCapsule.localPosition;

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
                                charCapsule.localPosition = positionBeforeStepUp;
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
                    positionBeforeStepUp = charCapsule.localPosition;
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

        _velocity = charCapsule.localPosition - last_pos;

        if (_delta_time != 0f)
        {
            _velocity = _velocity / _delta_time;
        }

        if (move_v_mag == 0f)
        {
            _velocity = Vector3.ProjectOnPlane(_velocity, _char_up);
        }
    }

    protected virtual void GroundResizeCapsule()
    {
        float weight = movementState.weight;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (movementState.isGroundCrouching)
        {
            targetCenter = groundCrouchCapsuleCenter;
            targetHeight = groundCrouchCapsuleHeight;
            targetRadius = groundCrouchCapsuleRadius;
            speed = groundStandToCrouchTransitionSpeed * Time.deltaTime;
        }
        else
        {
            targetCenter = groundStandCapsuleCenter;
            targetHeight = groundStandCapsuleHeight;
            targetRadius = groundStandCapsuleRadius;
            speed = groundCrouchToStandTransitionSpeed * Time.deltaTime;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        charCapsule.localCenter = Vector3.Lerp(charCapsule.localCenter, targetCenter, speed);
        charCapsule.localHeight = Mathf.Lerp(charCapsule.localHeight, targetHeight, speed);
        charCapsule.localRadius = Mathf.Lerp(charCapsule.localRadius, targetRadius, speed);

        weight = Mathf.Lerp(weight, Weight.max, speed);
        movementState.weight = weight;
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
            charCapsule.localPosition -= moved;
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
        float gravity_speed = airGravityAcceleration * mass * _delta_time * _delta_time * k_gravityMultiplier;

        Vector3 velocity = _velocity * _delta_time;
        Vector3 velocity_h = Vector3.ProjectOnPlane(velocity, char_up);
        Vector3 velocity_v = velocity - velocity_h;

        Vector3 move_v = velocity_v + (char_up * gravity_speed);
        Vector3 move_h = velocity_h;

        Vector3 move_h_x = Vector3.ProjectOnPlane(move_h, char_forward);
        Vector3 move_h_z = move_h - move_h_x;
        // processed move input
        Vector3 move_input_raw = charInputs.move;
        Vector3 move_input = new Vector3(move_input_raw.x, 0f, move_input_raw.y);
        move_input = Quaternion.Euler(0f, charView.turnAngle, 0f) * move_input;
        move_input = character.rotation * move_input;

        // helping movement in air
        Vector3 move_help_h = _currentMoveSpeed * move_input * _delta_time;
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
        if (charInputs.jump && _currentJumpCount < airMaxJumpCount)
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
        Vector3 lastPosition = charCapsule.localPosition;
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

        _velocity = charCapsule.localPosition - lastPosition;

        if (_delta_time != 0f)
        {
            _velocity = _velocity / _delta_time;
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
