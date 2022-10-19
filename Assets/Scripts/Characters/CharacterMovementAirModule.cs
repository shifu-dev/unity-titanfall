using UnityEngine;

public class CharacterMovementAirModule : CharacterMovementModule
{
    protected const uint AIR_MAX_MOVE_ITERATIONS = 10;
    protected const float GRAVITY_MULTIPLIER = .05f;

    public CharacterMovementAirModule(CharacterMovementAirModuleSource source)
    {
        // cache Data from CharacterDataSource
        if (source is not null)
        {
            mGravityAcceleration = source.gravityAcceleration;
            mGravityMaxSpeed = source.gravityMaxSpeed;
            mMinMoveDistance = source.minMoveDistance;
            mMoveSpeed = source.moveSpeed;
            mMoveAcceleration = source.moveAcceleration;
            mJumpPower = source.jumpPower;
            mMaxJumpCount = source.maxJumpCount;
        }
    }

    protected virtual void PhysAir()
    {
        AirCalculateValues();

        Vector3 charUp = mCharUp;
        Vector3 charForward = mCharacter.forward;
        Vector3 charRight = mCharacter.right;
        float mass = mCharacter.mass;
        float gravitySpeed = mGravityAcceleration * mass * mDeltaTime * mDeltaTime * GRAVITY_MULTIPLIER;

        Vector3 vel = mVelocity * mDeltaTime;
        Vector3 velH = Vector3.ProjectOnPlane(vel, charUp);
        Vector3 velV = vel - velH;

        Vector3 moveV = velV + (charUp * gravitySpeed);
        Vector3 moveH = velH;

        // Vector3 moveHX = Vector3.ProjectOnPlane(moveH, charForward);
        // Vector3 moveHZ = moveH - moveHX;
        // // processed move input
        // Vector3 moveInputRaw = _move;
        // Vector3 moveInput = new Vector3(moveInputRaw.x, 0f, moveInputRaw.y);
        // moveInput = Quaternion.Euler(0f, mCharView.turnAngle, 0f) * moveInput;
        // moveInput = character.rotation * moveInput;

        // // helping movement in mAir
        // Vector3 move_help_h = mCurrentMoveSpeed * moveInput * mDeltaTime;
        // Vector3 move_help_h_x = Vector3.ProjectOnPlane(move_help_h, charForward);
        // Vector3 move_help_h_z = move_help_h - move_help_h_x;

        // if (move_help_h_x.magnitude > 0f)
        // {
        //     if (moveHX.normalized == move_help_h_x.normalized)
        //     {
        //         if (move_help_h_x.magnitude > moveHX.magnitude)
        //         {
        //             moveHX = move_help_h_x;
        //         }
        //     }
        //     else
        //     {
        //         moveHX = move_help_h_x;
        //     }
        // }

        // if (move_help_h_z.magnitude > 0f)
        // {
        //     if (moveHZ.normalized == move_help_h_z.normalized)
        //     {
        //         if (move_help_h_z.magnitude > moveHZ.magnitude)
        //         {
        //             moveHZ = move_help_h_z;
        //         }
        //     }
        //     else
        //     {
        //         moveHZ = move_help_h_z;
        //     }
        // }

        // moveH = moveHX + moveHZ;

        // process character jump
        if (mInputJump && mCurrentJumpCount < mMaxJumpCount)
        {
            mCurrentJumpCount++;

            if (mCurrentMaintainVelocityOnJump == false)
            {
                moveV = Vector3.zero;
            }

            moveV = charUp * mCurrentJumpPower;
        }

        Vector3 move = moveH + moveV;

        AirMove(move);
    }

    protected virtual void AirCalculateValues()
    {
        mCurrentMoveAccel = mMoveAcceleration;
        mCurrentMoveSpeed = mMoveSpeed;
        mCurrentJumpPower = mJumpPower;
        mCurrentMaxJumpCount = mMaxJumpCount;
        mCurrentMinMoveDist = mMinMoveDistance;

        // TODO: add this field in data asset
        mCurrentMaintainVelocityOnJump = false;
    }

    protected virtual void AirMove(Vector3 move)
    {
        Vector3 lastPosition = mCapsule.position;
        Vector3 remainingMove = move;

        for (int i = 0; i < AIR_MAX_MOVE_ITERATIONS; i++)
        {
            remainingMove -= CapsuleMove(remainingMove, out RaycastHit hit, out Vector3 hitNormal);

            if (hit.collider is null)
            {
                // no collision, so end the move
                remainingMove = Vector3.zero;
                break;
            }

            AirMoveAlongSurface(move, ref remainingMove, hit, hitNormal);
        }

        Vector3 moved = mCapsule.position - lastPosition;
        SetVelocityByMove(moved);
    }

    protected virtual void AirMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (hit.collider is null || remainingMove == global::UnityEngine.Vector3.zero)
            return;

        RecalculateNormalIfZero(hit, ref hitNormal);

        // if (GroundCanStandOn(hit, hitNormal, out float slopeAngle))
        // {
        //     remainingMove = Vector3.zero;
        //     mCanGround = true;
        //     return;
        // }

        // hit.normal gives normal respective to capsule's body,
        // useful for sliding off on corners
        Vector3 slideMove = Vector3.ProjectOnPlane(remainingMove, hit.normal);
        if (mCurrentMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
    }

    protected float mGravityAcceleration;
    protected float mGravityMaxSpeed;
    protected float mMinMoveDistance;
    protected float mMoveSpeed;
    protected float mMoveAcceleration;
    protected float mJumpPower;

    protected uint mMaxJumpCount;

    protected bool mInputJump;

    protected float mCurrentMinMoveDist = 0;
    protected float mCurrentMoveSpeed = 0;
    protected float mCurrentMoveAccel = 0;
    protected float mCurrentJumpPower = 0;
    protected float mCurrentStepUpHeight = 0;
    protected float mCurrentStepDownDepth = 0;
    protected float mCurrentSlopeUpAngle = 0;
    protected float mCurrentSlopeDownAngle = 0;
    protected uint mCurrentJumpCount = 0;
    protected uint mCurrentMaxJumpCount = 0;
    protected bool mCurrentMaintainVelocityOnJump = false;
    protected bool mCurrentMaintainVelocityOnSurface = true;
    protected bool mCurrentMaintainVelocityAlongSurface = true;
}