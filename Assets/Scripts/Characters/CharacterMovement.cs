using System;
using UnityEngine;
using GameLog;

using ILogger = GameLog.ILogger;

public class CharacterMovement : CharacterBehaviour
{
    protected const uint k_debugMoveMultiplier = 100;
    protected const uint k_maxMoveIterations = 10;

    protected const float k_collisionOffset = .001f;
    protected const float k_recalculateNormalFallback = .01f;
    protected const float k_recalculateNormalAddon = .001f;

    protected const float k_minMoveAlongSurfaceForMaintainVelocity = .0001f;
    protected const float k_minGroundSlopeUpAngle = 0f;
    protected const float k_maxGroundSlopeUpAngle = 89.9f;

    public CharacterDataAsset charDataAsset { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterInputs charInputs { get; protected set; }
    public CharacterView charView { get; protected set; }
    public ILogger logger { get; protected set; }

    [SerializeField] private CharacterMovementStateImpl _movementState;
    public virtual CharacterMovementState movementState
    {
        get => _movementState;
        protected set => _movementState = new CharacterMovementStateImpl(value.current, value.weight);
    }

    [SerializeField] protected Vector3 _velocity;

    [SerializeField] protected CharacterMovementGroundResult _previousGroundResult;
    [SerializeField] protected CharacterMovementGroundResult _groundResult;

    protected float _currentStepUpHeight = 0;
    protected float _currentStepDownDepth = 0;
    protected float _currentSlopeUpAngle = 0;
    protected float _currentSlopeDownAngle = 0;
    protected bool _currentMaintainVelocityOnSurface = true;
    protected bool _currentMaintainVelocityAlongSurface = true;

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

    }

    public override void OnUpdateCharacter()
    {
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

        if (_velocity.y == 0f && _groundResult.isValid)
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

        Vector3 resolvePenetration = CapsuleResolvePenetration();

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
        return hit.RecalculateNormalUsingRaycast(out normal, charCapsule.layerMask, charCapsule.triggerQuery);
    }

    protected bool RecalculateNormal(RaycastHit hit, Vector3 direction, out Vector3 normal)
    {
        if (hit.collider)
        {
            Vector3 rayDirection = direction;
            Vector3 rayOrigin = hit.point + (-rayDirection * k_recalculateNormalFallback);
            const float rayDistance = k_recalculateNormalFallback + k_recalculateNormalAddon;

            Physics.Raycast(rayOrigin, rayDirection, out RaycastHit rayHit,
                rayDistance, charCapsule.layerMask, charCapsule.triggerQuery);

            if (rayHit.collider && rayHit.collider == hit.collider)
            {
                normal = rayHit.normal;
                return true;
            }
        }

        normal = Vector3.zero;
        return false;
    }

    //////////////////////////////////////////////////////////////////
    /// Ground Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysGround()
    {
        GroundCalculateValues(movementState.current, out float speed, out float acceleration, out float jump,
            out _currentStepDownDepth, out _currentStepUpHeight, out _currentSlopeUpAngle,
            out _currentSlopeDownAngle);

        float deltaTime = Time.deltaTime;
        Vector3 characterUp = character.up;

        Vector3 moveInput = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        moveInput = Quaternion.Euler(0, charView.turnAngle, 0) * moveInput.normalized;
        moveInput = character.rotation * moveInput;

        _velocity = Vector3.ProjectOnPlane(_velocity, characterUp);

        Vector3 deltaMove = moveInput * speed * deltaTime;
        deltaMove = Vector3.MoveTowards(_velocity, deltaMove, acceleration * deltaTime);

        deltaMove += characterUp * jump * deltaTime;

        GroundMove(deltaMove);
    }

    protected virtual void GroundCalculateValues(uint state, out float speed,
        out float acceleration, out float jump, out float stepDownDepth,
        out float stepUpHeight, out float slopeUpAngle, out float slopeDownAngle)
    {
        switch (state)
        {
            case CharacterMovementState.GROUND_STAND_IDLE:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandIdleAcceleration;
                speed = groundStandIdleSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE_JUMP:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandIdleAcceleration;
                speed = groundStandIdleSpeed;
                jump = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandWalkAcceleration;
                speed = groundStandWalkSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_WALK_JUMP:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandWalkAcceleration;
                speed = groundStandWalkSpeed;
                jump = groundStandJumpForce;
                break;


            case CharacterMovementState.GROUND_STAND_RUN:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandRunAcceleration;
                speed = groundStandRunSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_RUN_JUMP:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandRunAcceleration;
                speed = groundStandRunSpeed;
                jump = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandSprintAcceleration;
                speed = groundStandSprintSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT_JUMP:
                stepDownDepth = groundStandStepDownDepth;
                stepUpHeight = groundStandStepUpHeight;
                slopeUpAngle = groundStandSlopeUpAngle;
                slopeDownAngle = groundStandSlopeDownAngle;
                acceleration = groundStandSprintAcceleration;
                speed = groundStandSprintSpeed;
                jump = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                stepDownDepth = groundCrouchStepDownDepth;
                stepUpHeight = groundCrouchStepUpHeight;
                slopeUpAngle = groundCrouchSlopeUpAngle;
                slopeDownAngle = groundCrouchSlopeDownAngle;
                acceleration = groundCrouchIdleAcceleration;
                speed = groundCrouchIdleSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                stepDownDepth = groundCrouchStepDownDepth;
                stepUpHeight = groundCrouchStepUpHeight;
                slopeUpAngle = groundCrouchSlopeUpAngle;
                slopeDownAngle = groundCrouchSlopeDownAngle;
                acceleration = groundCrouchWalkAcceleration;
                speed = groundCrouchWalkSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                stepDownDepth = groundCrouchStepDownDepth;
                stepUpHeight = groundCrouchStepUpHeight;
                slopeUpAngle = groundCrouchSlopeUpAngle;
                slopeDownAngle = groundCrouchSlopeDownAngle;
                acceleration = groundCrouchRunAcceleration;
                speed = groundCrouchRunSpeed;
                jump = 0f;
                break;

            case CharacterMovementState.GROUND_SLIDE:
            case CharacterMovementState.GROUND_ROLL:
            default:
                stepDownDepth = 0f;
                stepUpHeight = 0f;
                slopeUpAngle = 0f;
                slopeDownAngle = 0f;
                acceleration = 0f;
                speed = 0f;
                jump = 0f;
                break;
        }
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 characterUp = character.up;
        Vector3 lastPosition = charCapsule.localPosition;

        GroundResizeCapsule();

        Vector3 horizontalMove = Vector3.ProjectOnPlane(originalMove, characterUp);
        Vector3 verticalMove = originalMove - horizontalMove;
        float verticalMoveMagnitude = verticalMove.magnitude;

        Vector3 remainingMove = horizontalMove;

        // perform the vertical move (usually jump)
        CapsuleMove(verticalMove);

        if (remainingMove.magnitude > groundMinMoveDistance)
        {
            var stepUpHeight = 0f;
            var canStepUp = verticalMoveMagnitude == 0f;
            var didStepUp = false;
            var didStepUpRecover = false;
            var positionBeforeStepUp = Vector3.zero;
            var moveBeforeStepUp = Vector3.zero;

            CapsuleResolvePenetration();

            for (uint it = 0; it < k_maxMoveIterations; it++)
            {
                remainingMove -= CapsuleMove(remainingMove, out RaycastHit moveHit, out Vector3 moveHitNormal);

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
                                remainingMove = moveBeforeStepUp;
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
                if (GroundSlideOnSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
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
                    moveBeforeStepUp = remainingMove;

                    stepUpHeight = CapsuleMove(characterUp * _currentStepUpHeight).magnitude;

                    continue;
                }

                // try sliding along the obstacle
                if (GroundSlideAlongSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
                {
                    continue;
                }

                // there's nothing we can do now, so stop the move
                remainingMove = Vector3.zero;
            }

        }

        GroundStepDown(originalMove);
        CapsuleResolvePenetration();

        _velocity = charCapsule.localPosition - lastPosition;

        if (verticalMoveMagnitude == 0f)
        {
            _velocity = Vector3.ProjectOnPlane(_velocity, characterUp);
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

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle))
        {
            if (slopeAngle == 0f)
            {
                return false;
            }

            Vector3 slopeMove = Vector3.ProjectOnPlane(remainingMove, hitNormal);
            if (_currentMaintainVelocityOnSurface)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            return true;
        }

        return false;
    }

    protected virtual bool GroundSlideAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        float remainingMoveSize = remainingMove.magnitude;

        if (hit.collider == null || remainingMoveSize == 0f)
            return false;

        if (hitNormal == Vector3.zero)
        {
            RecalculateNormal(hit, out hitNormal);
        }

        Vector3 hitProject = Vector3.ProjectOnPlane(hitNormal, character.up);
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
        var verticalMove = Vector3.Project(originalMove, character.up).magnitude;
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

            if (slopeNormal == Vector3.zero)
            {
                RecalculateNormal(hit, out slopeNormal);
            }

            float maxSlopeAngle = Math.Clamp(_currentSlopeUpAngle,
                k_minGroundSlopeUpAngle, k_maxGroundSlopeUpAngle);

            slopeAngle = Vector3.Angle(character.up, slopeNormal);


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
        result = new CharacterMovementGroundResult();

        if (GroundCheck(hit.collider) == false)
        {
            return false;
        }

        result.collider = hit.collider;
        result.direction = character.down;
        result.distance = hit.distance;

        result.angle = Vector3.Angle(character.up, hitNormal);

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
        Vector3 charUp = character.up;
        float deltaTime = Time.deltaTime;
        float moveAcceleration = airMoveAcceleration;
        float moveSpeed = airMoveSpeed;
        float mass = character.mass;
        float gravitySpeed = airGravityAcceleration * mass * deltaTime * deltaTime * .1f;

        Vector3 _horizontalVelocity = Vector3.ProjectOnPlane(_velocity, charUp);
        Vector3 _verticalVelocity = _velocity - _horizontalVelocity;

        Vector3 horizontalMove = _horizontalVelocity;
        Vector3 verticalMove = _verticalVelocity + charUp * gravitySpeed;

        // Calculate Movement
        Vector3 helperHorizontalMove = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        helperHorizontalMove = Quaternion.Euler(0, charView.turnAngle, 0) * helperHorizontalMove;
        helperHorizontalMove = character.rotation * helperHorizontalMove * moveSpeed;
        helperHorizontalMove = Vector3.ProjectOnPlane(helperHorizontalMove, horizontalMove);
        helperHorizontalMove = Vector3.zero;

        horizontalMove += helperHorizontalMove;

        Vector3 deltaMove = horizontalMove + verticalMove;

        AirMove(deltaMove);
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        Vector3 lastPosition = charCapsule.localPosition;

        CapsuleMove(originalMove);
        CapsuleResolvePenetration();

        _velocity = lastPosition - charCapsule.localPosition;
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

    public CharacterMovementGroundResult(RaycastHit hit, Vector3 direction)
    {
        direction.Normalize();

        this.collider = hit.collider;
        this.direction = direction;
        this.distance = hit.distance;

        if (direction != Vector3.zero)
        {
            Vector3 leftDirection = Quaternion.Euler(0, -90, 0) * direction;
            Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, leftDirection).normalized;
            this.angle = 90f - Vector3.SignedAngle(direction, obstacleForward, -leftDirection);
        }
        else
        {
            this.angle = 0;
        }

        this.edgeDistance = default;

        if (this.collider)
        {
            this.basePosition = collider.transform.position;
            this.baseRotation = collider.transform.rotation;
        }
        else
        {
            this.basePosition = Vector3.zero;
            this.baseRotation = Quaternion.identity;
        }
    }
}
