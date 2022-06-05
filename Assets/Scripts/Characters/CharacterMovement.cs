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

    [SerializeField, ReadOnly] private CharacterMovementStateImpl m_movementState;
    public virtual CharacterMovementState movementState
    {
        get => m_movementState;
        protected set => m_movementState = new CharacterMovementStateImpl(value.current, value.weight);
    }

    public bool physIsOnGround { get; protected set; }

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
        groundCrouchIdleAcceleration;

    protected bool groundStandMaintainVelocityOnSurface, groundStandMaintainVelocityAlongSurface,
        groundCrouchAutoRiseToStandSprint, groundCrouchMaintainVelocityOnSurface,
        groundCrouchMaintainVelocityAlongSurface;

    protected Vector3 groundStandCapsuleCenter, groundCrouchCapsuleCenter;

    public CharacterMovement()
    {
        movementState = new CharacterMovementStateImpl(CharacterMovementState.NONE);
        physIsOnGround = false;
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

        if (physIsOnGround)
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
        GroundCheck();
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
        charCapsule.CalculateSmallCapsuleGeometry(out var topSphere, out var baseSphere, out var radius);
        Vector3 offset = -move.normalized * offsetValue;
        topSphere += offset;
        baseSphere += offset;
        move += -offset;

        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude, charCapsule.layerMask, charCapsule.triggerQuery);
        return hit;
    }

    protected RaycastHit SmallBaseSphereCast(Vector3 move)
    {
        return charCapsule.SmallBaseSphereCast(move);
    }

    protected RaycastHit SmallCapsuleMove(Vector3 move)
    {
        return charCapsule.CapsuleMove(move, 0);
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
        deltaMove = Vector3.MoveTowards(m_velocity, deltaMove, acceleration * Time.deltaTime);

        // Perform move
        var previousPosition = charCapsule.position;
        GroundMove(deltaMove);

        m_lastMove = deltaMove;
        m_velocity = charCapsule.position - previousPosition;
    }

    protected virtual void GroundCheck()
    {
        physIsOnGround = true;
        // RaycastHit hit = SmallBaseSphereCast(character.down * m_groundCheckDepth);
        // if (hit.collider == null)
        // {
        //     physIsOnGround = false;
        //     return;
        // }

        // var hitLayer = hit.collider.gameObject.layer;
        // physIsOnGround = m_groundLayer.Contains(hitLayer);
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

    protected virtual void GroundJump(float height)
    {
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

        bool canRunIteration(uint it) => it < k_maxMoveIterations ||
            remainingMove.magnitude == 0 || remainingMove.magnitude < groundMinMoveDistance;

        for (uint it = 0; canRunIteration(it); it++)
        {
            ResolvePenetrationForSmallCapsule();

            RaycastHit sweepHit = SmallCapsuleMove(remainingMove);
            if (sweepHit.collider == null)
            {
                break;
            }

            // pending move vector after we collided with something
            remainingMove = remainingMove - (originalMove.normalized * sweepHit.distance);

            // treat obstacle as slope first
            if (GroundMoveOnSurface(originalMove, ref remainingMove, sweepHit))
            {
                continue;
            }

            float stepUp = GroundStepUp(originalMove, ref remainingMove, sweepHit);

            if (GroundMoveAlongSurface(originalMove, ref remainingMove, sweepHit))
            {
                SmallCapsuleMove(charCapsule.down * stepUp);
                continue;
            }

            SmallCapsuleMove(charCapsule.down * stepUp);
        }

        ResolvePenetrationForSmallCapsule();
        GroundStepDown(originalMove, ref remainingMove);
        ResolvePenetrationForSmallCapsule();

        charCapsule.PerformMove();
    }

    protected virtual float GroundStepUp(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (m_currentStepUpHeight <= 0 || hit.collider == null || remainingMove == Vector3.zero)
            return 0;

        Vector3 capCenter_HitPoint = Vector3.ProjectOnPlane(hit.point - charCapsule.center, charCapsule.right);
        float hitAngleFromCapCenter = Vector3.SignedAngle(charCapsule.forward, capCenter_HitPoint.normalized, charCapsule.right);

        // check if hit point is below than capsule center
        if (hitAngleFromCapCenter < 0)
        {
            return 0;
        }

        Vector3 basePoint = charCapsule.basePosition;
        Vector3 basePoint_ObstacleTop = hit.point - basePoint;
        Vector3 obstacleHeightVector = Vector3.ProjectOnPlane(basePoint_ObstacleTop, charCapsule.up);
        float obstacleHeight = obstacleHeightVector.magnitude;

        if (obstacleHeight > m_currentStepUpHeight)
            return 0;

        Vector3 stepUpVector = charCapsule.up * obstacleHeight;
        RaycastHit stepUpHit = SmallCapsuleCast(stepUpVector);

        if (stepUpHit.collider == null)
        {
            charCapsule.Move(stepUpVector);
            return stepUpVector.magnitude;
        }

        return 0;
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

    protected virtual bool GroundMoveOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (m_currentSlopeUpAngle <= 0 || hit.collider == null || remainingMove == Vector3.zero)
            return false;

        Vector3 moveVectorLeft = (Quaternion.Euler(0, -90, 0) * remainingMove).normalized;
        Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, moveVectorLeft).normalized;
        float slopeAngle = 90f - Vector3.SignedAngle(remainingMove.normalized, obstacleForward, -moveVectorLeft);
        slopeAngle = Math.Max(slopeAngle, 0);

        if (slopeAngle > m_currentSlopeUpAngle)
        {
            return false;
        }

        Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnSlopes = groundStandMaintainVelocityOnSurface;
        if (maintainVelocityOnSlopes)
        {
            slopeMove = slopeMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slopeMove;
        return true;
    }

    protected virtual bool GroundMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
            return false;

        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnWallSlides = groundStandMaintainVelocityOnSurface;
        if (maintainVelocityOnWallSlides)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
        return true;
    }

    //////////////////////////////////////////////////////////////////
    /// Air Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysAir()
    {
        // Calculate speed
        // float speed = m_airHelperSpeed;
        float speed = 0;

        // Calculate Movement
        Vector3 moveInputVector = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        Vector3 directionalMoveVector = Quaternion.Euler(0, 0, 0) * moveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform move
        AirMove(deltaMove);
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        float mass = character.ScaledMass;
        // float speed = m_airGravityAcceleration * Time.deltaTime * mass * 0.02f;
        float speed = 0 * Time.deltaTime * mass * 0.02f;
        speed = charCapsule.velocity.y + speed;

        Vector3 gravityDirection = character.down;

        SmallCapsuleMove(gravityDirection * speed);
        ResolvePenetrationForSmallCapsule();
    }
}