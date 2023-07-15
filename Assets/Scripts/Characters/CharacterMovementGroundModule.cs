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
        _prevGroundResult = GroundResult.invalid;
        _groundResult = GroundResult.invalid;

        // cache Data from CharacterDataSource
        if (source is not null)
        {
            _groundCheckDepth = source.checkDepth;
            _groundLayer = source.groundLayer;
            _minMoveDistance = source.minMoveDistance;

            _standDeacceleration = source.standIdleAcceleration;
            _standWalkSpeed = source.standWalkSpeed;
            _standWalkAcceleration = source.standWalkAcceleration;
            _standRunSpeed = source.standRunSpeed;
            _standRunAcceleration = source.standRunAcceleration;
            _standSprintSpeed = source.standSprintSpeed;
            _standSprintAcceleration = source.standSprintAcceleration;
            _standSprintLeftAngleMax = source.standSprintLeftAngleMax;
            _standSprintRightAngleMax = source.standSprintRightAngleMax;
            _standJumpForce = source.standJumpForce;
            _standStepUpPercent = source.standStepUpPercent;
            _standStepUpHeight = _capsule.height * _standStepUpPercent / 100f;
            _standStepDownPercent = source.standStepDownPercent;
            _standStepDownHeight = _capsule.height * _standStepDownPercent / 100f;
            _standSlopeUpAngle = source.standSlopeUpAngle;
            _standSlopeDownAngle = source.standSlopeDownAngle;
            _standMaintainVelocityOnSurface = source.standMaintainVelocityOnSurface;
            _standMaintainVelocityAlongSurface = source.standMaintainVelocityAlongSurface;
            _standCapsuleCenter = source.standCapsuleCenter;
            _standCapsuleHeight = source.standCapsuleHeight;
            _standCapsuleRadius = source.standCapsuleRadius;
            _standToCrouchTransitionSpeed = source.standToCrouchTransitionSpeed;

            _crouchDeacceleration = source.crouchIdleAcceleration;
            _crouchWalkSpeed = source.crouchWalkSpeed;
            _crouchWalkAcceleration = source.crouchWalkAcceleration;
            _crouchRunSpeed = source.crouchRunSpeed;
            _crouchRunAcceleration = source.crouchRunAcceleration;
            _crouchJumpForce = source.crouchJumpForce;
            _crouchStepUpPercent = source.crouchStepUpPercent;
            _crouchStepUpHeight = _capsule.height * _crouchStepUpPercent / 100f;
            _crouchStepDownPercent = source.crouchStepDownPercent;
            _crouchStepUpHeight = _capsule.height * _crouchStepUpPercent / 100f;
            _crouchSlopeUpAngle = source.crouchSlopeUpAngle;
            _crouchSlopeDownAngle = source.crouchSlopeDownAngle;
            _crouchMaintainVelocityOnSurface = source.crouchMaintainVelocityOnSurface;
            _crouchMaintainVelocityAlongSurface = source.crouchMaintainVelocityAlongSurface;
            _crouchCapsuleCenter = source.crouchCapsuleCenter;
            _crouchCapsuleHeight = source.crouchCapsuleHeight;
            _crouchCapsuleRadius = source.crouchCapsuleRadius;
            _crouchToStandTransitionSpeed = source.crouchToStandTransitionSpeed;
        }
    }

    public override void OnLoaded(CharacterMovement charMovement)
    {
        base.OnLoaded(charMovement);

        if (_character is not null)
        {
            _charView = _character.charView;
        }
    }

    public override void OnUnloaded(CharacterMovement charMovement)
    {
        base.OnUnloaded(charMovement);

        _charView = null;
    }

    public override bool ShouldRun()
    {
        _baseDeltaPosition = Vector3.zero;
        _baseDeltaRotation = Vector3.zero;

        if (_groundResult.isValid)
        {
            _baseDeltaPosition = _groundResult.collider.transform.position - _groundResult.basePosition;
            _baseDeltaRotation = _groundResult.collider.transform.rotation.eulerAngles - _groundResult.baseRotation.eulerAngles;
        }

        if (_baseDeltaPosition != Vector3.zero || _baseDeltaRotation != Vector3.zero)
        {
            return true;
        }

        return _groundResult.isValid;
    }

    public override void RunPhysics()
    {
        base.RunPhysics();

        PullPhysicsData();
        RecoverFromBaseMove();

        UpdateValues();

        Vector3 moveInput = new Vector3(_inputMove.x, 0, _inputMove.y);
        moveInput = Quaternion.Euler(0, _charView.turnAngle, 0) * moveInput.normalized;
        moveInput = _character.rotation * moveInput;

        _velocity = Vector3.ProjectOnPlane(_velocity, _charUp);

        Vector3 move = moveInput * _curMoveSpeed * _deltaTime;
        move = Vector3.MoveTowards(_velocity * _deltaTime, move, _curMoveAccel * _deltaTime);

        move += _charUp * _curJumpPower * _deltaTime;

        GroundMove(move);

        _lastMovementState = _movementState;
        _lastLocomotionState = _locomotionState;

        PushPhysicsData();
    }

    //////////////////////////////////////////////////////////////////

    public virtual void Walk()
    {
        _movementState = MovementState.Walking;
    }

    public virtual void Run()
    {
        _movementState = MovementState.Running;
    }

    public virtual void Sprint()
    {
        _movementState = MovementState.Sprinting;
    }

    public virtual void Stand()
    {
        _locomotionState = LocomotionState.Standing;
    }

    public virtual void Crouch()
    {
        _locomotionState = LocomotionState.Crouching;
    }

    public virtual void Prone()
    {
        _locomotionState = LocomotionState.Proning;
    }

    public virtual void Jump()
    {
        _locomotionState = LocomotionState.Jumping;
    }

    public virtual bool CanStandOnGround(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        return CanStandOn(hit, slopeNormal, out slopeAngle);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void UpdateValues()
    {
        switch (_locomotionState)
        {
            case LocomotionState.Standing:
                _curStepDownDepth = _standStepDownHeight;
                _curStepUpHeight = _standStepUpHeight;
                _curMaxSlopeUpAngle = _standSlopeUpAngle;
                _curSlopeDownAngle = _standSlopeDownAngle;
                _curMaintainVelocityOnSurface = _standMaintainVelocityOnSurface;
                _curMaintainVelocityAlongSurface = _standMaintainVelocityAlongSurface;
                _curJumpPower = 0f;

                if (_locomotionState == LocomotionState.Jumping)
                    _curJumpPower = _standJumpForce;

                switch (_movementState)
                {
                    case MovementState.Idle:
                        _curMoveAccel = _standDeacceleration;
                        _curMoveSpeed = 0;
                        break;

                    case MovementState.Walking:
                        _curMoveAccel = _standWalkAcceleration;
                        _curMoveSpeed = _standWalkSpeed;
                        break;

                    case MovementState.Running:
                        _curMoveAccel = _standRunAcceleration;
                        _curMoveSpeed = _standRunSpeed;
                        break;

                    case MovementState.Sprinting:
                        _curMoveAccel = _standSprintAcceleration;
                        _curMoveSpeed = _standSprintSpeed;
                        break;

                    default:
                        _curMoveAccel = 0;
                        _curMoveSpeed = 0;
                        break;
                }

                break;

            case LocomotionState.Crouching:
                _curStepDownDepth = _crouchStepDownDepth;
                _curStepUpHeight = _crouchStepUpHeight;
                _curMaxSlopeUpAngle = _crouchSlopeUpAngle;
                _curSlopeDownAngle = _crouchSlopeDownAngle;
                _curMaintainVelocityOnSurface = _crouchMaintainVelocityOnSurface;
                _curMaintainVelocityAlongSurface = _crouchMaintainVelocityAlongSurface;
                _curJumpPower = 0f;

                if (_locomotionState == LocomotionState.Jumping)
                    _curJumpPower = _crouchJumpForce;

                switch (_movementState)
                {
                    case MovementState.Idle:
                        _curMoveAccel = _crouchDeacceleration;
                        _curMoveSpeed = 0;
                        break;

                    case MovementState.Walking:
                        _curMoveAccel = _crouchWalkAcceleration;
                        _curMoveSpeed = _crouchWalkSpeed;
                        break;

                    case MovementState.Running:
                        _curMoveAccel = _crouchRunAcceleration;
                        _curMoveSpeed = _crouchRunSpeed;
                        break;

                    default:
                        _curMoveAccel = 0;
                        _curMoveSpeed = 0;
                        break;
                }

                break;
        }

        _curMaxSlopeUpAngle = Math.Clamp(_curMaxSlopeUpAngle,
            MIN_SLOPE_ANGLE, MAX_SLOPE_ANGLE);
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        UpdateCapsuleSize();

        Vector3 moveH = Vector3.ProjectOnPlane(originalMove, _charUp);
        Vector3 moveV = originalMove - moveH;
        Vector3 remainingMove = moveH;
        float moveVMag = moveV.magnitude;

        // perform the vertical move (usually jump)
        if (moveVMag > 0f)
        {
            CapsuleMove(moveV);
        }

        if (remainingMove.magnitude > _minMoveDistance)
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

                    CapsuleMove(_character.down * stepUpHeight, out RaycastHit stepUpRecoverHit, out Vector3 stepUpRecoverHitNormal);

                    if (stepUpRecoverHit.collider)
                    {
                        // if we cannot step on this _ground, revert the step up
                        // and continue the loop without stepping up this time
                        if (CanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal, out float baseAngle) == false)
                        {
                            if (baseAngle < 90f)
                            {
                                _capsule.position = positionBeforeStepUp;
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
                    positionBeforeStepUp = _capsule.position;
                    moveBeforeStepUp = remainingMove;

                    stepUpHeight = CapsuleMove(_charUp * _curStepUpHeight).magnitude;

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
        if (_baseDeltaPosition != Vector3.zero)
        {
            _capsule.position += _baseDeltaPosition;
            _baseDeltaPosition = Vector3.zero;
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

        if (_locomotionState == LocomotionState.Crouching)
        {
            targetCenter = _crouchCapsuleCenter;
            targetHeight = _crouchCapsuleHeight;
            targetRadius = _crouchCapsuleRadius;
            speed = _standToCrouchTransitionSpeed * _deltaTime;
        }
        else
        {
            targetCenter = _standCapsuleCenter;
            targetHeight = _standCapsuleHeight;
            targetRadius = _standCapsuleRadius;
            speed = _crouchToStandTransitionSpeed * _deltaTime;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        // _capsule.center = Vector3.Lerp(mCapsule.center, targetCenter, speed);
        _capsule.height = Mathf.Lerp(_capsule.height, targetHeight, speed);
        _capsule.radius = Mathf.Lerp(_capsule.radius, targetRadius, speed);

        weight = Mathf.Lerp(weight, 1f, speed);
        // _movementStateWeight = weight;
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
            Ray ray = new Ray(hit.point + remainingMove, _charUp);
            plane.Raycast(ray, out float enter);

            Vector3 slopeMove = remainingMove + (_charUp * enter);

            if (_curMaintainVelocityOnSurface == false)
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

        Vector3 hitProject = Vector3.ProjectOnPlane(hitNormal, _charUp);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMoveSize, hitProject);
        if (_curMaintainVelocityAlongSurface)
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
        var verticalMove = Vector3.Project(originalMove, _charUp).magnitude;
        if (verticalMove != 0f || _curStepDownDepth <= 0)
            return false;

        CapsuleResolvePenetration();

        var moved = CapsuleMove(_character.down * _curStepDownDepth, out RaycastHit hit, out Vector3 hitNormal);

        if (CanStandOn(hit, hitNormal) == false)
        {
            _capsule.position -= moved;
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

        return _groundLayer.Contains(collider.gameObject.layer);
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

            slopeAngle = Vector3.Angle(_charUp, slopeNormal);
            if (slopeAngle >= MIN_SLOPE_ANGLE && slopeAngle <= _curMaxSlopeUpAngle)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool CastForGround(float depth, out GroundResult result)
    {
        BaseSphereCast(_character.down * depth, out RaycastHit hit, out Vector3 hitNormal);
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
        result.direction = _character.down;
        result.distance = hit.distance;

        result.angle = slopeAngle;

        result.basePosition = result.collider.transform.position;
        result.baseRotation = result.collider.transform.rotation;

        result.edgeDistance = default;

        return true;
    }

    protected virtual void UpdateGroundResult()
    {
        _prevGroundResult = _groundResult;
        CastForGround(_groundCheckDepth, out _groundResult);
    }

    //////////////////////////////////////////////////////////////////

    protected virtual void CreateAnimationGraph()
    {
        if (_animGraph.IsValid())
        {
            throw new NullReferenceException($"PlayableGraph for character {_character.name} is invalid");
        }

        if (_source is null)
        {
            throw new NullReferenceException(@$"{nameof(CharacterMovementGroundModuleSource)} 
                for {nameof(CharacterMovementGroundModule)} of character {_character.name} is null");
        }

        _animBaseTree = new AnimationBlendTree1D(_animGraph);
        _animStandTree = new AnimationBlendTree1D(_animGraph);
        _animStandWalkTree = new AnimationBlendTree2DSimpleDirectional(_animGraph);
        _animStandRunTree = new AnimationBlendTree2DSimpleDirectional(_animGraph);
        _animCrouchTree = new AnimationBlendTree1D(_animGraph);
        _animCrouchWalkTree = new AnimationBlendTree2DSimpleDirectional(_animGraph);
        _animCrouchRunTree = new AnimationBlendTree2DSimpleDirectional(_animGraph);

        Vector2 center = new Vector2(0.00f, 0.00f);
        Vector2 front = new Vector2(0.00f, 1.00f);
        Vector2 frontLeft = new Vector2(-0.70f, 0.70f);
        Vector2 frontRight = new Vector2(-0.70f, 0.70f);
        Vector2 left = new Vector2(-0.10f, 0.00f);
        Vector2 right = new Vector2(0.10f, 0.00f);
        Vector2 back = new Vector2(0.00f, -1.00f);
        Vector2 backLeft = new Vector2(-0.70f, -0.70f);
        Vector2 backRight = new Vector2(0.70f, -0.70f);

        // Stand Tree
        // -----------------------------------------------------------------------------------------

        _animStandWalkTree.Reserve(9);
        _animStandWalkTree.AddElement(_source.animStandIdle, center);
        _animStandWalkTree.AddElement(_source.animStandWalkForward, front);
        _animStandWalkTree.AddElement(_source.animStandWalkForwardLeft, frontLeft);
        _animStandWalkTree.AddElement(_source.animStandWalkForwardRight, frontRight);
        _animStandWalkTree.AddElement(_source.animStandWalkLeft, left);
        _animStandWalkTree.AddElement(_source.animStandWalkRight, right);
        _animStandWalkTree.AddElement(_source.animStandWalkBackward, back);
        _animStandWalkTree.AddElement(_source.animStandWalkBackwardLeft, backLeft);
        _animStandWalkTree.AddElement(_source.animStandWalkBackwardRight, backRight);
        _animStandWalkTree.EnableFootIk();
        _animStandWalkTree.BuildGraph(true);
        _animStandWalkTree.UpdateGraph(true);

        _animStandRunTree.Reserve(9);
        _animStandRunTree.AddElement(_source.animStandIdle, center * 2f);
        _animStandRunTree.AddElement(_source.animStandRunForward, front * 2f);
        _animStandRunTree.AddElement(_source.animStandRunForwardLeft, frontLeft * 2f);
        _animStandRunTree.AddElement(_source.animStandRunForwardRight, frontRight * 2f);
        _animStandRunTree.AddElement(_source.animStandRunLeft, left * 2f);
        _animStandRunTree.AddElement(_source.animStandRunRight, right * 2f);
        _animStandRunTree.AddElement(_source.animStandRunBackward, back * 2f);
        _animStandRunTree.AddElement(_source.animStandRunBackwardLeft, backLeft * 2f);
        _animStandRunTree.AddElement(_source.animStandRunBackwardRight, backRight * 2f);
        _animStandRunTree.EnableFootIk();
        _animStandRunTree.BuildGraph(true);
        _animStandRunTree.UpdateGraph(true);

        _animStandTree.Reserve(4);
        _animStandTree.AddElement(_source.animStandIdle, 0f);
        _animStandTree.AddElement(_animStandWalkTree, 1f);
        _animStandTree.AddElement(_animStandRunTree, 2f);
        _animStandTree.AddElement(_source.animStandSprintForward, 3f);
        _animStandTree.EnableFootIk();
        _animStandTree.BuildGraph(true);
        _animStandTree.UpdateGraph(true);

        // Crouch Tree
        // -----------------------------------------------------------------------------------------

        _animCrouchWalkTree.Reserve(9);
        _animCrouchWalkTree.AddElement(_source.animCrouchIdle, center);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkForward, front);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkForwardLeft, frontLeft);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkForwardRight, frontRight);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkLeft, left);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkRight, right);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkBackward, back);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkBackwardLeft, backLeft);
        _animCrouchWalkTree.AddElement(_source.animCrouchWalkBackwardRight, backRight);
        _animCrouchWalkTree.EnableFootIk();
        _animCrouchWalkTree.BuildGraph(true);
        _animCrouchWalkTree.UpdateGraph(true);

        _animCrouchRunTree.Reserve(9);
        _animCrouchRunTree.AddElement(_source.animCrouchIdle, center * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunForward, front * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunForwardLeft, frontLeft * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunForwardRight, frontRight * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunLeft, left * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunRight, right * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunBackward, back * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunBackwardLeft, backLeft * 2f);
        _animCrouchRunTree.AddElement(_source.animCrouchRunBackwardRight, backRight * 2f);
        _animCrouchRunTree.EnableFootIk();
        _animCrouchRunTree.BuildGraph(true);
        _animCrouchRunTree.UpdateGraph(true);

        _animCrouchTree.Reserve(3);
        _animCrouchTree.AddElement(_source.animCrouchIdle, 0f);
        _animCrouchTree.AddElement(_animCrouchWalkTree, 1f);
        _animCrouchTree.AddElement(_animCrouchRunTree, 2f);
        _animCrouchTree.EnableFootIk();
        _animCrouchTree.BuildGraph(true);
        _animCrouchTree.UpdateGraph(true);

        // Base Tree
        // -----------------------------------------------------------------------------------------

        _animBaseTree.Reserve(2);
        _animBaseTree.AddElement(_animStandTree, 0f);
        _animBaseTree.AddElement(_animCrouchTree, 1f);
        _animBaseTree.EnableFootIk();
        _animBaseTree.BuildGraph(true);
        _animBaseTree.UpdateGraph(true);
    }

    protected virtual void UpdateAnimationGraph()
    {
        float walkSpeed = _standWalkSpeed;
        float runSpeed = _standRunSpeed;
        float sprintSpeed = _standSprintSpeed;

        Vector2 velocity = new Vector2(_velocity.x, _velocity.z);
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

        _animStandWalkTree.SetBlendPosition(velocity);
        _animStandRunTree.SetBlendPosition(velocity);
        _animCrouchWalkTree.SetBlendPosition(velocity);
        _animCrouchRunTree.SetBlendPosition(velocity);

        _animStandTree.SetBlendPosition(speed);
        _animCrouchTree.SetBlendPosition(speed);
        _animBaseTree.SetBlendPosition(0f);
    }

    protected CharacterMovementGroundModuleSource _source;
    protected CharacterView _charView;
    protected GroundResult _groundResult;
    protected GroundResult _prevGroundResult;

    protected Vector3 _baseDeltaPosition;
    protected Vector3 _baseDeltaRotation;

    protected LocomotionState _locomotionState;         // current state to process
    protected MovementState _movementState;             // current state to process
    protected LocomotionState _lastLocomotionState;     // last processed state, could be same as current state
    protected MovementState _lastMovementState;         // last processed state, could be same as current state
    protected LocomotionState _prevLocomotionState;     // previous state, different from current state
    protected MovementState _prevMovementState;         // previous state, different from current state

    protected PlayableGraph _animGraph;
    protected AnimationBlendTree1D _animBaseTree;
    protected AnimationBlendTree1D _animStandTree;
    protected AnimationBlendTree2D _animStandWalkTree;
    protected AnimationBlendTree2D _animStandRunTree;
    protected AnimationBlendTree1D _animCrouchTree;
    protected AnimationBlendTree2D _animCrouchWalkTree;
    protected AnimationBlendTree2D _animCrouchRunTree;

    protected float _curMoveSpeed = 0;
    protected float _curMoveAccel = 0;
    protected float _curJumpPower = 0;
    protected float _curStepUpHeight = 0;
    protected float _curStepDownDepth = 0;
    protected float _curMaxSlopeUpAngle = 0;
    protected float _curSlopeDownAngle = 0;
    protected bool _curMaintainVelocityOnSurface = true;
    protected bool _curMaintainVelocityAlongSurface = true;

    // cached values from source asset
    protected readonly LayerMask _groundLayer;
    protected readonly float _minMoveDistance;
    protected readonly float _groundCheckDepth;

    protected readonly float _standDeacceleration;
    protected readonly float _standWalkSpeed;
    protected readonly float _standWalkAcceleration;
    protected readonly float _standRunSpeed;
    protected readonly float _standRunAcceleration;
    protected readonly float _standSprintSpeed;
    protected readonly float _standSprintAcceleration;
    protected readonly float _standSprintLeftAngleMax;
    protected readonly float _standSprintRightAngleMax;
    protected readonly float _standJumpForce;
    protected readonly float _standStepUpPercent;
    protected readonly float _standStepDownPercent;
    protected readonly float _standStepUpHeight;
    protected readonly float _standStepDownHeight;
    protected readonly float _standSlopeUpAngle;
    protected readonly float _standSlopeDownAngle;
    protected readonly float _standCapsuleHeight;
    protected readonly float _standCapsuleRadius;
    protected readonly float _standToCrouchTransitionSpeed;
    protected readonly bool _standMaintainVelocityOnSurface;
    protected readonly bool _standMaintainVelocityAlongSurface;
    protected readonly Vector3 _standCapsuleCenter;

    protected readonly float _crouchDeacceleration;
    protected readonly float _crouchWalkSpeed;
    protected readonly float _crouchWalkAcceleration;
    protected readonly float _crouchRunSpeed;
    protected readonly float _crouchRunAcceleration;
    protected readonly float _crouchJumpForce;
    protected readonly float _crouchStepUpPercent;
    protected readonly float _crouchStepDownPercent;
    protected readonly float _crouchStepUpHeight;
    protected readonly float _crouchStepDownDepth;
    protected readonly float _crouchSlopeUpAngle;
    protected readonly float _crouchSlopeDownAngle;
    protected readonly float _crouchCapsuleHeight;
    protected readonly float _crouchCapsuleRadius;
    protected readonly float _crouchToStandTransitionSpeed;
    protected readonly bool _crouchMaintainVelocityOnSurface;
    protected readonly bool _crouchMaintainVelocityAlongSurface;
    protected readonly Vector3 _crouchCapsuleCenter;
}