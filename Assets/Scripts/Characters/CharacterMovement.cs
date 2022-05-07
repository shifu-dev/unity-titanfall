using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterMovement : MonoBehaviour
{
    public const float k_MinGroundStandStepUpPercent = 0f;
    public const float k_MaxGroundStandStepUpPercent = 49f;
    public const float k_MinGroundStandStepDownPercent = 0f;
    public const float k_MaxGroundStandStepDownPercent = 50f;
    public const float k_MinGroundStandSlopeUpAngle = 0f;
    public const float k_MaxGroundStandSlopeUpAngle = 89f;
    public const float k_MinGroundStandSlopeDownAngle = 0f;
    public const float k_MaxGroundStandSlopeDownAngle = -89f;
    public const float k_GroundTestDepth = 0.01f;
    public const uint k_MaxMoveIterations = 10;

    public Character Character { get; protected set; }
    public CharacterCapsule CharacterCapsule { get => Character.CharacterCapsule; }
    public CharacterInputs CharacterInputs { get => Character.CharacterInputs; }

    public CharacterMovementState MovementState;

    // GroundData
    public bool IsOnGround;
    public float GroundCheckDepth;
    public float GroundStandWalkSpeed;
    public float GroundStandRunSpeed;
    public float GroundStandSprintSpeed;
    public float GroundStandSprintLeftAngleMax;
    public float GroundStandSprintRightAngleMax;
    public float GroundStandJumpSpeed;

    // Ground Stand StepUp
    protected float m_GroundStandStepUpPercent;
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
    protected float m_GroundStandStepDownPercent;
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
    protected float m_GroundStandSlopeUpAngle;
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
    protected float m_GroundStandSlopeDownAngle;
    public float GroundStandSlopeDownAngle
    {
        get => m_GroundStandSlopeDownAngle;
        set
        {
            m_GroundStandSlopeDownAngle = Math.Clamp(value,
                k_MinGroundStandSlopeDownAngle, k_MaxGroundStandSlopeDownAngle);
        }
    }

    public bool GroundStandMaintainVelocityOnSlopes = true;
    public bool GroundStandMaintainVelocityOnWallSlides = true;
    public float GroundCrouchWalkSpeed;
    public float GroundCrouchRunSpeed;
    public float GroundCrouchJumpSpeed;
    public float GroundCrouchStepUpHeight;
    public float GroundCrouchStepDownHeight;
    public float GroundCrouchWalkableAngleZ;
    public bool GroundCrouchAutoRiseToStandSprint;
    public float GroundProneMoveSpeed;
    public bool GroundProneAutoRiseToStandSprint;
    protected float m_MinMoveDistance = 0f;

    public bool PhysIsOnGround { get; protected set; }

    public CharacterMovement()
    {
        PhysIsOnGround = false;
    }

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

    protected virtual void UpdatePhysicsData()
    {
        PhysIsOnGround = true;
    }

    protected virtual void UpdateMovementState()
    {
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
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_IDLE;
                }
                else if (CharacterInputs.WantsToWalk)
                {
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_WALK;
                }
                else if (CharacterInputs.WantsToSprint && CharacterInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                      && CharacterInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
                {
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_SPRINT;
                }
                else
                {
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_RUN;
                }
            }
        }
    }

    protected virtual void UpdatePhysicsState()
    {
        if (MovementState.IsGrounded())
        {
            PhysGround();
        }
    }

    protected virtual void PhysGround()
    {
        // Calculate speed
        float speed = 0;

        if (CharacterInputs.WantsToCrouch)
        {
            speed = GroundCrouchRunSpeed;
        }
        else if (CharacterInputs.WantsToProne)
        {
            speed = GroundProneMoveSpeed;
        }
        else    // Standing
        {
            if (CharacterInputs.MoveInputVector.normalized.magnitude == 0)
            {
                speed = 0;
            }
            else if (CharacterInputs.WantsToWalk)
            {
                speed = GroundStandWalkSpeed;
            }
            else if (CharacterInputs.WantsToSprint && CharacterInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                  && CharacterInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
            {
                speed = GroundStandSprintSpeed;
            }
            else
            {
                speed = GroundStandRunSpeed;
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
        if (hit.IsHit())
        {
            IsOnGround = hit.collider.tag.Contains("Ground");
        }
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 remainingMove = originalMove;

        bool canRunIteration(uint it) => it < k_MaxMoveIterations ||
            remainingMove.magnitude == 0 || remainingMove.magnitude < m_MinMoveDistance;

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

        if (stepUpHit.IsHit())
            return false;

        CharacterCapsule.Move(stepUpVector);
        Debug.Log("Ground StepUp" + " | StepUpCapacity: " + stepUpHeight + " | ObstacleHeight: "
            + obstacleHeight + " | StepUpHeight: " + stepUpVector.y);

        return true;
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
            bool maintainVelocityOnSlopes = GroundStandMaintainVelocityOnSlopes;
            if (maintainVelocityOnSlopes)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            // Debug.Log("GroundSlopeMove | RemainingMove: " + remainingMove + " | GameObject: " + hit.collider.name);
            return;
        }

        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMove.magnitude, hit.normal);
        bool maintainVelocityOnWallSlides = GroundStandMaintainVelocityOnSlopes;
        if (maintainVelocityOnWallSlides)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
        // Debug.Log("GroundSlideAlong | RemainingMove: " + remainingMove + " | GameObject: " + hit.collider.name);
    }
}