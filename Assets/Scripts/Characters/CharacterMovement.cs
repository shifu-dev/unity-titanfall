using System;
using UnityEngine;

public class CharacterMovement : CharacterBehaviour
{
    #region EDITOR
#if UNITY_EDITOR

    public virtual void OnEditorEnable()
    {
        charCapsule = GetComponent<CharacterCapsule>();
    }

#endif
    #endregion

    protected const float k_collisionOffset = 0;
    protected const uint k_maxMoveIterations = 10;

    public CharacterDataAsset charDataAsset { get; protected set; }
    public CharacterCapsule charCapsule { get; protected set; }
    public CharacterInputs charInputs { get; protected set; }
    public CharacterView charView { get; protected set; }

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

        ResolvePenetrationForSmallCapsule();
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

    protected Collider[] SmallCapsuleOverlap()
    {
        return charCapsule.SmallCapsuleOverlap();
    }

    protected RaycastHit SmallCapsuleCast(Vector3 move, float offsetValue = 0)
    {
        charCapsule.CalculateSmallCapsuleGeometry(out var topSphere,
            out var baseSphere, out var radius);

        Vector3 offset = -move.normalized * offsetValue;
        topSphere += offset;
        baseSphere += offset;
        move += -offset;

        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit,
            move.magnitude, charCapsule.layerMask, charCapsule.triggerQuery);

        if (hit.collider)
        {
            hit.distance = hit.distance - offsetValue;
        }

        return hit;
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

    protected Vector3 SmallCapsuleMove(Vector3 move)
    {
        return charCapsule.SweepMove(move);
    }

    protected Vector3 SmallCapsuleMove(Vector3 move, out RaycastHit hit)
    {
        return charCapsule.SweepMove(move, out hit);
    }

    protected Vector3 ResolvePenetrationForSmallCapsule()
    {
        return charCapsule.ResolvePenetrationForSmallCapsule(k_collisionOffset);
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

    protected virtual CharacterMovementGroundResult GroundCheck()
    {
        return GroundCheck(groundCheckDepth);
    }

    protected virtual CharacterMovementGroundResult GroundCheck(float depth)
    {
        RaycastHit hit = SmallBaseSphereCast(character.down * depth, .02f);
        var result = new CharacterMovementGroundResult();

        if (hit.collider == null)
        {
            return result;
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

        return result;
    }

    protected void GroundUpdateResult()
    {
        m_previousGroundResult = m_groundResult;
        m_groundResult = GroundCheck(groundCheckDepth);
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

    protected virtual void GroundMove(Vector3 originalMove)
    {
        GroundResizeCapsule();

        if (originalMove.magnitude < groundMinMoveDistance)
        {
            ResolvePenetrationForSmallCapsule();
            return;
        }

        Vector3 remainingMove = originalMove;
        float? stepUp = null;

        ResolvePenetrationForSmallCapsule();

        bool canRunIteration(uint it) => it < k_maxMoveIterations || remainingMove.magnitude > 0;
        for (uint it = 0; canRunIteration(it); it++)
        {
            // move the capsule and calculate the pending move vector
            remainingMove -= SmallCapsuleMove(remainingMove, out RaycastHit sweepHit);
            if (sweepHit.collider == null)
            {
                break;
            }

            if (GroundSlideOnSurface(originalMove, ref remainingMove, sweepHit))
            {
                continue;
            }

            // step up the first time, we hit an obstacle
            if (stepUp.HasValue == false)
            {
                stepUp = GroundStepUp(originalMove, ref remainingMove, sweepHit);
                continue;
            }

            if (GroundSlideAlongSurface(originalMove, ref remainingMove, sweepHit))
            {
                continue;
            }

            remainingMove = Vector3.zero;
        }

        ResolvePenetrationForSmallCapsule();

        if (stepUp.HasValue)
        {
            SmallCapsuleMove(character.down * stepUp.Value, out RaycastHit stepUpRecoverHit);
            if (stepUpRecoverHit.collider == null)
            {
                GroundStepDown(originalMove, ref remainingMove);
            }
        }
        else
        {
            GroundStepDown(originalMove, ref remainingMove);
        }

        ResolvePenetrationForSmallCapsule();
        charCapsule.PerformMove();
    }

    protected virtual float GroundStepUp(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || m_currentStepUpHeight <= 0f || remainingMove == Vector3.zero)
            return 0f;

        // store current position before running simulation
        var prevPosition = charCapsule.localPosition;

        var stepUp = charCapsule.SweepMove(character.up * m_currentStepUpHeight, out var stepUpHit).magnitude;
        charCapsule.ResolvePenetrationForSmallCapsule();

        // store stepped up position 
        var stepUpPosition = charCapsule.localPosition;

        // we only step over obstacles if we can partially fit on it (i.e. fit the capsule's radius)
        // var horizontal = remainingMove * charCapsule.radius;
        var horizontal = remainingMove;

        // any obstacle ahead after we step up?
        charCapsule.SweepMove(horizontal, out RaycastHit moveHit);
        if (moveHit.collider)
        {
            float slopeAngle = Vector3.Angle(character.up, moveHit.normal);

            if (slopeAngle > m_currentSlopeUpAngle)
            {
                // restore previous position
                charCapsule.localPosition = prevPosition;
                return 0f;
            }
        }

        // any obstacle below after we step up and recover it?
        RaycastHit recoverHit = new RaycastHit();
        if (moveHit.collider == null)
        {
            charCapsule.SweepMove(character.down * stepUp, out recoverHit);
            if (recoverHit.collider)
            {
                float slopeAngle = Vector3.Angle(character.up, recoverHit.normal);

                if (slopeAngle > m_currentSlopeUpAngle)
                {
                    // restore previous position
                    charCapsule.localPosition = prevPosition;
                    return 0f;
                }
            }
        }

        charCapsule.localPosition = stepUpPosition;
        return stepUp;
    }

    protected virtual bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
    {
        return hit.RecalculateNormal(out normal, charCapsule.layerMask, charCapsule.triggerQuery);
    }

    protected virtual bool GroundSlideOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero && m_currentSlopeUpAngle <= 0)
            return false;

        RecalculateNormal(hit, out Vector3 hitNormal);
        float slopeAngle = Vector3.Angle(character.up, hitNormal);

        if (slopeAngle <= m_currentSlopeUpAngle)
        {
            // treat surface as slope
            Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
            if (m_currentMaintainVelocityOnSurface)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            return true;
        }

        return false;
    }

    protected virtual bool GroundSlideAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
            return false;

        // treat surface as wall
        Vector3 hitProject = Vector3.ProjectOnPlane(hit.normal, character.up);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hitProject);
        if (m_currentMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
        return true;
    }

    protected virtual void GroundSlideOnOrAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
            return;

        if (m_currentSlopeUpAngle > 0)
        {
            RecalculateNormal(hit, out Vector3 hitNormal);
            float slopeAngle = Vector3.Angle(character.up, hitNormal);

            if (slopeAngle <= m_currentSlopeUpAngle)
            {
                // treat surface as slope
                Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
                if (m_currentMaintainVelocityOnSurface)
                {
                    slopeMove = slopeMove.normalized * remainingMove.magnitude;
                }

                remainingMove = slopeMove;
                return;
            }
        }

        // treat surface as wall
        Vector3 hitProject = Vector3.ProjectOnPlane(hit.normal, character.up);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hitProject);
        if (m_currentMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
    }

    protected virtual bool GroundStepDown(Vector3 originalMove, ref Vector3 remainingMove)
    {
        if (m_currentStepDownDepth <= 0)
            return false;

        Vector3 down = charCapsule.down;
        RaycastHit stepDownHit = SmallCapsuleCast(down * m_currentStepDownDepth, 0.0001f);
        if (stepDownHit.collider == null)
        {
            return false;
        }

        charCapsule.Move(down * stepDownHit.distance);
        return true;
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
        SmallCapsuleMove(originalMove);
        ResolvePenetrationForSmallCapsule();
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
