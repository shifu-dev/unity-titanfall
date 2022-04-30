using System;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class CharacterMovement : MonoBehaviour
{
    public Character Character { get; protected set; }
    public CharacterCapsule CharacterCapsule { get => Character.CharacterCapsule; }
    public CharacterInputs CharacterInputs { get => Character.CharacterInputs; }

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
        // Vector3 directionalMoveVector = Quaternion.Euler(0, MovementInputs.LookInputVector.y, 0) * normalizedMoveInputVector;
        Vector3 directionalMoveVector = Quaternion.Euler(0, 0, 0) * normalizedMoveInputVector;
        Vector3 deltaMove = directionalMoveVector * speed * Time.deltaTime;

        // Perform move

        GroundMove(deltaMove, 10);

        Velocity = LastPosition - CharacterCapsule.GetWorldCenter;
        Velocity = Velocity / Time.deltaTime;
    }

    // returns total distance moved
    float GroundMove(Vector3 deltaMove, uint iterations, float moveThreshold = 0.01f)
    {
        CharacterCapsule charCapsule = CharacterCapsule;
        Vector3 nextDeltaMove = deltaMove;
        float totalDistMoved = 0;
        float maxSlopeAngle = GroundStandWalkableAngleZ;
        float maxStepUpHeight = GroundStandStepUpHeight;
        bool mantainVelocityOnSlopes = true;

        while (iterations > 0 && nextDeltaMove.magnitude >= moveThreshold)
        {
            RaycastHit hit = charCapsule.CapsuleMove(nextDeltaMove);
            totalDistMoved += hit.distance;

            // no collision occurred, so we made the complete move
            if (hit.IsHit() == false)
            {
                // Debug.Log("NoCollision");
                break;
            }

            // Debug.Log("Collision: " + hit.collider.name); 
            Vector3 remainingDeltaMove = nextDeltaMove - (nextDeltaMove.normalized * totalDistMoved);
            Debug.DrawLine(hit.point, hit.point + remainingDeltaMove * 100, Color.blue);

            // check if we can climb the slope
            // float hitAngle = Vector3.SignedAngle(charCapsule.GetWorldUpVector, hit.normal, charCapsule.GetWorldRightVector);
            float hitAngle = Vector3.Angle(charCapsule.GetWorldUpVector, hit.normal);
            if (hitAngle <= maxSlopeAngle)
            {
                Debug.LogWarning("SlopeAngle: " + hitAngle);
                Vector3 slopeDeltaMove = Vector3.ProjectOnPlane(remainingDeltaMove, hit.normal);
                if (mantainVelocityOnSlopes)
                {
                    slopeDeltaMove = slopeDeltaMove.normalized * remainingDeltaMove.magnitude;
                }

                nextDeltaMove = slopeDeltaMove;
                iterations--;
                continue;
            }

            double obstacleHeight = 0;
            {   // get obstacle height relative to character base
                Vector3 hypotenuseVector = hit.point - charCapsule.GetWorldBasePosition;
                Vector3 baseVector = Vector3.ProjectOnPlane(hypotenuseVector, charCapsule.GetWorldUpVector);
                obstacleHeight = Math.Sqrt(Math.Pow(hypotenuseVector.magnitude, 2) - Math.Pow(baseVector.magnitude, 2));
            }

            // check if we can step up the obstacle
            if (obstacleHeight <= maxStepUpHeight)
            {
                Debug.LogWarning("StepUp: " + obstacleHeight);
                RaycastHit stepUpHitInfo = charCapsule.CapsuleMoveNoHit(charCapsule.GetWorldUpVector * (float)obstacleHeight);
                if (stepUpHitInfo.collider)
                {
                    // We will perform the rest of the move in the next move
                    nextDeltaMove = remainingDeltaMove;
                }
                else
                {
                    nextDeltaMove = Vector3.zero;
                }

                iterations--;
                continue;
            }

            // try sliding through the surface
            GroundSlideAlongSurface(remainingDeltaMove, hit.normal, out RaycastHit slideHitInfo);
            iterations = 0;
        }

        return totalDistMoved;
    }

    float GroundSlideAlongSurface(Vector3 deltaMove, Vector3 surfaceNormal, out RaycastHit hitInfo, float moveThreshold = 0.01f)
    {
        Vector3 slideDeltaMove = Vector3.ProjectOnPlane(deltaMove, surfaceNormal);
        if (slideDeltaMove.magnitude < moveThreshold)
        {
            hitInfo = new RaycastHit();
            return 0;
        }

        hitInfo = CharacterCapsule.CapsuleMove(slideDeltaMove);
        return hitInfo.distance;
    }

    float GroundStepUpFrom(float stepUpHeight, Vector3 deltaMove, Vector3 surfaceNormal, RaycastHit hitInfo, float moveThreshold = 0.01f)
    {
        CharacterCapsule charCapsule = CharacterCapsule;
        // Vector3 remainingDeltaMove = deltaMove - (deltaMove.normalized * totalDistMoved);

        // get obstacle height relative to character base
        Vector3 hypotenuseVector = hitInfo.point - charCapsule.GetWorldBasePosition;
        Vector3 baseVector = Vector3.ProjectOnPlane(hypotenuseVector, charCapsule.GetWorldUpVector);
        double obstacleHeight = Math.Sqrt(Math.Pow(hypotenuseVector.magnitude, 2) - Math.Pow(baseVector.magnitude, 2));

        // check if we can step up the obstacle
        if (obstacleHeight <= stepUpHeight)
        {
            RaycastHit stepUpHitInfo = charCapsule.CapsuleMove(charCapsule.GetWorldUpVector * (float)obstacleHeight);
            return stepUpHitInfo.distance;
        }

        return 0;
    }
}