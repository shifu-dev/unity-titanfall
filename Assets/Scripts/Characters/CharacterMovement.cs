using System;
using UnityEngine;

public class CharacterMovement : CharacterBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Constants
    //////////////////////////////////////////////////////////////////

    protected const float k_collisionOffset = 0;

    protected const float k_minGroundStandStepUpPercent = 0f;
    protected const float k_maxGroundStandStepUpPercent = 49f;
    protected const float k_minGroundStandStepDownPercent = 0f;
    protected const float k_maxGroundStandStepDownPercent = 49f;
    protected const float k_minGroundStandSlopeUpAngle = 0f;
    protected const float k_maxGroundStandSlopeUpAngle = 89f;
    protected const float k_minGroundStandSlopeDownAngle = 0f;
    protected const float k_maxGroundStandSlopeDownAngle = -89f;

    protected const float k_minGroundCrouchStepUpPercent = 0f;
    protected const float k_maxGroundCrouchStepUpPercent = 49f;
    protected const float k_minGroundCrouchStepDownPercent = 0f;
    protected const float k_maxGroundCrouchStepDownPercent = 50f;
    protected const float k_minGroundCrouchSlopeUpAngle = 0f;
    protected const float k_maxGroundCrouchSlopeUpAngle = 89f;
    protected const float k_minGroundCrouchSlopeDownAngle = 0f;
    protected const float k_maxGroundCrouchSlopeDownAngle = -89f;

    protected const float k_minGroundProneSlopeUpAngle = 0f;
    protected const float k_maxGroundProneSlopeUpAngle = 89f;
    protected const float k_minGroundProneSlopeDownAngle = 0f;
    protected const float k_maxGroundProneSlopeDownAngle = -89f;

    protected const float k_minGroundTestDepth = 0.01f;
    protected const float k_maxGroundTestDepth = 0.5f;
    protected const uint k_maxMoveIterations = 10;

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Character Data

    protected CharacterCapsule charCapsule { get => character.charCapsule; }
    protected CharacterInputs charInputs { get => character.charInputs; }
    protected CharacterView charView { get => character.charView; }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Movement State

    public CharacterMovementState movementState { get; protected set; }
    public bool physIsOnGround { get; protected set; }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// GroundData

    [Range(k_minGroundTestDepth, k_maxGroundTestDepth)]
    [SerializeField] protected float m_groundCheckDepth;

    [SerializeField] protected LayerMask m_groundLayer;

    [Min(0f)]
    [SerializeField] protected float m_groundMinMoveDistance;

    //////////////////////////////////////////////////////////////////
    /// Ground Stand Data

    [SerializeField] protected float m_groundStandWalkSpeed;
    public float groundStandWalkSpeed => m_groundStandWalkSpeed;

    [SerializeField] protected float m_groundStandRunSpeed;
    public float groundStandRunSpeed => m_groundStandRunSpeed;

    [SerializeField] protected float m_groundStandSprintSpeed;
    public float groundStandSprintSpeed => m_groundStandSprintSpeed;

    [SerializeField, Range(-90, 0)] protected float m_groundStandSprintLeftAngleMax;
    public float groundStandSprintLeftAngleMax => m_groundStandSprintLeftAngleMax;

    [SerializeField, Range(0, 90)] protected float m_groundStandSprintRightAngleMax;
    public float groundStandSprintRightAngleMax => m_groundStandSprintRightAngleMax;

    [SerializeField] protected float m_groundStandJumpSpeed;
    public float groundStandJumpSpeed => m_groundStandJumpSpeed;

    // Ground Stand StepUp
    [Range(k_minGroundStandStepUpPercent, k_maxGroundStandStepUpPercent)]
    [SerializeField] protected float m_groundStandStepUpPercent;
    public float groundStandStepUpPercent
    {
        get => m_groundStandStepUpPercent;
        set
        {
            m_groundStandStepUpPercent = Math.Clamp(value,
                k_minGroundStandStepUpPercent, k_maxGroundStandStepUpPercent);
        }
    }
    public float groundStandStepUpHeight
    {
        get => charCapsule.height * (m_groundStandStepUpPercent / 100);
        set
        {
            groundStandStepUpPercent = (value / charCapsule.height) * 100;
        }
    }

    // Ground Stand StepDown
    [Range(k_minGroundStandStepDownPercent, k_maxGroundStandStepDownPercent)]
    [SerializeField] protected float m_groundStandStepDownPercent;
    public float groundStandStepDownPercent
    {
        get => m_groundStandStepDownPercent;
        set
        {
            m_groundStandStepDownPercent = Math.Clamp(value,
                k_minGroundStandStepDownPercent, k_maxGroundStandStepDownPercent);
        }
    }
    public float groundStandStepDownDepth
    {
        get => charCapsule.height * (m_groundStandStepDownPercent / 100);
        set
        {
            groundStandStepDownPercent = (value / charCapsule.height) * 100;
        }
    }

    // Ground Stand SlopeUp
    [Range(k_minGroundStandSlopeUpAngle, k_maxGroundStandSlopeUpAngle)]
    [SerializeField] protected float m_groundStandSlopeUpAngle;
    public float groundStandSlopeUpAngle
    {
        get => m_groundStandSlopeUpAngle;
        set
        {
            m_groundStandSlopeUpAngle = Math.Clamp(value,
                k_minGroundStandSlopeUpAngle, k_maxGroundStandSlopeUpAngle);
        }
    }

    // Ground Stand SlopeDown
    [Range(k_minGroundStandSlopeDownAngle, k_maxGroundStandSlopeDownAngle)]
    [SerializeField] protected float m_groundStandSlopeDownAngle;
    public float groundStandSlopeDownAngle
    {
        get => m_groundStandSlopeDownAngle;
        set
        {
            m_groundStandSlopeDownAngle = Math.Clamp(value,
                k_minGroundStandSlopeDownAngle, k_maxGroundStandSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_groundStandMaintainVelocityOnSurface;
    [SerializeField] protected bool m_groundStandMaintainVelocityAlongSurface;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Crouch Data

    [SerializeField] protected float m_groundCrouchWalkSpeed;
    [SerializeField] protected float m_groundCrouchRunSpeed;
    [SerializeField] protected float m_groundCrouchJumpSpeed;

    // Ground Crouch StepUp
    [Range(k_minGroundCrouchStepUpPercent, k_maxGroundCrouchStepUpPercent)]
    [SerializeField] protected float m_groundCrouchStepUpPercent;
    public float groundCrouchStepUpPercent
    {
        get => m_groundCrouchStepUpPercent;
        set
        {
            m_groundCrouchStepUpPercent = Math.Clamp(value,
                k_minGroundCrouchStepUpPercent, k_maxGroundCrouchStepUpPercent);
        }
    }
    public float groundCrouchStepUpHeight
    {
        get => charCapsule.height * (m_groundCrouchStepUpPercent / 100);
        set
        {
            groundCrouchStepUpPercent = (value / charCapsule.height) * 100;
        }
    }

    // Ground Crouch StepDown
    [Range(k_minGroundCrouchStepDownPercent, k_maxGroundCrouchStepDownPercent)]
    [SerializeField] protected float m_groundCrouchStepDownPercent;
    public float groundCrouchStepDownPercent
    {
        get => m_groundCrouchStepDownPercent;
        set
        {
            m_groundCrouchStepDownPercent = Math.Clamp(value,
                k_minGroundCrouchStepDownPercent, k_maxGroundCrouchStepDownPercent);
        }
    }
    public float groundCrouchStepDownDepth
    {
        get => charCapsule.height * (m_groundCrouchStepDownPercent / 100);
        set
        {
            groundCrouchStepDownPercent = (value / charCapsule.height) * 100;
        }
    }

    // Ground Crouch SlopeUp
    [Range(k_minGroundCrouchSlopeUpAngle, k_maxGroundCrouchSlopeUpAngle)]
    [SerializeField] protected float m_groundCrouchSlopeUpAngle;
    public float groundCrouchSlopeUpAngle
    {
        get => m_groundCrouchSlopeUpAngle;
        set
        {
            m_groundCrouchSlopeUpAngle = Math.Clamp(value,
                k_minGroundCrouchSlopeUpAngle, k_maxGroundCrouchSlopeUpAngle);
        }
    }

    // Ground Crouch SlopeDown
    [Range(k_minGroundCrouchSlopeDownAngle, k_maxGroundCrouchSlopeDownAngle)]
    [SerializeField] protected float m_groundCrouchSlopeDownAngle;
    public float groundCrouchSlopeDownAngle
    {
        get => m_groundCrouchSlopeDownAngle;
        set
        {
            m_groundCrouchSlopeDownAngle = Math.Clamp(value,
                k_minGroundCrouchSlopeDownAngle, k_maxGroundCrouchSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_groundCrouchMaintainVelocityOnSurface;
    [SerializeField] protected bool m_groundCrouchMaintainVelocityAlongSurface;
    [SerializeField] protected bool m_groundCrouchAutoRiseToStandSprint;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Prone Data

    [SerializeField] protected float m_groundProneMoveSpeed;

    // Ground Prone SlopeUp
    [Range(k_minGroundProneSlopeUpAngle, k_maxGroundProneSlopeUpAngle)]
    [SerializeField] protected float m_groundProneSlopeUpAngle;
    public float groundProneSlopeUpAngle
    {
        get => m_groundProneSlopeUpAngle;
        set
        {
            m_groundProneSlopeUpAngle = Math.Clamp(value,
                k_minGroundProneSlopeUpAngle, k_maxGroundProneSlopeUpAngle);
        }
    }

    // Ground Prone SlopeDown
    [Range(k_minGroundProneSlopeDownAngle, k_maxGroundProneSlopeDownAngle)]
    [SerializeField] protected float m_groundProneSlopeDownAngle;
    public float groundProneSlopeDownAngle
    {
        get => m_groundProneSlopeDownAngle;
        set
        {
            m_groundProneSlopeDownAngle = Math.Clamp(value,
                k_minGroundProneSlopeDownAngle, k_maxGroundProneSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_groundProneMaintainVelocityOnSurface;
    [SerializeField] protected bool m_groundProneMaintainVelocityAlongSurface;
    [SerializeField] protected bool m_groundProneAutoRiseToStandSprint;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Air Data

    [SerializeField] protected float m_airHelperSpeed;
    [SerializeField] protected Vector3 m_airGravityDirection;
    [SerializeField] protected float m_airGravityAcceleration;

    //////////////////////////////////////////////////////////////////
    // Constructors
    //////////////////////////////////////////////////////////////////

    public CharacterMovement()
    {
        movementState = new CharacterMovementState(CharacterMovementState.Enum.UNKNOWN);
        physIsOnGround = false;

        /// GroundData
        m_groundCheckDepth = 0.05f;
        m_groundLayer = 0;
        m_groundMinMoveDistance = 0.001f;

        /// Ground Stand Data
        m_groundStandWalkSpeed = 10;
        m_groundStandRunSpeed = 15;
        m_groundStandSprintSpeed = 25;
        m_groundStandSprintLeftAngleMax = -45;
        m_groundStandSprintRightAngleMax = 45;
        m_groundStandJumpSpeed = 40;
        m_groundStandStepUpPercent = 10;
        m_groundStandStepDownPercent = 10;
        m_groundStandSlopeUpAngle = 45;
        m_groundStandSlopeDownAngle = 55;
        m_groundStandMaintainVelocityOnSurface = false;
        m_groundStandMaintainVelocityAlongSurface = false;

        /// Ground Crouch Data
        m_groundCrouchWalkSpeed = 8;
        m_groundCrouchRunSpeed = 12;
        m_groundCrouchJumpSpeed = 50;
        m_groundCrouchStepUpPercent = 5;
        m_groundCrouchStepDownPercent = 5;
        m_groundCrouchSlopeUpAngle = 55;
        m_groundCrouchSlopeDownAngle = 65;
        m_groundCrouchMaintainVelocityOnSurface = false;
        m_groundCrouchMaintainVelocityAlongSurface = false;
        m_groundCrouchAutoRiseToStandSprint = true;

        /// Ground Prone Data
        m_groundProneMoveSpeed = 4;
        m_groundProneSlopeUpAngle = 15;
        m_groundProneSlopeDownAngle = 25;
        m_groundProneMaintainVelocityOnSurface = false;
        m_groundProneMaintainVelocityAlongSurface = false;
        m_groundProneAutoRiseToStandSprint = true;

        /// Air Data
        m_airHelperSpeed = 1f;
        m_airGravityDirection = Vector3.zero;
        m_airGravityAcceleration = 9.8f;
    }

    //////////////////////////////////////////////////////////////////
    /// Update Calls
    //////////////////////////////////////////////////////////////////

    public override void OnUpdateCharacter()
    {
        UpdatePhysicsData();
        UpdateMovementState();
        UpdatePhysicsState();
    }

    //////////////////////////////////////////////////////////////////
    /// Physics State
    //////////////////////////////////////////////////////////////////

    protected virtual void UpdatePhysicsData()
    {
        GroundCheck();
    }

    protected virtual void UpdateMovementState()
    {
        var state = movementState;

        if (physIsOnGround)
        {
            if (charInputs.crouch)
            {
            }
            else if (charInputs.prone)
            {
            }
            else    // Standing
            {
                if (charInputs.move.normalized.magnitude == 0)
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_IDLE;
                }
                else if (charInputs.walk)
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_WALK;
                }
                else if (charInputs.sprint && charInputs.moveAngle > groundStandSprintLeftAngleMax
                                                      && charInputs.moveAngle < groundStandSprintRightAngleMax)
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_SPRINT;
                }
                else
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_RUN;
                }
            }
        }
        else
        {
            state.State = CharacterMovementState.Enum.AIR_IDLE;
        }

        movementState = state;
    }

    protected virtual void UpdatePhysicsState()
    {
        if (movementState.IsGrounded())
        {
            PhysGround();
        }
        else
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

    protected RaycastHit SmallCapsuleCast(Vector3 move)
    {
        return charCapsule.SmallCapsuleCast(move);
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
        // Calculate speed
        float speed = 0;

        if (charInputs.crouch)
        {
            speed = m_groundCrouchRunSpeed;
        }
        else if (charInputs.prone)
        {
            speed = m_groundProneMoveSpeed;
        }
        else    // Standing
        {
            if (charInputs.move.normalized.magnitude == 0)
            {
                speed = 0;
            }
            else if (charInputs.walk)
            {
                speed = m_groundStandWalkSpeed;
            }
            else if (charInputs.sprint && charInputs.moveAngle.IsInRange(
                groundStandSprintLeftAngleMax, groundStandSprintRightAngleMax))
            {
                speed = m_groundStandSprintSpeed;
            }
            else
            {
                speed = m_groundStandRunSpeed;
            }
        }
        speed = speed / 4;

        // Calculate Movement
        Vector3 moveInputVector = new Vector3(charInputs.move.x, 0, charInputs.move.y);
        Vector3 normalizedMoveInputVector = moveInputVector.normalized;
        Vector3 directionalMoveVector = Quaternion.Euler(0, charView.turnAngle, 0) * normalizedMoveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform move
        GroundMove(deltaMove);
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

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 remainingMove = originalMove;

        bool canRunIteration(uint it) => it < k_maxMoveIterations ||
            remainingMove.magnitude == 0 || remainingMove.magnitude < m_groundMinMoveDistance;

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

        GroundStepDown(originalMove, ref remainingMove);
        ResolvePenetrationForSmallCapsule();
    }

    protected virtual float GroundStepUp(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
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

        float stepUpHeight = groundStandStepUpHeight;
        if (obstacleHeight > stepUpHeight)
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
        float stepDown = groundStandStepDownDepth;
        Vector3 down = charCapsule.down;
        RaycastHit stepDownHit = SmallCapsuleCast(down * stepDown);
        if (stepDownHit.collider == null)
        {
            return false;
        }

        charCapsule.Move(down * (stepDownHit.distance - 0.025f));
        return true;
    }

    protected virtual bool GroundMoveOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        Vector3 moveVectorLeft = (Quaternion.Euler(0, -90, 0) * remainingMove).normalized;
        Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, moveVectorLeft).normalized;
        float slopeAngle = 90f - Vector3.SignedAngle(remainingMove.normalized, obstacleForward, -moveVectorLeft);
        slopeAngle = Math.Max(slopeAngle, 0);

        float walkableAngle = m_groundStandSlopeUpAngle;
        if (slopeAngle > walkableAngle)
        {
            return false;
        }

        Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnSlopes = m_groundStandMaintainVelocityOnSurface;
        if (maintainVelocityOnSlopes)
        {
            slopeMove = slopeMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slopeMove;
        return true;
    }

    protected virtual bool GroundMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnWallSlides = m_groundStandMaintainVelocityOnSurface;
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
        float speed = m_airHelperSpeed;

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
        float speed = m_airGravityAcceleration * Time.deltaTime * mass * 0.02f;
        speed = charCapsule.velocity.y + speed;

        Vector3 gravityDirection = character.down;

        SmallCapsuleMove(gravityDirection * speed);
        ResolvePenetrationForSmallCapsule();
    }
}