using System;
using UnityEngine;

public interface IMovementInputs
{
    bool IsValid { get; }
    float MoveInputAngle { get; }
    Vector3 MoveInputVector { get; }
    Vector3 LookInputVector { get; }
    bool WantsToWalk { get; }
    bool WantsToSprint { get; }
    bool WantsToJump { get; }
    bool WantsToCrouch { get; }
    bool WantsToProne { get; }
}

[RequireComponent(typeof(CharacterCollision))]
public class CharacterMovement : MonoBehaviour
{
    public Character Character { get => _Character; }

    [NonSerialized] protected Character _Character;

    public IMovementInputs MovementInputs;

    public CharacterMovementState MovementState;
    public bool IsOnGround;
    public float CurrentMoveSpeed;
    public float GroundCheckDepth;
    public float GroundStandWalkSpeed;
    public float GroundStandRunSpeed;
    public float GroundStandSprintSpeed;
    public float GroundStandSprintLeftAngleMax;
    public float GroundStandSprintRightAngleMax;
    public float GroundStandJumpSpeed;
    public float GroundStandStepUpHeight;
    public float GroundStandStepDownHeight;
    public float GroundStandWalkableAngleZ;
    public float GroundCrouchWalkSpeed;
    public float GroundCrouchRunSpeed;
    public float GroundCrouchJumpSpeed;
    public float GroundCrouchStepUpHeight;
    public float GroundCrouchStepDownHeight;
    public float GroundCrouchWalkableAngleZ;
    public bool GroundCrouchAutoRiseToStandSprint;
    public float GroundProneMoveSpeed;
    public bool GroundProneAutoRiseToStandSprint;

    protected bool PhysIsOnGround = false;
    protected Vector3 Velocity = Vector3.zero;
    protected Vector3 LastPosition = Vector3.zero;

    public void Init(Character character)
    {
        _Character = character;
        MovementInputs = _Character.CharacterInputs;
    }

    public void UpdateImpl()
    {
        UpdatePhysicsData();
        UpdateMovementState();
        UpdatePhysicsState();
    }

    protected virtual void UpdatePhysicsData()
    {
        PhysIsOnGround = true;
    }

    protected virtual void UpdateMovementState()
    {
        if (PhysIsOnGround)
        {
            if (MovementInputs.WantsToCrouch)
            {
            }
            else if (MovementInputs.WantsToProne)
            {
            }
            else	// Standing
            {
                if (MovementInputs.MoveInputVector.normalized.magnitude == 0)
                {
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_IDLE;
                }
                else if (MovementInputs.WantsToWalk)
                {
                    MovementState.State = CharacterMovementState.Enum.GROUND_STAND_WALK;
                }
                else if (MovementInputs.WantsToSprint && MovementInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                      && MovementInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
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

        if (MovementInputs.WantsToCrouch)
        {
            speed = GroundCrouchRunSpeed;
        }
        else if (MovementInputs.WantsToProne)
        {
            speed = GroundProneMoveSpeed;
        }
        else    // Standing
        {
            if (MovementInputs.MoveInputVector.normalized.magnitude == 0)
            {
                speed = 0;
            }
            else if (MovementInputs.WantsToWalk)
            {
                speed = GroundStandWalkSpeed;
            }
            else if (MovementInputs.WantsToSprint && MovementInputs.MoveInputAngle > GroundStandSprintLeftAngleMax
                                                  && MovementInputs.MoveInputAngle < GroundStandSprintRightAngleMax)
            {
                speed = GroundStandSprintSpeed;
            }
            else
            {
                speed = GroundStandRunSpeed;
            }
        }

        // Calculate Movement
        Vector3 moveInputVector = new Vector3(MovementInputs.MoveInputVector.x, 0, MovementInputs.MoveInputVector.y);
        Vector3 normalizedMoveInputVector = moveInputVector.normalized;
        Vector3 directionalMoveVector = Quaternion.Euler(0, MovementInputs.LookInputVector.y, 0) * normalizedMoveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform Collision checks
        var rb = Character.CharacterCollision.Rigidbody;
        LastPosition = rb.position;

        MoveAlongSurface(rb, deltaMove, 10);

        Velocity = LastPosition - rb.position;
    }

    // returns total distance moved
    float MoveAlongSurface(Rigidbody rb, Vector3 deltaMove, uint iterations)
    {
        // if (iterations == 0 || rb == null || deltaMove.magnitude < 0.01f)
        if (iterations == 0 || rb == null)
            return 0;

        bool hit = rb.SweepTest(deltaMove.normalized, out RaycastHit hitInfo, deltaMove.magnitude, QueryTriggerInteraction.Ignore);
        if (hit)
        {
            // move to the hit position
            Vector3 movedDelta = deltaMove * hitInfo.distance;
            Vector3 remainingDeltaMove = deltaMove - deltaMove * hitInfo.distance;

            // check if can step up

            Debug.DrawLine(rb.position, rb.position + movedDelta, Color.green);
            rb.MovePosition(rb.position + movedDelta);

            // try sliding through the surface
            Vector3 surfaceDeltaMove = Vector3.ProjectOnPlane(remainingDeltaMove, hitInfo.normal);
            return movedDelta.magnitude + MoveAlongSurface(rb, surfaceDeltaMove, --iterations);
        }

        // no collision occurred, so we made the complete move
        rb.MovePosition(rb.position + deltaMove);
        return deltaMove.magnitude;
    }
}