using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterMovement : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    /// Constants
    //////////////////////////////////////////////////////////////////

    protected const float k_MinGroundStandStepUpPercent = 0f;
    protected const float k_MaxGroundStandStepUpPercent = 49f;
    protected const float k_MinGroundStandStepDownPercent = 0f;
    protected const float k_MaxGroundStandStepDownPercent = 50f;
    protected const float k_MinGroundStandSlopeUpAngle = 0f;
    protected const float k_MaxGroundStandSlopeUpAngle = 89f;
    protected const float k_MinGroundStandSlopeDownAngle = 0f;
    protected const float k_MaxGroundStandSlopeDownAngle = -89f;

    protected const float k_MinGroundCrouchStepUpPercent = 0f;
    protected const float k_MaxGroundCrouchStepUpPercent = 49f;
    protected const float k_MinGroundCrouchStepDownPercent = 0f;
    protected const float k_MaxGroundCrouchStepDownPercent = 50f;
    protected const float k_MinGroundCrouchSlopeUpAngle = 0f;
    protected const float k_MaxGroundCrouchSlopeUpAngle = 89f;
    protected const float k_MinGroundCrouchSlopeDownAngle = 0f;
    protected const float k_MaxGroundCrouchSlopeDownAngle = -89f;

    protected const float k_MinGroundProneSlopeUpAngle = 0f;
    protected const float k_MaxGroundProneSlopeUpAngle = 89f;
    protected const float k_MinGroundProneSlopeDownAngle = 0f;
    protected const float k_MaxGroundProneSlopeDownAngle = -89f;

    protected const float k_GroundTestDepth = 0.01f;
    protected const uint k_MaxMoveIterations = 10;

    //////////////////////////////////////////////////////////////////
    /// Variables
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Character Data

    protected Character Character { get; set; }
    protected CharacterCapsule CharacterCapsule { get => Character.CharacterCapsule; }
    protected CharacterInputs CharacterInputs { get => Character.CharacterInputs; }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Movement State

    public CharacterMovementState MovementState { get; protected set; }
    public bool PhysIsOnGround { get; protected set; }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// GroundData

    [SerializeField] protected float m_GroundCheckDepth;
    [SerializeField] protected float m_GroundMinMoveDistance;

    //////////////////////////////////////////////////////////////////
    /// Ground Stand Data

    [SerializeField] protected float m_GroundStandWalkSpeed;
    public float GroundStandWalkSpeed => m_GroundStandWalkSpeed;

    [SerializeField] protected float m_GroundStandRunSpeed;
    public float GroundStandRunSpeed => m_GroundStandRunSpeed;

    [SerializeField] protected float m_GroundStandSprintSpeed;
    public float GroundStandSprintSpeed => m_GroundStandSprintSpeed;

    [SerializeField] protected float m_GroundStandSprintLeftAngleMax;
    public float GroundStandSprintLeftAngleMax => m_GroundStandSprintLeftAngleMax;

    [SerializeField] protected float m_GroundStandSprintRightAngleMax;
    public float GroundStandSprintRightAngleMax => m_GroundStandSprintRightAngleMax;

    [SerializeField] protected float m_GroundStandJumpSpeed;
    public float GroundStandJumpSpeed => m_GroundStandJumpSpeed;

    // Ground Stand StepUp
    [Range(k_MinGroundStandStepUpPercent, k_MaxGroundStandStepUpPercent)]
    [SerializeField] protected float m_GroundStandStepUpPercent;
    public float GroundStandStepUpPercent
    {
        get => m_GroundStandStepUpPercent;
        set
        {
            m_GroundStandStepUpPercent = Math.Clamp(value,
                k_MinGroundStandStepUpPercent, k_MaxGroundStandStepUpPercent);
        }
    }
    public float GroundStandStepUpHeight
    {
        get => CharacterCapsule.GetHeight * (m_GroundStandStepUpPercent / 100);
        set
        {
            GroundStandStepUpPercent = (value / CharacterCapsule.GetHeight) * 100;
        }
    }

    // Ground Stand StepDown
    [Range(k_MinGroundStandStepDownPercent, k_MaxGroundStandStepDownPercent)]
    [SerializeField] protected float m_GroundStandStepDownPercent;
    public float GroundStandStepDownPercent
    {
        get => m_GroundStandStepDownPercent;
        set
        {
            m_GroundStandStepDownPercent = Math.Clamp(value,
                k_MinGroundStandStepDownPercent, k_MaxGroundStandStepDownPercent);
        }
    }
    public float GroundStandStepDownDepth
    {
        get => CharacterCapsule.GetHeight * (m_GroundStandStepDownPercent / 100);
        set
        {
            GroundStandStepDownPercent = (value / CharacterCapsule.GetHeight) * 100;
        }
    }

    // Ground Stand SlopeUp
    [Range(k_MinGroundStandSlopeUpAngle, k_MaxGroundStandSlopeUpAngle)]
    [SerializeField] protected float m_GroundStandSlopeUpAngle;
    public float GroundStandSlopeUpAngle
    {
        get => m_GroundStandSlopeUpAngle;
        set
        {
            m_GroundStandSlopeUpAngle = Math.Clamp(value,
                k_MinGroundStandSlopeUpAngle, k_MaxGroundStandSlopeUpAngle);
        }
    }

    // Ground Stand SlopeDown
    [Range(k_MinGroundStandSlopeDownAngle, k_MaxGroundStandSlopeDownAngle)]
    [SerializeField] protected float m_GroundStandSlopeDownAngle;
    public float GroundStandSlopeDownAngle
    {
        get => m_GroundStandSlopeDownAngle;
        set
        {
            m_GroundStandSlopeDownAngle = Math.Clamp(value,
                k_MinGroundStandSlopeDownAngle, k_MaxGroundStandSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_GroundStandMaintainVelocityOnSlopes;
    [SerializeField] protected bool m_GroundStandMaintainVelocityOnWallSlides;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Crouch Data

    [SerializeField] protected float m_GroundCrouchWalkSpeed;
    [SerializeField] protected float m_GroundCrouchRunSpeed;
    [SerializeField] protected float m_GroundCrouchJumpSpeed;

    // Ground Crouch StepUp
    [Range(k_MinGroundCrouchStepUpPercent, k_MaxGroundCrouchStepUpPercent)]
    [SerializeField] protected float m_GroundCrouchStepUpPercent;
    public float GroundCrouchStepUpPercent
    {
        get => m_GroundCrouchStepUpPercent;
        set
        {
            m_GroundCrouchStepUpPercent = Math.Clamp(value,
                k_MinGroundCrouchStepUpPercent, k_MaxGroundCrouchStepUpPercent);
        }
    }
    public float GroundCrouchStepUpHeight
    {
        get => CharacterCapsule.GetHeight * (m_GroundCrouchStepUpPercent / 100);
        set
        {
            GroundCrouchStepUpPercent = (value / CharacterCapsule.GetHeight) * 100;
        }
    }

    // Ground Crouch StepDown
    [Range(k_MinGroundCrouchStepDownPercent, k_MaxGroundCrouchStepDownPercent)]
    [SerializeField] protected float m_GroundCrouchStepDownPercent;
    public float GroundCrouchStepDownPercent
    {
        get => m_GroundCrouchStepDownPercent;
        set
        {
            m_GroundCrouchStepDownPercent = Math.Clamp(value,
                k_MinGroundCrouchStepDownPercent, k_MaxGroundCrouchStepDownPercent);
        }
    }
    public float GroundCrouchStepDownDepth
    {
        get => CharacterCapsule.GetHeight * (m_GroundCrouchStepDownPercent / 100);
        set
        {
            GroundCrouchStepDownPercent = (value / CharacterCapsule.GetHeight) * 100;
        }
    }

    // Ground Crouch SlopeUp
    [Range(k_MinGroundCrouchSlopeUpAngle, k_MaxGroundCrouchSlopeUpAngle)]
    [SerializeField] protected float m_GroundCrouchSlopeUpAngle;
    public float GroundCrouchSlopeUpAngle
    {
        get => m_GroundCrouchSlopeUpAngle;
        set
        {
            m_GroundCrouchSlopeUpAngle = Math.Clamp(value,
                k_MinGroundCrouchSlopeUpAngle, k_MaxGroundCrouchSlopeUpAngle);
        }
    }

    // Ground Crouch SlopeDown
    [Range(k_MinGroundCrouchSlopeDownAngle, k_MaxGroundCrouchSlopeDownAngle)]
    [SerializeField] protected float m_GroundCrouchSlopeDownAngle;
    public float GroundCrouchSlopeDownAngle
    {
        get => m_GroundCrouchSlopeDownAngle;
        set
        {
            m_GroundCrouchSlopeDownAngle = Math.Clamp(value,
                k_MinGroundCrouchSlopeDownAngle, k_MaxGroundCrouchSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_GroundCrouchMaintainVelocityOnSlopes;
    [SerializeField] protected bool m_GroundCrouchMaintainVelocityOnWallSlides;
    [SerializeField] protected bool m_GroundCrouchAutoRiseToStandSprint;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Ground Prone Data

    [SerializeField] protected float m_GroundProneMoveSpeed;

    // Ground Prone SlopeUp
    [Range(k_MinGroundProneSlopeUpAngle, k_MaxGroundProneSlopeUpAngle)]
    [SerializeField] protected float m_GroundProneSlopeUpAngle;
    public float GroundProneSlopeUpAngle
    {
        get => m_GroundProneSlopeUpAngle;
        set
        {
            m_GroundProneSlopeUpAngle = Math.Clamp(value,
                k_MinGroundProneSlopeUpAngle, k_MaxGroundProneSlopeUpAngle);
        }
    }

    // Ground Prone SlopeDown
    [Range(k_MinGroundProneSlopeDownAngle, k_MaxGroundProneSlopeDownAngle)]
    [SerializeField] protected float m_GroundProneSlopeDownAngle;
    public float GroundProneSlopeDownAngle
    {
        get => m_GroundProneSlopeDownAngle;
        set
        {
            m_GroundProneSlopeDownAngle = Math.Clamp(value,
                k_MinGroundProneSlopeDownAngle, k_MaxGroundProneSlopeDownAngle);
        }
    }

    [SerializeField] protected bool m_GroundProneMaintainVelocityOnSlopes;
    [SerializeField] protected bool m_GroundProneMaintainVelocityOnWallSlides;
    [SerializeField] protected bool m_GroundProneAutoRiseToStandSprint;

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    /// Air Data

    [SerializeField] protected float m_AirHelperSpeed;
    [SerializeField] protected Vector3 m_AirGravityDirection;
    [SerializeField] protected float m_AirGravityAcceleration;

    //////////////////////////////////////////////////////////////////
    // Constructors
    //////////////////////////////////////////////////////////////////

    public CharacterMovement()
    {
        PhysIsOnGround = false;

        m_AirHelperSpeed = 1f;
        m_AirGravityDirection = Vector3.zero;
        m_AirGravityAcceleration = 9.8f;
    }

    //////////////////////////////////////////////////////////////////
    /// Update Calls
    //////////////////////////////////////////////////////////////////

    public void Init(Character character)
    {
        Character = character;
    }

    public void UpdateImpl()
    {
        // Debug.Log("CharacterMovement| Frame: " + Time.frameCount);
        UpdatePhysicsData();
        UpdateMovementState();
        UpdatePhysicsState();
    }

    public void FixedUpdateImpl()
    {
    }

    //////////////////////////////////////////////////////////////////
    /// Physics State
    //////////////////////////////////////////////////////////////////

    protected virtual void UpdatePhysicsData()
    {
        CheckForGround();
    }

    protected virtual void UpdateMovementState()
    {
        var state = MovementState;

        if (PhysIsOnGround)
        {
            if (CharacterInputs.WantsToCrouch)
            {
            }
            else if (CharacterInputs.WantsToProne)
            {
            }
            else	// Standing
            {
                if (CharacterInputs.MoveInputVector.normalized.magnitude == 0)
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_IDLE;
                }
                else if (CharacterInputs.WantsToWalk)
                {
                    state.State = CharacterMovementState.Enum.GROUND_STAND_WALK;
                }
                else if (CharacterInputs.WantsToSprint && CharacterInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                      && CharacterInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
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

        MovementState = state;
    }

    protected virtual void UpdatePhysicsState()
    {
        if (MovementState.IsGrounded())
        {
            PhysGround();
        }
        else
        {
            PhysAir();
        }
    }

    //////////////////////////////////////////////////////////////////
    /// Ground Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysGround()
    {
        // Calculate speed
        float speed = 0;

        if (CharacterInputs.WantsToCrouch)
        {
            speed = m_GroundCrouchRunSpeed;
        }
        else if (CharacterInputs.WantsToProne)
        {
            speed = m_GroundProneMoveSpeed;
        }
        else    // Standing
        {
            if (CharacterInputs.MoveInputVector.normalized.magnitude == 0)
            {
                speed = 0;
            }
            else if (CharacterInputs.WantsToWalk)
            {
                speed = m_GroundStandWalkSpeed;
            }
            else if (CharacterInputs.WantsToSprint && CharacterInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                  && CharacterInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
            {
                speed = m_GroundStandSprintSpeed;
            }
            else
            {
                speed = m_GroundStandRunSpeed;
            }
        }

        // Calculate Movement
        Vector3 moveInputVector = new Vector3(CharacterInputs.MoveInputVector.x, 0, CharacterInputs.MoveInputVector.y);
        Vector3 normalizedMoveInputVector = moveInputVector.normalized;
        Vector3 directionalMoveVector = Quaternion.Euler(0, 0, 0) * normalizedMoveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform move
        GroundMove(deltaMove);
    }

    protected virtual void CheckForGround()
    {
        RaycastHit hit = CharacterCapsule.SmallBaseSphereCast(Character.GetDown * k_GroundTestDepth);
        if (hit.collider == null)
        {
            PhysIsOnGround = false;
            return;
        }

        var hitLayer = hit.collider.gameObject.layer;
        var hitLayerName = LayerMask.LayerToName(hitLayer);
        PhysIsOnGround = hitLayerName == "Ground";
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 remainingMove = originalMove;

        bool canRunIteration(uint it) => it < k_MaxMoveIterations ||
            remainingMove.magnitude == 0 || remainingMove.magnitude < m_GroundMinMoveDistance;

        // Debug.Log(Time.frameCount + " | CharacterMovement | Move: " + originalMove + " | Iterations: " + k_MaxMoveIterations);
        for (uint it = 0; canRunIteration(it); it++)
        {
            // Debug.Log(it + " | RemainingMove: " + remainingMove);
            RaycastHit sweepHit = CharacterCapsule.CapsuleMove(remainingMove, 0f);
            if (sweepHit.collider == null)
            {
                CharacterCapsule.ResolvePenetration();
                remainingMove = Vector3.zero;
                break;
            }

            // pending move vector after we collided with something
            remainingMove = remainingMove - (originalMove.normalized * sweepHit.distance);

            // try stepup
            if (GroundStepUp(originalMove, ref remainingMove, sweepHit))
            {
                // Perform the rest of the move in the next loop
                // This way we can step up again
                continue;
            }

            GroundMoveAlongSurface(originalMove, ref remainingMove, sweepHit);
            // Debug.Break();
        }
    }

    protected virtual bool GroundStepUp(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null || remainingMove == Vector3.zero)
            return false;

        // check if hit point is below than capsule center
        Vector3 capCenter_HitPoint = Vector3.ProjectOnPlane(hit.point - CharacterCapsule.GetCenter, CharacterCapsule.GetRightVector);
        float hitAngleFromCapCenter = Vector3.SignedAngle(CharacterCapsule.GetForwardVector, capCenter_HitPoint.normalized, CharacterCapsule.GetRightVector);
        // Debug.Log("GroundStepUp| HitAngle: " + hitAngleFromCapCenter);
        if (hitAngleFromCapCenter >= 0)
        {
            return false;
        }

        Vector3 basePoint = CharacterCapsule.GetBasePosition;
        Vector3 basePoint_ObstacleTop = hit.point - basePoint;
        Vector3 obstacleHeightVector = Vector3.ProjectOnPlane(basePoint_ObstacleTop, CharacterCapsule.GetUpVector);
        float obstacleHeight = obstacleHeightVector.magnitude;

        float stepUpHeightPercent = Math.Clamp(GroundStandStepUpPercent,
            k_MinGroundStandStepUpPercent, k_MaxGroundStandStepUpPercent);

        float stepUpHeight = CharacterCapsule.GetHeight * (stepUpHeightPercent / 100);

        if (obstacleHeight > stepUpHeight)
            return false;

        Vector3 stepUpVector = CharacterCapsule.GetUpVector * obstacleHeight;
        RaycastHit stepUpHit = CharacterCapsule.SmallCapsuleCast(stepUpVector);

        if (stepUpHit.collider == null)
        {
            CharacterCapsule.Move(stepUpVector);

            // Debug.Log("Ground StepUp" + " | StepUpCapacity: " + stepUpHeight + " | ObstacleHeight: "
            //     + obstacleHeight + " | StepUpHeight: " + stepUpVector.y);

            return true;
        }

        return false;
    }

    protected virtual bool GroundStepDown(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        if (hit.collider == null)
            return false;

        float stepDownDepth = GroundStandStepDownDepth;
        if (stepDownDepth >= 0)
            return false;

        RaycastHit stepDownHit = CharacterCapsule.SmallBaseSphereCast(Character.GetDown * stepDownDepth);
        if (stepDownHit.collider == null)
        {
            return false;
        }

        CharacterCapsule.Move(Character.GetDown * hit.distance);

        Debug.Log("GroundStepDown" + " | StepDownDepth: " + stepDownDepth +
            " | CurrentStepDown: " + Character.GetDown * hit.distance);

        return true;
    }

    protected virtual void GroundMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit)
    {
        Vector3 moveVectorLeft = (Quaternion.Euler(0, -90, 0) * remainingMove).normalized;
        Vector3 obstacleForward = Vector3.ProjectOnPlane(-hit.normal, moveVectorLeft).normalized;
        float slopeAngle = 90f - Vector3.SignedAngle(remainingMove.normalized, obstacleForward, -moveVectorLeft);
        slopeAngle = Math.Max(slopeAngle, 0);

        // Debug.Log("GameObject: " + hit.collider.name + " | SlopeAngle: " + slopeAngle);

        float walkableAngle = 0f;
        if (false && slopeAngle <= walkableAngle)
        {
            // walk
            Vector3 slopeMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
            bool maintainVelocityOnSlopes = m_GroundStandMaintainVelocityOnSlopes;
            if (maintainVelocityOnSlopes)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            // Debug.Log("GroundSlopeMove | RemainingMove: " + remainingMove + " | GameObject: " + hit.collider.name);
            return;
        }

        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnWallSlides = m_GroundStandMaintainVelocityOnSlopes;
        if (maintainVelocityOnWallSlides)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
        // Debug.Log("GroundSlideAlong | RemainingMove: " + remainingMove + " | GameObject: " + hit.collider.name);
    }

    //////////////////////////////////////////////////////////////////
    /// Air Physics
    //////////////////////////////////////////////////////////////////

    protected virtual void PhysAir()
    {
        // Calculate speed
        float speed = m_AirHelperSpeed;

        // Calculate Movement
        Vector3 moveInputVector = new Vector3(CharacterInputs.MoveInputVector.x, 0, CharacterInputs.MoveInputVector.y);
        Vector3 directionalMoveVector = Quaternion.Euler(0, 0, 0) * moveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform move
        AirMove(deltaMove);
    }

    protected virtual void AirMove(Vector3 originalMove)
    {
        GroundMove(originalMove);

        float mass = Character.ScaledMass;
        float speed = m_AirGravityAcceleration * Time.deltaTime;
        speed = CharacterCapsule.Velocity.y + speed;
        Vector3 gravityDirection = Quaternion.Euler(Character.GetDown) * m_AirGravityDirection;

        CharacterCapsule.CapsuleMove(gravityDirection * speed);
    }
}