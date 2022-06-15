using System;
using UnityEngine;
using GameLog;

using ILogger = GameLog.ILogger;

public class CharacterMovement : CharacterBehaviour
{
    protected const uint k_debugMoveMultiplier = 100;
    protected const uint k_maxMoveIterations = 10;

    protected const float k_collisionOffset = .01f;
    protected const float k_recalculateNormalFallback = .01f;
    protected const float k_recalculateNormalAddon = .001f;

    protected const float k_minGroundSlopeUpAngle = 0f;
    protected const float k_maxGroundSlopeUpAngle = 89.9f;

    public CharacterDataAsset charDataAsset { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterInputs charInputs { get; protected set; }
    public CharacterView charView { get; protected set; }
    public ILogger logger { get; protected set; }

    [SerializeField] private CharacterMovementStateImpl m_movementState;
    public virtual CharacterMovementState movementState
    {
        get => m_movementState;
        protected set => m_movementState = new CharacterMovementStateImpl(value.current, value.weight);
    }

    [SerializeField] protected CharacterMovementGroundResult m_previousGroundResult;
    [SerializeField] protected CharacterMovementGroundResult m_groundResult;

    protected float groundStandStepUpHeight => charCapsule.height * groundStandStepUpPercent / 100;
    protected float groundStandStepDownDepth => charCapsule.height * groundStandStepDownPercent / 100;
    protected float groundCrouchStepUpHeight => charCapsule.height * groundCrouchStepUpPercent / 100;
    protected float groundCrouchStepDownDepth => charCapsule.height * groundCrouchStepUpPercent / 100;

    protected Vector3 m_velocity = Vector3.zero;
    protected Vector3 m_lastMove = Vector3.zero;
    protected float m_currentStepUpHeight = 0;
    protected float m_currentStepDownDepth = 0;
    protected float m_currentSlopeUpAngle = 0;
    protected float m_currentSlopeDownAngle = 0;
    protected bool m_currentMaintainVelocityOnSurface = true;
    protected bool m_currentMaintainVelocityAlongSurface = true;

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

    protected uint airMaxJumpCount;

    protected bool groundStandMaintainVelocityOnSurface, groundStandMaintainVelocityAlongSurface,
        groundCrouchAutoRiseToStandSprint, groundCrouchMaintainVelocityOnSurface,
        groundCrouchMaintainVelocityAlongSurface;

    protected Vector3 groundStandCapsuleCenter, groundCrouchCapsuleCenter;

    public CharacterMovement()
    {
        movementState = new CharacterMovementStateImpl(CharacterMovementState.NONE);
        m_previousGroundResult = CharacterMovementGroundResult.invalid;
        m_groundResult = CharacterMovementGroundResult.invalid;
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

    protected virtual void UpdateMovementState()
    {
        CharacterMovementState newState = new CharacterMovementStateImpl();

        if (m_groundResult.isValid)
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
                if (charInputs.jump)
                {
                    newState.current = CharacterMovementState.GROUND_STAND_JUMP;
                }
                else if (charInputs.move.magnitude == 0)
                {
                    newState.current = CharacterMovementState.GROUND_STAND_IDLE;
                }
                else if (charInputs.walk)
                {
                    newState.current = CharacterMovementState.GROUND_STAND_WALK;
                }
                else if (charInputs.sprint && charInputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && charInputs.moveAngle < groundStandSprintRightAngleMax)
                {
                    newState.current = CharacterMovementState.GROUND_STAND_SPRINT;
                }
                else
                {
                    newState.current = CharacterMovementState.GROUND_STAND_RUN;
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
        if (m_groundResult.isValid)
        {
            var deltaPosition = m_groundResult.collider.transform.position - m_groundResult.basePosition;
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

    protected bool CapsuleCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        return charCapsule.CapsuleCast(move, out smallHit, out bigHit);
    }

    protected Vector3 CapsuleMove(Vector3 remainingMove)
    {
        return CapsuleMove(remainingMove, out RaycastHit smallHit, out RaycastHit bigHit);
    }

    protected Vector3 CapsuleMove(Vector3 remainingMove, out RaycastHit hit)
    {
        var moved = CapsuleMove(remainingMove, out RaycastHit smallHit, out RaycastHit bigHit);
        hit = smallHit.collider ? smallHit : bigHit;

        return moved;
    }

    protected Vector3 CapsuleMove(Vector3 remainingMove, out RaycastHit hit, out Vector3 hitNormal)
    {
        var moved = CapsuleMove(remainingMove, out RaycastHit smallHit, out RaycastHit bigHit, out hitNormal);
        hit = smallHit.collider ? smallHit : bigHit;

        return moved;
    }

    protected Vector3 CapsuleMove(Vector3 remainingMove, out RaycastHit smallHit, out RaycastHit bigHit)
    {
        if (remainingMove.magnitude == 0f)
        {
            smallHit = new RaycastHit();
            bigHit = new RaycastHit();

            return Vector3.zero;
        }

        CapsuleCast(remainingMove, out smallHit, out bigHit);

        Vector3 direction = remainingMove.normalized;
        float distance = remainingMove.magnitude;

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

    protected Vector3 CapsuleMove(Vector3 remainingMove, out RaycastHit smallHit, out RaycastHit bigHit, out Vector3 hitNormal)
    {
        Vector3 moved = CapsuleMove(remainingMove, out smallHit, out bigHit);
        RaycastHit hit = smallHit.collider ? smallHit : bigHit;

        if (hit.collider)
        {
            Vector3 rayDirection = remainingMove.normalized;
            Vector3 rayOrigin = hit.point + (-rayDirection * k_recalculateNormalFallback);
            const float rayDistance = k_recalculateNormalFallback + k_recalculateNormalAddon;

            Physics.Raycast(rayOrigin, rayDirection, out RaycastHit rayHit,
                rayDistance, charCapsule.layerMask, charCapsule.triggerQuery);

            if (rayHit.collider && rayHit.collider == hit.collider)
            {
                hitNormal = rayHit.normal;
                return moved;
            }
        }

        hitNormal = Vector3.zero;
        return moved;
    }

    protected RaycastHit SmallBaseSphereCast(Vector3 move, float offsetValue = 0)
    {
        charCapsule.CalculateSmallCapsuleGeometry(out var topSphere,
            out var baseSphere, out var radius);

        Vector3 offset = -move.normalized * offsetValue;
        topSphere += offset;
        baseSphere += offset;
        move += -offset;

        Physics.SphereCast(baseSphere, radius, move.normalized, out RaycastHit hit,
            move.magnitude, charCapsule.layerMask, charCapsule.triggerQuery);

        if (hit.collider)
        {
            hit.distance = hit.distance - offsetValue;
        }

        return hit;
    }

    protected Vector3 CapsuleResolvePenetration()
    {
        return charCapsule.ResolvePenetrationForBigCapsule(k_collisionOffset);
    }

    protected void TeleportTo(Vector3 pos, Quaternion rot)
    {
        charCapsule.position = pos;
        charCapsule.rotation = rot;

        m_velocity = Vector3.zero;
        m_lastMove = Vector3.zero;
    }

    protected bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
    {
        return hit.RecalculateNormalUsingRaycast(out normal, charCapsule.layerMask, charCapsule.triggerQuery);
    }

    //////////////////////////////////////////////////////////////////
    /// Ground Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysGround()
    {
        // Calculate Speed
        float speed = 0;
        float acceleration = 0;

        switch (movementState.current)
        {
            case CharacterMovementState.GROUND_STAND_JUMP:
                m_currentStepUpHeight = groundStandStepUpHeight;
                m_currentStepDownDepth = groundStandStepDownDepth;
                m_currentSlopeUpAngle = groundStandSlopeUpAngle;
                acceleration = groundStandJumpForce;
                speed = groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE:
                m_currentStepUpHeight = groundStandStepUpHeight;
                m_currentStepDownDepth = groundStandStepDownDepth;
                m_currentSlopeUpAngle = groundStandSlopeUpAngle;
                acceleration = groundStandIdleAcceleration;
                speed = groundStandIdleSpeed;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                m_currentStepUpHeight = groundStandStepUpHeight;
                m_currentStepDownDepth = groundStandStepDownDepth;
                m_currentSlopeUpAngle = groundStandSlopeUpAngle;
                acceleration = groundStandWalkAcceleration;
                speed = groundStandWalkSpeed;
                break;

            case CharacterMovementState.GROUND_STAND_RUN:
                m_currentStepUpHeight = groundStandStepUpHeight;
                m_currentStepDownDepth = groundStandStepDownDepth;
                m_currentSlopeUpAngle = groundStandSlopeUpAngle;
                acceleration = groundStandRunAcceleration;
                speed = groundStandRunSpeed;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                m_currentStepUpHeight = groundStandStepUpHeight;
                m_currentStepDownDepth = groundStandStepDownDepth;
                m_currentSlopeUpAngle = groundStandSlopeUpAngle;
                acceleration = groundStandSprintAcceleration;
                speed = groundStandSprintSpeed;
                break;

            case CharacterMovementState.GROUND_CROUCH_JUMP:
                m_currentStepUpHeight = groundCrouchStepUpHeight;
                m_currentStepDownDepth = groundCrouchStepDownDepth;
                m_currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                acceleration = groundCrouchJumpForce;
                speed = groundCrouchJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                m_currentStepUpHeight = groundCrouchStepUpHeight;
                m_currentStepDownDepth = groundCrouchStepDownDepth;
                m_currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                acceleration = groundCrouchIdleAcceleration;
                speed = groundCrouchIdleSpeed;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                m_currentStepUpHeight = groundCrouchStepUpHeight;
                m_currentStepDownDepth = groundCrouchStepDownDepth;
                m_currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                acceleration = groundCrouchWalkAcceleration;
                speed = groundCrouchWalkSpeed;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                m_currentStepUpHeight = groundCrouchStepUpHeight;
                m_currentStepDownDepth = groundCrouchStepDownDepth;
                m_currentSlopeUpAngle = groundCrouchSlopeUpAngle;
                acceleration = groundCrouchRunAcceleration;
                speed = groundCrouchRunSpeed;
                break;

            default:
                m_currentStepUpHeight = 0;
                m_currentStepDownDepth = 0;
                m_currentSlopeUpAngle = 0;
                acceleration = 0;
                speed = 0;
                break;
        }

        // Calculate move input
        Vector3 moveInput = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        moveInput = Quaternion.Euler(0, charView.turnAngle, 0) * moveInput.normalized;

        Vector3 deltaMove = moveInput * speed * Time.deltaTime;
        // deltaMove = Vector3.MoveTowards(m_lastMove, deltaMove, acceleration * Time.deltaTime);

        deltaMove.y = movementState.isGroundStandJump ? speed : 0;

        // Perform move
        var previousPosition = charCapsule.position;
        GroundMove(deltaMove);

        m_lastMove = deltaMove;
        m_velocity = charCapsule.position - previousPosition;
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        GroundResizeCapsule();

        if (originalMove.magnitude < groundMinMoveDistance)
        {
            CapsuleResolvePenetration();
            return;
        }

        Vector3 remainingMove = originalMove;
        remainingMove.y = 0f;

        var stepUpHeight = 0f;
        var canStepUp = true;
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
                    if (GroundCanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal) == false)
                    {

                        charCapsule.localPosition = positionBeforeStepUp;
                        remainingMove = moveBeforeStepUp;
                        canStepUp = false;

                        continue;
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

                stepUpHeight = CapsuleMove(character.up * m_currentStepUpHeight).magnitude;

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

        GroundStepDown();
        CapsuleResolvePenetration();

        charCapsule.PerformMove();
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
            if (m_currentMaintainVelocityOnSurface)
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
        if (m_currentMaintainVelocityAlongSurface)
        {
            if (slideMove.magnitude.IsEqualToZero() == false)
            {
                slideMove = slideMove.normalized * remainingMoveSize;
            }
        }

        remainingMove = slideMove;
        return true;
    }

    protected virtual bool GroundStepDown()
    {
        if (m_currentStepDownDepth <= 0)
            return false;

        CapsuleResolvePenetration();

        var moved = CapsuleMove(character.down * m_currentStepDownDepth, out RaycastHit hit, out Vector3 hitNormal);

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

            float maxSlopeAngle = Math.Clamp(m_currentSlopeUpAngle,
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
        RaycastHit hit = SmallBaseSphereCast(character.down * depth, .02f);
        result = new CharacterMovementGroundResult();

        if (GroundCheck(hit.collider) == false)
        {
            return false;
        }

        result.collider = hit.collider;
        result.direction = character.down;
        result.distance = hit.distance;

        Vector3 leftDirection = Quaternion.Euler(0, -90, 0) * result.direction;
        Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, leftDirection).normalized;
        result.angle = 90f - Vector3.SignedAngle(result.direction, obstacleForward, -leftDirection);

        result.basePosition = result.collider.transform.position;
        result.baseRotation = result.collider.transform.rotation;

        result.edgeDistance = default;

        return true;
    }

    protected virtual void GroundUpdateResult()
    {
        m_previousGroundResult = m_groundResult;
        GroundCast(groundCheckDepth, out m_groundResult);
    }

    //////////////////////////////////////////////////////////////////
    /// Air Physics
    //////////////////////////////////////////////////////////////////

    [SerializeField] protected float gravityMultiplier = .01f;
    protected virtual void PhysAir()
    {
        float deltaTime = Time.deltaTime;
        float moveSpeed = airMoveSpeed;
        float mass = character.scaledMass;
        float gravitySpeed = airGravityAcceleration * mass * deltaTime * deltaTime * gravityMultiplier;

        // Calculate Movement
        Vector3 moveInput = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        moveInput = Quaternion.Euler(0, charView.turnAngle, 0) * moveInput;

        Vector3 deltaMove = moveInput * moveSpeed * Time.deltaTime; // horizontal movement
        deltaMove = m_velocity + (character.down * -gravitySpeed); // add gravity

        // Perform move
        var previousPosition = charCapsule.position;
        AirMove(deltaMove);

        m_lastMove = deltaMove;
        m_velocity = charCapsule.position - previousPosition;
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        CapsuleMove(originalMove);
        CapsuleResolvePenetration();
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
