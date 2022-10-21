using System;
using UnityEngine;
using UnityEngine.Playables;
using GameFramework.Extensions;

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

    protected const uint MAX_MOVE_ITERATIONS = 10;
    protected const float MIN_MOVE_ALONG_SURFACE_TO_MAINTAIN_VELOCITY = .0001f;
    protected const float MIN_SLOPE_ANGLE = 0f;
    protected const float MAX_SLOPE_ANGLE = 89.9f;

    public CharacterMovementGroundModule(CharacterMovementGroundModuleSource source)
    {
        mGroundPreviousResult = GroundResult.invalid;
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
    }

    //////////////////////////////////////////////////////////////////

    public virtual void Walk()
    {
        mInputWalk = true;
    }

    public virtual void Run()
    {
    }

    public virtual void Sprint()
    {
        mInputSprint = true;
    }

    public virtual void Stand()
    {
    }

    public virtual void Crouch()
    {
        mInputCrouch = true;
    }

    public virtual void Prone()
    {
        mInputProne = true;
    }

    public virtual void Jump()
    {
        mInputJump = true;
    }

    protected virtual void ConsumeInputs()
    {
        mInputMove = Vector2.zero;
        mInputWalk = false;
        mInputSprint = false;
        mInputCrouch = false;
        mInputProne = false;
        mInputJump = false;
    }

    public virtual bool CanStandOnGround(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        return CanStandOn(hit, slopeNormal, out slopeAngle);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void UpdateValues()
    {
        if (mMoveState.isGroundStanding)
        {
            mCurrentMaintainVelocityOnSurface = mStandMaintainVelocityOnSurface;
            mCurrentMaintainVelocityAlongSurface = mStandMaintainVelocityAlongSurface;
        }
        else if (mMoveState.isGroundCrouching)
        {
            mCurrentMaintainVelocityOnSurface = mCrouchMaintainVelocityOnSurface;
            mCurrentMaintainVelocityAlongSurface = mCrouchMaintainVelocityAlongSurface;
        }

        switch (mMoveState.current)
        {
            case CharacterMovementState.GROUND_STAND_IDLE:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandDeacceleration;
                mCurrentMoveSpeed = 0;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE_JUMP:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandDeacceleration;
                mCurrentMoveSpeed = 0;
                mCurrentJumpPower = mStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandWalkAcceleration;
                mCurrentMoveSpeed = mStandWalkSpeed;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_WALK_JUMP:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandWalkAcceleration;
                mCurrentMoveSpeed = mStandWalkSpeed;
                mCurrentJumpPower = mStandJumpForce;
                break;


            case CharacterMovementState.GROUND_STAND_RUN:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandRunAcceleration;
                mCurrentMoveSpeed = mStandRunSpeed;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_RUN_JUMP:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandRunAcceleration;
                mCurrentMoveSpeed = mStandRunSpeed;
                mCurrentJumpPower = mStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandSprintAcceleration;
                mCurrentMoveSpeed = mStandSprintSpeed;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT_JUMP:
                mCurrentStepDownDepth = mStandStepDownDepth;
                mCurrentStepUpHeight = mStandStepUpHeight;
                mCurrentMaxSlopeUpAngle = mStandSlopeUpAngle;
                mCurrentSlopeDownAngle = mStandSlopeDownAngle;
                mCurrentMoveAccel = mStandSprintAcceleration;
                mCurrentMoveSpeed = mStandSprintSpeed;
                mCurrentJumpPower = mStandJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                mCurrentStepDownDepth = mCrouchStepDownDepth;
                mCurrentStepUpHeight = mCrouchStepUpHeight;
                mCurrentMaxSlopeUpAngle = mCrouchSlopeUpAngle;
                mCurrentSlopeDownAngle = mCrouchSlopeDownAngle;
                mCurrentMoveAccel = mCrouchDeacceleration;
                mCurrentMoveSpeed = 0;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                mCurrentStepDownDepth = mCrouchStepDownDepth;
                mCurrentStepUpHeight = mCrouchStepUpHeight;
                mCurrentMaxSlopeUpAngle = mCrouchSlopeUpAngle;
                mCurrentSlopeDownAngle = mCrouchSlopeDownAngle;
                mCurrentMoveAccel = mCrouchWalkAcceleration;
                mCurrentMoveSpeed = mCrouchWalkSpeed;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                mCurrentStepDownDepth = mCrouchStepDownDepth;
                mCurrentStepUpHeight = mCrouchStepUpHeight;
                mCurrentMaxSlopeUpAngle = mCrouchSlopeUpAngle;
                mCurrentSlopeDownAngle = mCrouchSlopeDownAngle;
                mCurrentMoveAccel = mCrouchRunAcceleration;
                mCurrentMoveSpeed = mCrouchRunSpeed;
                mCurrentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_SLIDE:
            case CharacterMovementState.GROUND_ROLL:
            default:
                mCurrentStepDownDepth = 0f;
                mCurrentStepUpHeight = 0f;
                mCurrentMaxSlopeUpAngle = 0f;
                mCurrentSlopeDownAngle = 0f;
                mCurrentMoveAccel = 0f;
                mCurrentMoveSpeed = 0f;
                mCurrentJumpPower = 0f;
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
        float moveVMag = moveV.magnitude;

        Vector3 remainingMove = moveH;

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
        float weight = mInputMovementStateWeight;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (mMoveState.isGroundCrouching)
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
        mMovementStateWeight = weight;
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
        mGroundPreviousResult = mGroundResult;
        CastForGround(mGroundCheckDepth, out mGroundResult);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void CreateLocomotionGraph()
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

        mBaseTree = new AnimationBlendTree1D(mGraph);
        mStandTree = new AnimationBlendTree1D(mGraph);
        mStandWalkTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mStandRunTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mCrouchTree = new AnimationBlendTree1D(mGraph);
        mCrouchWalkTree = new AnimationBlendTree2DSimpleDirectional(mGraph);
        mCrouchRunTree = new AnimationBlendTree2DSimpleDirectional(mGraph);

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

        mStandWalkTree.Reserve(9);
        mStandWalkTree.AddElement(mSource.animStandIdle, center);
        mStandWalkTree.AddElement(mSource.animStandWalkForward, front);
        mStandWalkTree.AddElement(mSource.animStandWalkForwardLeft, frontLeft);
        mStandWalkTree.AddElement(mSource.animStandWalkForwardRight, frontRight);
        mStandWalkTree.AddElement(mSource.animStandWalkLeft, left);
        mStandWalkTree.AddElement(mSource.animStandWalkRight, right);
        mStandWalkTree.AddElement(mSource.animStandWalkBackward, back);
        mStandWalkTree.AddElement(mSource.animStandWalkBackwardLeft, backLeft);
        mStandWalkTree.AddElement(mSource.animStandWalkBackwardRight, backRight);
        mStandWalkTree.FootIk = true;
        mStandWalkTree.BuildGraph(true);
        mStandWalkTree.UpdateGraph(true);

        mStandRunTree.Reserve(9);
        mStandRunTree.AddElement(mSource.animStandIdle, center * 2f);
        mStandRunTree.AddElement(mSource.animStandRunForward, front * 2f);
        mStandRunTree.AddElement(mSource.animStandRunForwardLeft, frontLeft * 2f);
        mStandRunTree.AddElement(mSource.animStandRunForwardRight, frontRight * 2f);
        mStandRunTree.AddElement(mSource.animStandRunLeft, left * 2f);
        mStandRunTree.AddElement(mSource.animStandRunRight, right * 2f);
        mStandRunTree.AddElement(mSource.animStandRunBackward, back * 2f);
        mStandRunTree.AddElement(mSource.animStandRunBackwardLeft, backLeft * 2f);
        mStandRunTree.AddElement(mSource.animStandRunBackwardRight, backRight * 2f);
        mStandRunTree.FootIk = true;
        mStandRunTree.BuildGraph(true);
        mStandRunTree.UpdateGraph(true);

        mStandTree.Reserve(4);
        mStandTree.AddElement(mSource.animStandIdle, 0f);
        mStandTree.AddElement(mStandWalkTree, 1f);
        mStandTree.AddElement(mStandRunTree, 2f);
        mStandTree.AddElement(mSource.animStandSprintForward, 3f);
        mStandTree.FootIk = true;
        mStandTree.BuildGraph(true);
        mStandTree.UpdateGraph(true);

        //////////////////////////////////////////////////////////////////

        mCrouchWalkTree.Reserve(9);
        mCrouchWalkTree.AddElement(mSource.animCrouchIdle, center);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkForward, front);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkForwardLeft, frontLeft);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkForwardRight, frontRight);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkLeft, left);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkRight, right);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkBackward, back);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkBackwardLeft, backLeft);
        mCrouchWalkTree.AddElement(mSource.animCrouchWalkBackwardRight, backRight);
        mCrouchWalkTree.FootIk = true;
        mCrouchWalkTree.BuildGraph(true);
        mCrouchWalkTree.UpdateGraph(true);

        mCrouchRunTree.Reserve(9);
        mCrouchRunTree.AddElement(mSource.animCrouchIdle, center * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunForward, front * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunForwardLeft, frontLeft * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunForwardRight, frontRight * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunLeft, left * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunRight, right * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunBackward, back * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunBackwardLeft, backLeft * 2f);
        mCrouchRunTree.AddElement(mSource.animCrouchRunBackwardRight, backRight * 2f);
        mCrouchRunTree.FootIk = true;
        mCrouchRunTree.BuildGraph(true);
        mCrouchRunTree.UpdateGraph(true);

        mCrouchTree.Reserve(3);
        mCrouchTree.AddElement(mSource.animCrouchIdle, 0f);
        mCrouchTree.AddElement(mCrouchWalkTree, 1f);
        mCrouchTree.AddElement(mCrouchRunTree, 2f);
        mCrouchTree.FootIk = true;
        mCrouchTree.BuildGraph(true);
        mCrouchTree.UpdateGraph(true);

        //////////////////////////////////////////////////////////////////

        mBaseTree.Reserve(2);
        mBaseTree.AddElement(mStandTree, 0f);
        mBaseTree.AddElement(mCrouchTree, 1f);
        mBaseTree.FootIk = true;
        mBaseTree.BuildGraph(true);
        mBaseTree.UpdateGraph(true);
    }

    protected virtual void UpdateLocomotionGraph()
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

        mStandWalkTree.SetBlendPosition(velocity);
        mStandRunTree.SetBlendPosition(velocity);
        mCrouchWalkTree.SetBlendPosition(velocity);
        mCrouchRunTree.SetBlendPosition(velocity);

        mStandTree.SetBlendPosition(speed, true);
        mCrouchTree.SetBlendPosition(speed, true);
        mBaseTree.SetBlendPosition(0f, true);
    }

    protected CharacterMovementGroundModuleSource mSource;
    protected CharacterView mCharView;
    protected GroundResult mGroundPreviousResult;
    protected GroundResult mGroundResult;
    protected LayerMask mGroundLayer;

    protected Vector3 mBaseDeltaPosition;
    protected Vector3 mBaseDeltaRotation;

    protected float mMinMoveDistance;
    protected float mGroundCheckDepth;

    protected float mStandDeacceleration;
    protected float mStandWalkSpeed;
    protected float mStandWalkAcceleration;
    protected float mStandRunSpeed;
    protected float mStandRunAcceleration;
    protected float mStandSprintSpeed;
    protected float mStandSprintAcceleration;
    protected float mStandSprintLeftAngleMax;
    protected float mStandSprintRightAngleMax;
    protected float mStandJumpForce;
    protected float mStandStepUpPercent;
    protected float mStandStepDownPercent;
    protected float mStandSlopeUpAngle;
    protected float mStandSlopeDownAngle;
    protected float mStandCapsuleHeight;
    protected float mStandCapsuleRadius;
    protected float mStandToCrouchTransitionSpeed;
    protected float mStandStepUpHeight => mCapsule.height * mStandStepUpPercent / 100;
    protected float mStandStepDownDepth => mCapsule.height * mStandStepDownPercent / 100;
    protected bool mStandMaintainVelocityOnSurface;
    protected bool mStandMaintainVelocityAlongSurface;
    protected Vector3 mStandCapsuleCenter;

    protected float mCrouchDeacceleration;
    protected float mCrouchWalkSpeed;
    protected float mCrouchWalkAcceleration;
    protected float mCrouchRunSpeed;
    protected float mCrouchRunAcceleration;
    protected float mCrouchJumpForce;
    protected float mCrouchStepUpPercent;
    protected float mCrouchStepDownPercent;
    protected float mCrouchSlopeUpAngle;
    protected float mCrouchSlopeDownAngle;
    protected float mCrouchCapsuleHeight;
    protected float mCrouchCapsuleRadius;
    protected float mCrouchToStandTransitionSpeed;
    protected float mCrouchStepUpHeight => mCapsule.height * mCrouchStepUpPercent / 100;
    protected float mCrouchStepDownDepth => mCapsule.height * mCrouchStepUpPercent / 100;
    protected bool mCrouchMaintainVelocityOnSurface;
    protected bool mCrouchMaintainVelocityAlongSurface;
    protected Vector3 mCrouchCapsuleCenter;

    protected float mCurrentMoveSpeed = 0;
    protected float mCurrentMoveAccel = 0;
    protected float mCurrentJumpPower = 0;
    protected float mCurrentStepUpHeight = 0;
    protected float mCurrentStepDownDepth = 0;
    protected float mCurrentMaxSlopeUpAngle = 0;
    protected float mCurrentSlopeDownAngle = 0;
    protected bool mCurrentMaintainVelocityOnSurface = true;
    protected bool mCurrentMaintainVelocityAlongSurface = true;

    protected bool mDidJumpedLastUpdate;

    protected CharacterMovementState mMoveState = null;
    protected CharacterMovementState newState = null;
    protected float mInputMovementStateWeight = 0f;
    protected float mMovementStateWeight = 0f;
    protected bool mInputCrouch = false;
    protected bool mInputProne = false;
    protected bool mInputSprint = false;
    protected bool mInputWalk = false;
    protected bool mInputJump = false;

    protected PlayableGraph mGraph;
    protected AnimationBlendTree1D mBaseTree;
    protected AnimationBlendTree1D mStandTree;
    protected AnimationBlendTree2D mStandWalkTree;
    protected AnimationBlendTree2D mStandRunTree;
    protected AnimationBlendTree1D mCrouchTree;
    protected AnimationBlendTree2D mCrouchWalkTree;
    protected AnimationBlendTree2D mCrouchRunTree;
}