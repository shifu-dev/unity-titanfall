using GameFramework.Extensions;
using System;
using UnityEngine;
using UnityEngine.Playables;

public class CharacterMovementGroundModule : CharacterMovementModule
{
    protected struct GroundResult
    {
        public static readonly GroundResult invalid = new GroundResult();

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
            get => collider is not null;
        }
    }

    protected enum MovementState : byte
    {
        Idle,
        Walking,
        Running,
        Sprinting
    }

    protected enum LocomotionState : byte
    {
        Standing,
        Crouching,
        Proning,
        Jumping
    }

    protected const uint MAX_MOVE_ITERATIONS = 10;
    protected const float MIN_MOVE_ALONG_SURFACE_TO_MAINTAIN_VELOCITY = .0001f;
    protected const float MIN_SLOPE_ANGLE = 0f;
    protected const float MAX_SLOPE_ANGLE = 89.9f;

    public CharacterMovementGroundModule(CharacterMovementGroundModuleSource source)
    {
        mPrevGroundResult = GroundResult.invalid;
        mGroundResult = GroundResult.invalid;

        // cache Data from CharacterDataSource
        if (source is not null)
        {
            mGroundCheckDepth = source.checkDepth;
            mGroundLayer = source.groundLayer;
            mMinMoveDistance = source.minMoveDistance;

            mStandDeacceleration = source.standIdleAcceleration;
            mStandWalkSpeed = source.standWalkSpeed;
            mStandWalkAcceleration = source.standWalkAcceleration;
            mStandRunSpeed = source.standRunSpeed;
            mStandRunAcceleration = source.standRunAcceleration;
            mStandSprintSpeed = source.standSprintSpeed;
            mStandSprintAcceleration = source.standSprintAcceleration;
            mStandSprintLeftAngleMax = source.standSprintLeftAngleMax;
            mStandSprintRightAngleMax = source.standSprintRightAngleMax;
            mStandJumpForce = source.standJumpForce;
            mStandStepUpPercent = source.standStepUpPercent;
            mStandStepDownPercent = source.standStepDownPercent;
            mStandSlopeUpAngle = source.standSlopeUpAngle;
            mStandSlopeDownAngle = source.standSlopeDownAngle;
            mStandMaintainVelocityOnSurface = source.standMaintainVelocityOnSurface;
            mStandMaintainVelocityAlongSurface = source.standMaintainVelocityAlongSurface;
            mStandCapsuleCenter = source.standCapsuleCenter;
            mStandCapsuleHeight = source.standCapsuleHeight;
            mStandCapsuleRadius = source.standCapsuleRadius;
            mStandToCrouchTransitionSpeed = source.standToCrouchTransitionSpeed;

            mCrouchDeacceleration = source.crouchIdleAcceleration;
            mCrouchWalkSpeed = source.crouchWalkSpeed;
            mCrouchWalkAcceleration = source.crouchWalkAcceleration;
            mCrouchRunSpeed = source.crouchRunSpeed;
            mCrouchRunAcceleration = source.crouchRunAcceleration;
            mCrouchJumpForce = source.crouchJumpForce;
            mCrouchStepUpPercent = source.crouchStepUpPercent;
            mCrouchStepDownPercent = source.crouchStepDownPercent;
            mCrouchSlopeUpAngle = source.crouchSlopeUpAngle;
            mCrouchSlopeDownAngle = source.crouchSlopeDownAngle;
            mCrouchMaintainVelocityOnSurface = source.crouchMaintainVelocityOnSurface;
            mCrouchMaintainVelocityAlongSurface = source.crouchMaintainVelocityAlongSurface;
            mCrouchCapsuleCenter = source.crouchCapsuleCenter;
            mCrouchCapsuleHeight = source.crouchCapsuleHeight;
            mCrouchCapsuleRadius = source.crouchCapsuleRadius;
            mCrouchToStandTransitionSpeed = source.crouchToStandTransitionSpeed;
        }
    }

    public override void OnLoaded(CharacterMovement charMovement)
    {
        base.OnLoaded(charMovement);

        if (mCharacter is not null)
        {
            mCharView = mCharacter.charView;
        }
    }

    public override void OnUnloaded(CharacterMovement charMovement)
    {
        base.OnUnloaded(charMovement);

        mCharView = null;
    }

    public override bool ShouldRun()
    {
        mBaseDeltaPosition = Vector3.zero;
        mBaseDeltaRotation = Vector3.zero;

        if (mGroundResult.isValid)
        {
            mBaseDeltaPosition = mGroundResult.collider.transform.position - mGroundResult.basePosition;
            mBaseDeltaRotation = mGroundResult.collider.transform.rotation.eulerAngles - mGroundResult.baseRotation.eulerAngles;
        }

        if (mBaseDeltaPosition != Vector3.zero || mBaseDeltaRotation != Vector3.zero)
        {
            return true;
        }

        return mGroundResult.isValid;
    }

    public override void RunPhysics()
    {
        base.RunPhysics();

        PullPhysicsData();
        RecoverFromBaseMove();

        UpdateValues();

        Vector3 moveInput = new Vector3(mInputMove.x, 0, mInputMove.y);
        moveInput = Quaternion.Euler(0, mCharView.turnAngle, 0) * moveInput.normalized;
        moveInput = mCharacter.rotation * moveInput;

        mVelocity = Vector3.ProjectOnPlane(mVelocity, mCharUp);

        Vector3 move = moveInput * mCurrentMoveSpeed * mDeltaTime;
        move = Vector3.MoveTowards(mVelocity * mDeltaTime, move, mCurrentMoveAccel * mDeltaTime);

        move += mCharUp * mCurrentJumpPower * mDeltaTime;

        GroundMove(move);

        mLastMovementState = mMovementState;
        mLastLocomotionState = mLocomotionState;

        PushPhysicsData();
    }

    //////////////////////////////////////////////////////////////////

    public virtual void Walk()
    {
        mMovementState = MovementState.Walking;
    }

    public virtual void Run()
    {
        mMovementState = MovementState.Running;
    }

    public virtual void Sprint()
    {
        mMovementState = MovementState.Sprinting;
    }

    public virtual void Stand()
    {
        mLocomotionState = LocomotionState.Standing;
    }

    public virtual void Crouch()
    {
        mLocomotionState = LocomotionState.Crouching;
    }

    public virtual void Prone()
    {
        mLocomotionState = LocomotionState.Proning;
    }

    public virtual void Jump()
    {
        mLocomotionState = LocomotionState.Jumping;
    }

    public virtual bool CanStandOnGround(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        return CanStandOn(hit, slopeNormal, out slopeAngle);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void UpdateValues()
    {
        switch (mLocomotionState)
        {
            case LocomotionState.Standing:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMaintainVelocityOnSurface = mStandMaintainVelocityOnSurface;
                mCurrentMaintainVelocityAlongSurface = mStandMaintainVelocityAlongSurface;
                mCurrentJumpPower = 0f;

                if (mLocomotionState == LocomotionState.Jumping)
                    mCurrentJumpPower = mStandJumpForce;

                switch (mMovementState)
                {
                    case MovementState.Idle:
                        mCurrentMoveAccel = mStandDeacceleration;
                        mCurrentMoveSpeed = 0;
                        break;

                    case MovementState.Walking:
                        mCurrentMoveAccel = mStandWalkAcceleration;
                        mCurrentMoveSpeed = mStandWalkSpeed;
                        break;

                    case MovementState.Running:
                        mCurrentMoveAccel = mStandRunAcceleration;
                        mCurrentMoveSpeed = mStandRunSpeed;
                        break;

                    case MovementState.Sprinting:
                        mCurrentMoveAccel = mStandSprintAcceleration;
                        mCurrentMoveSpeed = mStandSprintSpeed;
                        break;

                    default:
                        mCurrentMoveAccel = 0;
                        mCurrentMoveSpeed = 0;
                        break;
                }

                break;

            case LocomotionState.Crouching:
                mCurrentStepDownDepth = mCrouchStepDownDepth;
                mCurrentStepUpHeight = mCrouchStepUpHeight;
                mCurrentMaxSlopeUpAngle = mCrouchSlopeUpAngle;
                mCurrentSlopeDownAngle = mCrouchSlopeDownAngle;
                mCurrentMaintainVelocityOnSurface = mCrouchMaintainVelocityOnSurface;
                mCurrentMaintainVelocityAlongSurface = mCrouchMaintainVelocityAlongSurface;
                mCurrentJumpPower = 0f;

                if (mLocomotionState == LocomotionState.Jumping)
                    mCurrentJumpPower = mCrouchJumpForce;

                switch (mMovementState)
                {
                    case MovementState.Idle:
                        mCurrentMoveAccel = mCrouchDeacceleration;
                        mCurrentMoveSpeed = 0;
                        break;

                    case MovementState.Walking:
                        mCurrentMoveAccel = mCrouchWalkAcceleration;
                        mCurrentMoveSpeed = mCrouchWalkSpeed;
                        break;

                    case MovementState.Running:
                        mCurrentMoveAccel = mCrouchRunAcceleration;
                        mCurrentMoveSpeed = mCrouchRunSpeed;
                        break;

                    default:
                        mCurrentMoveAccel = 0;
                        mCurrentMoveSpeed = 0;
                        break;
                }

                break;
        }

        mCurrentMaxSlopeUpAngle = Math.Clamp(mCurrentMaxSlopeUpAngle,
            MIN_SLOPE_ANGLE, MAX_SLOPE_ANGLE);
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        UpdateCapsuleSize();

        Vector3 moveH = Vector3.ProjectOnPlane(originalMove, mCharUp);
        Vector3 moveV = originalMove - moveH;
        Vector3 remainingMove = moveH;
        float moveVMag = moveV.magnitude;

        // perform the vertical move (usually jump)
        if (moveVMag > 0f)
        {
            CapsuleMove(moveV);
        }

        if (remainingMove.magnitude > mMinMoveDistance)
        {
            var stepUpHeight = 0f;
            var canStepUp = moveVMag == 0f;
            var didStepUp = false;
            var didStepUpRecover = false;
            var positionBeforeStepUp = Vector3.zero;
            var moveBeforeStepUp = Vector3.zero;

            CapsuleResolvePenetration();

            for (uint it = 0; it < MAX_MOVE_ITERATIONS; it++)
            {
                remainingMove -= CapsuleMove(remainingMove, out RaycastHit moveHit, out Vector3 moveHitNormal);

                // perform step up recover
                if (didStepUp && !didStepUpRecover)
                {
                    didStepUp = false;
                    didStepUpRecover = true;

                    CapsuleMove(mCharacter.down * stepUpHeight, out RaycastHit stepUpRecoverHit, out Vector3 stepUpRecoverHitNormal);

                    if (stepUpRecoverHit.collider)
                    {
                        // if we cannot step on this mGround, revert the step up
                        // and continue the loop without stepping up this time
                        if (CanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal, out float baseAngle) == false)
                        {
                            if (baseAngle < 90f)
                            {
                                mCapsule.position = positionBeforeStepUp;
                                remainingMove = moveBeforeStepUp;
                                canStepUp = false;

                                continue;
                            }
                        }
                    }
                }

                // if there is no collision (no obstacle or remainingMove == 0)
                // break the loop
                if (moveHit.collider is null)
                {
                    break;
                }

                // try sliding on the obstacle
                if (SlideOnSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
                {
                    continue;
                }

                // step up the first time, we hit an obstacle
                if (canStepUp && didStepUp == false)
                {
                    canStepUp = false;
                    didStepUp = true;
                    didStepUpRecover = false;
                    positionBeforeStepUp = mCapsule.position;
                    moveBeforeStepUp = remainingMove;

                    stepUpHeight = CapsuleMove(mCharUp * mCurrentStepUpHeight).magnitude;

                    continue;
                }

                // try sliding along the obstacle
                if (SlideAlongSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
                {
                    continue;
                }

                // there's nothing we can do now, so stop the move
                remainingMove = Vector3.zero;
            }
        }

        StepDown(originalMove);
        CapsuleResolvePenetration();
    }

    protected virtual void RecoverFromBaseMove()
    {
        if (mBaseDeltaPosition != Vector3.zero)
        {
            mCapsule.position += mBaseDeltaPosition;
            mBaseDeltaPosition = Vector3.zero;
        }

        // TODO: update position for base rotation also
    }

    protected virtual void UpdateCapsuleSize()
    {
        float weight = 0;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (mLocomotionState == LocomotionState.Crouching)
        {
            targetCenter = mCrouchCapsuleCenter;
            targetHeight = mCrouchCapsuleHeight;
            targetRadius = mCrouchCapsuleRadius;
            speed = mStandToCrouchTransitionSpeed * mDeltaTime;
        }
        else
        {
            targetCenter = mStandCapsuleCenter;
            targetHeight = mStandCapsuleHeight;
            targetRadius = mStandCapsuleRadius;
            speed = mCrouchToStandTransitionSpeed * mDeltaTime;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        // mCapsule.center = Vector3.Lerp(mCapsule.center, targetCenter, speed);
        mCapsule.height = Mathf.Lerp(mCapsule.height, targetHeight, speed);
        mCapsule.radius = Mathf.Lerp(mCapsule.radius, targetRadius, speed);

        weight = Mathf.Lerp(weight, 1f, speed);
        // mMovementStateWeight = weight;
    }

    protected virtual bool SlideOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (remainingMove == Vector3.zero)
            return false;

        if (CanStandOn(hit, hitNormal, out float slopeAngle))
        {
            if (slopeAngle == 0f)
            {
                return false;
            }

            Plane plane = new Plane(hitNormal, hit.point);
            Ray ray = new Ray(hit.point + remainingMove, mCharUp);
            plane.Raycast(ray, out float enter);

            Vector3 slopeMove = remainingMove + (mCharUp * enter);

            if (mCurrentMaintainVelocityOnSurface == false)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            return true;
        }

        return false;
    }

    protected virtual bool SlideAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        float remainingMoveSize = remainingMove.magnitude;

        if (hit.collider is null || remainingMoveSize == 0f)
            return false;

        RecalculateNormalIfZero(hit, ref hitNormal);

        Vector3 hitProject = Vector3.ProjectOnPlane(hitNormal, mCharUp);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMoveSize, hitProject);
        if (mCurrentMaintainVelocityAlongSurface)
        {
            // to avoid sliding along perpendicular surface for very small values,
            // may be a result of small miscalculations
            if (slideMove.magnitude > MIN_MOVE_ALONG_SURFACE_TO_MAINTAIN_VELOCITY)
            {
                slideMove = slideMove.normalized * remainingMoveSize;
            }
        }

        remainingMove = slideMove;
        return true;
    }

    protected virtual bool StepDown(Vector3 originalMove)
    {
        var verticalMove = Vector3.Project(originalMove, mCharUp).magnitude;
        if (verticalMove != 0f || mCurrentStepDownDepth <= 0)
            return false;

        CapsuleResolvePenetration();

        var moved = CapsuleMove(mCharacter.down * mCurrentStepDownDepth, out RaycastHit hit, out Vector3 hitNormal);

        if (CanStandOn(hit, hitNormal) == false)
        {
            mCapsule.position -= moved;
            return false;
        }

        return true;
    }

    protected virtual bool CheckIsGround(Collider collider)
    {
        if (collider is null)
        {
            return false;
        }

        return mGroundLayer.Contains(collider.gameObject.layer);
    }

    protected virtual bool CanStandOn(RaycastHit hit)
    {
        return CanStandOn(hit, Vector3.zero);
    }

    protected virtual bool CanStandOn(RaycastHit hit, Vector3 slopeNormal)
    {
        return CanStandOn(hit, slopeNormal, out float slopeAngle);
    }

    protected virtual bool CanStandOn(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        slopeAngle = 0f;

        if (hit.collider is not null)
        {
            if (CheckIsGround(hit.collider) is false)
            {
                return false;
            }

            RecalculateNormalIfZero(hit, ref slopeNormal);

            slopeAngle = Vector3.Angle(mCharUp, slopeNormal);
            if (slopeAngle >= MIN_SLOPE_ANGLE && slopeAngle <= mCurrentMaxSlopeUpAngle)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool CastForGround(float depth, out GroundResult result)
    {
        BaseSphereCast(mCharacter.down * depth, out RaycastHit hit, out Vector3 hitNormal);
        if (hitNormal == Vector3.zero)
        {
            hitNormal = hit.normal;
        }

        result = new GroundResult();

        if (CanStandOn(hit, hitNormal, out float slopeAngle) == false)
        {
            if (slopeAngle < 90f)
            {
                return false;
            }
        }

        result.collider = hit.collider;
        result.direction = mCharacter.down;
        result.distance = hit.distance;

        result.angle = slopeAngle;

        result.basePosition = result.collider.transform.position;
        result.baseRotation = result.collider.transform.rotation;

        result.edgeDistance = default;

        return true;
    }

    protected virtual void UpdateGroundResult()
    {
        mPrevGroundResult = mGroundResult;
        CastForGround(mGroundCheckDepth, out mGroundResult);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void CreateAnimationGraph()
    {
        if (mGraph.IsValid())
        {
            throw new NullReferenceException($"PlayableGraph for character {mCharacter.name} is invalid");
        }

        if (mSource is null)
        {
            throw new NullReferenceException(@$"{nameof(CharacterMovementGroundModuleSource)} 
                for {nameof(CharacterMovementGroundModule)} of character {mCharacter.name} is null");
        }

        mAnimBaseTree = new AnimationBlendTree1D(mGraph);
        mAnimStandTree = new AnimationBlendTree1D(mGraph);
        mAnimStandWalkTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mAnimStandRunTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mAnimCrouchTree = new AnimationBlendTree1D(mGraph);
        mAnimCrouchWalkTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mAnimCrouchRunTree = new AnimationBlendTree2DSimpleDirectional(mGraph);

        Vector2 center = new Vector2(0.00f, 0.00f);
        Vector2 front = new Vector2(0.00f, 1.00f);
        Vector2 frontLeft = new Vector2(-0.70f, 0.70f);
        Vector2 frontRight = new Vector2(-0.70f, 0.70f);
        Vector2 left = new Vector2(-0.10f, 0.00f);
        Vector2 right = new Vector2(0.10f, 0.00f);
        Vector2 back = new Vector2(0.00f, -1.00f);
        Vector2 backLeft = new Vector2(-0.70f, -0.70f);
        Vector2 backRight = new Vector2(0.70f, -0.70f);

        //////////////////////////////////////////////////////////////////

        mAnimStandWalkTree.Reserve(9);
        mAnimStandWalkTree.AddElement(mSource.animStandIdle, center);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkForward, front);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkForwardLeft, frontLeft);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkForwardRight, frontRight);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkLeft, left);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkRight, right);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkBackward, back);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkBackwardLeft, backLeft);
        mAnimStandWalkTree.AddElement(mSource.animStandWalkBackwardRight, backRight);
        mAnimStandWalkTree.FootIk = true;
        mAnimStandWalkTree.BuildGraph(true);
        mAnimStandWalkTree.UpdateGraph(true);

        mAnimStandRunTree.Reserve(9);
        mAnimStandRunTree.AddElement(mSource.animStandIdle, center * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunForward, front * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunForwardLeft, frontLeft * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunForwardRight, frontRight * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunLeft, left * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunRight, right * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunBackward, back * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunBackwardLeft, backLeft * 2f);
        mAnimStandRunTree.AddElement(mSource.animStandRunBackwardRight, backRight * 2f);
        mAnimStandRunTree.FootIk = true;
        mAnimStandRunTree.BuildGraph(true);
        mAnimStandRunTree.UpdateGraph(true);

        mAnimStandTree.Reserve(4);
        mAnimStandTree.AddElement(mSource.animStandIdle, 0f);
        mAnimStandTree.AddElement(mAnimStandWalkTree, 1f);
        mAnimStandTree.AddElement(mAnimStandRunTree, 2f);
        mAnimStandTree.AddElement(mSource.animStandSprintForward, 3f);
        mAnimStandTree.FootIk = true;
        mAnimStandTree.BuildGraph(true);
        mAnimStandTree.UpdateGraph(true);

        //////////////////////////////////////////////////////////////////

        mAnimCrouchWalkTree.Reserve(9);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchIdle, center);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkForward, front);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkForwardLeft, frontLeft);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkForwardRight, frontRight);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkLeft, left);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkRight, right);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkBackward, back);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkBackwardLeft, backLeft);
        mAnimCrouchWalkTree.AddElement(mSource.animCrouchWalkBackwardRight, backRight);
        mAnimCrouchWalkTree.FootIk = true;
        mAnimCrouchWalkTree.BuildGraph(true);
        mAnimCrouchWalkTree.UpdateGraph(true);

        mAnimCrouchRunTree.Reserve(9);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchIdle, center * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunForward, front * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunForwardLeft, frontLeft * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunForwardRight, frontRight * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunLeft, left * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunRight, right * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunBackward, back * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunBackwardLeft, backLeft * 2f);
        mAnimCrouchRunTree.AddElement(mSource.animCrouchRunBackwardRight, backRight * 2f);
        mAnimCrouchRunTree.FootIk = true;
        mAnimCrouchRunTree.BuildGraph(true);
        mAnimCrouchRunTree.UpdateGraph(true);

        mAnimCrouchTree.Reserve(3);
        mAnimCrouchTree.AddElement(mSource.animCrouchIdle, 0f);
        mAnimCrouchTree.AddElement(mAnimCrouchWalkTree, 1f);
        mAnimCrouchTree.AddElement(mAnimCrouchRunTree, 2f);
        mAnimCrouchTree.FootIk = true;
        mAnimCrouchTree.BuildGraph(true);
        mAnimCrouchTree.UpdateGraph(true);

        //////////////////////////////////////////////////////////////////

        mAnimBaseTree.Reserve(2);
        mAnimBaseTree.AddElement(mAnimStandTree, 0f);
        mAnimBaseTree.AddElement(mAnimCrouchTree, 1f);
        mAnimBaseTree.FootIk = true;
        mAnimBaseTree.BuildGraph(true);
        mAnimBaseTree.UpdateGraph(true);
    }

    protected virtual void UpdateAnimationGraph()
    {
        float walkSpeed = mStandWalkSpeed;
        float runSpeed = mStandRunSpeed;
        float sprintSpeed = mStandSprintSpeed;

        Vector2 velocity = new Vector2(mVelocity.x, mVelocity.z);
        float speed = velocity.magnitude;
        speed = MathF.Round(speed, 2);

        if (speed <= walkSpeed)
        {
            speed = speed / walkSpeed;
        }
        else if (speed <= runSpeed)
        {
            speed = 1f + (speed - walkSpeed) / (runSpeed - walkSpeed);
        }
        else
        {
            speed = 2f + (speed - runSpeed) / (sprintSpeed - runSpeed);
        }

        velocity = velocity.normalized * Mathf.Clamp(speed, 0f, 2f);

        mAnimStandWalkTree.SetBlendPosition(velocity);
        mAnimStandRunTree.SetBlendPosition(velocity);
        mAnimCrouchWalkTree.SetBlendPosition(velocity);
        mAnimCrouchRunTree.SetBlendPosition(velocity);

        mAnimStandTree.SetBlendPosition(speed);
        mAnimCrouchTree.SetBlendPosition(speed);
        mAnimBaseTree.SetBlendPosition(0f);
    }

    protected CharacterMovementGroundModuleSource mSource;
    protected CharacterView mCharView;
    protected GroundResult mGroundResult;
    protected GroundResult mPrevGroundResult;

    protected Vector3 mBaseDeltaPosition;
    protected Vector3 mBaseDeltaRotation;

    protected LocomotionState mLocomotionState;         // current state to process
    protected MovementState mMovementState;             // current state to process
    protected LocomotionState mLastLocomotionState;     // last processed state, could be same as current state
    protected MovementState mLastMovementState;         // last processed state, could be same as current state
    protected LocomotionState mPrevLocomotionState;     // previous state, different from current state
    protected MovementState mPrevMovementState;         // previous state, different from current state

    protected PlayableGraph mGraph;
    protected AnimationBlendTree1D mAnimBaseTree;
    protected AnimationBlendTree1D mAnimStandTree;
    protected AnimationBlendTree2D mAnimStandWalkTree;
    protected AnimationBlendTree2D mAnimStandRunTree;
    protected AnimationBlendTree1D mAnimCrouchTree;
    protected AnimationBlendTree2D mAnimCrouchWalkTree;
    protected AnimationBlendTree2D mAnimCrouchRunTree;

    protected float mCurrentMoveSpeed = 0;
    protected float mCurrentMoveAccel = 0;
    protected float mCurrentJumpPower = 0;
    protected float mCurrentStepUpHeight = 0;
    protected float mCurrentStepDownDepth = 0;
    protected float mCurrentMaxSlopeUpAngle = 0;
    protected float mCurrentSlopeDownAngle = 0;
    protected bool mCurrentMaintainVelocityOnSurface = true;
    protected bool mCurrentMaintainVelocityAlongSurface = true;

    // cached values from source asset
    protected readonly LayerMask mGroundLayer;
    protected readonly float mMinMoveDistance;
    protected readonly float mGroundCheckDepth;

    protected readonly float mStandDeacceleration;
    protected readonly float mStandWalkSpeed;
    protected readonly float mStandWalkAcceleration;
    protected readonly float mStandRunSpeed;
    protected readonly float mStandRunAcceleration;
    protected readonly float mStandSprintSpeed;
    protected readonly float mStandSprintAcceleration;
    protected readonly float mStandSprintLeftAngleMax;
    protected readonly float mStandSprintRightAngleMax;
    protected readonly float mStandJumpForce;
    protected readonly float mStandStepUpPercent;
    protected readonly float mStandStepDownPercent;
    protected float mStandStepUpHeight => mCapsule.height * mStandStepUpPercent / 100f;
    protected float mStandStepDownDepth => mCapsule.height * mStandStepDownPercent / 100f;
    protected readonly float mStandSlopeUpAngle;
    protected readonly float mStandSlopeDownAngle;
    protected readonly float mStandCapsuleHeight;
    protected readonly float mStandCapsuleRadius;
    protected readonly float mStandToCrouchTransitionSpeed;
    protected readonly bool mStandMaintainVelocityOnSurface;
    protected readonly bool mStandMaintainVelocityAlongSurface;
    protected readonly Vector3 mStandCapsuleCenter;

    protected readonly float mCrouchDeacceleration;
    protected readonly float mCrouchWalkSpeed;
    protected readonly float mCrouchWalkAcceleration;
    protected readonly float mCrouchRunSpeed;
    protected readonly float mCrouchRunAcceleration;
    protected readonly float mCrouchJumpForce;
    protected readonly float mCrouchStepUpPercent;
    protected readonly float mCrouchStepDownPercent;
    protected float mCrouchStepUpHeight => mCapsule.height * mCrouchStepUpPercent / 100f;
    protected float mCrouchStepDownDepth => mCapsule.height * mCrouchStepUpPercent / 100f;
    protected readonly float mCrouchSlopeUpAngle;
    protected readonly float mCrouchSlopeDownAngle;
    protected readonly float mCrouchCapsuleHeight;
    protected readonly float mCrouchCapsuleRadius;
    protected readonly float mCrouchToStandTransitionSpeed;
    protected readonly bool mCrouchMaintainVelocityOnSurface;
    protected readonly bool mCrouchMaintainVelocityAlongSurface;
    protected readonly Vector3 mCrouchCapsuleCenter;
}