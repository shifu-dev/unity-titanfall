using System;
using UnityEngine;

public interface IMovementData
{
    float CurrentMoveSpeed { get; }
    float GroundCheckDepth { get; }
    float GroundStandWalkSpeed { get; }
    float GroundStandRunSpeed { get; }
    float GroundStandSprintSpeed { get; }
    float GroundStandSprintLeftAngleMax { get; }
    float GroundStandSprintRightAngleMax { get; }
    float GroundStandJumpSpeed { get; }
    float GroundStandStepUpHeight { get; }
    float GroundStandStepDownHeight { get; }
    float GroundStandWalkableAngleZ { get; }
    float GroundCrouchWalkSpeed { get; }
    float GroundCrouchRunSpeed { get; }
    float GroundCrouchJumpSpeed { get; }
    float GroundCrouchStepUpHeight { get; }
    float GroundCrouchStepDownHeight { get; }
    float GroundCrouchWalkableAngleZ { get; }
    bool GroundCrouchAutoRiseToStandSprint { get; }
    float GroundProneMoveSpeed { get; }
    bool GroundProneAutoRiseToStandSprint { get; }
}

public interface IMovementInputs
{
	bool IsValid { get; }
    Vector3 MoveInputVector { get; }
    float MoveInputAngle { get; }
    bool WantsToWalk { get; }
    bool WantsToSprint { get; }
    bool WantsToJump { get; }
    bool WantsToCrouch { get; }
    bool WantsToProne { get; }
}

[RequireComponent(typeof(CharacterCapsule))]
public class CharacterMovement : MonoBehaviour
{
    protected Character Character;

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

    public IMovementInputs MovementInputs;

    protected bool PhysIsOnGround = false;

    public void Init(Character character)
    {
        Character = character;
        MovementInputs = Character.CharacterInputs;
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
    }
}