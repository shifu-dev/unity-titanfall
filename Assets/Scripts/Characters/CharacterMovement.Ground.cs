using System;
using UnityEngine;
using GameFramework.Extensions;

public partial class CharacterMovement : CharacterBehaviour
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

    protected const uint GROUND_MAX_MOVE_ITERATIONS = 10;
    protected const float MIN_MOVE_ALONG_SURFACE_FOR_MAINTAIN_VELOCITY = .0001f;
    protected const float MIN_GROUND_SLOPE_ANGLE = 0f;
    protected const float MAX_GROUND_SLOPE_ANGLE = 89.9f;

    protected virtual void PhysGround()
    {
        GroundCalculateValues();

        Vector3 moveInput = new Vector3(_move.x, 0, _move.y);
        moveInput = Quaternion.Euler(0, _charView.turnAngle, 0) * moveInput.normalized;
        moveInput = character.rotation * moveInput;

        _velocity = Vector3.ProjectOnPlane(_velocity, _charUp);

        Vector3 move = moveInput * _currentMoveSpeed * _deltaTime;
        move = Vector3.MoveTowards(_velocity * _deltaTime, move, _currentMoveAccel * _deltaTime);

        move += _charUp * _currentJumpPower * _deltaTime;

        GroundMove(move);
    }

    protected virtual void GroundCalculateValues()
    {
        _charUp = character.up;
        _charRight = character.right;
        _charForward = character.forward;
        _currentJumpCount = 0;
        _currentMinMoveDist = _groundMinMoveDistance;

        if (MovementState.isGroundStanding)
        {
            _currentMaintainVelocityOnSurface = _groundStandMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = _groundStandMaintainVelocityAlongSurface;
        }
        else if (MovementState.isGroundCrouching)
        {
            _currentMaintainVelocityOnSurface = _groundCrouchMaintainVelocityOnSurface;
            _currentMaintainVelocityAlongSurface = _groundCrouchMaintainVelocityAlongSurface;
        }

        switch (MovementState.current)
        {
            case CharacterMovementState.GROUND_STAND_IDLE:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandIdleAcceleration;
                _currentMoveSpeed = _groundStandIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_IDLE_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandIdleAcceleration;
                _currentMoveSpeed = _groundStandIdleSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_WALK:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandWalkAcceleration;
                _currentMoveSpeed = _groundStandWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_WALK_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandWalkAcceleration;
                _currentMoveSpeed = _groundStandWalkSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;


            case CharacterMovementState.GROUND_STAND_RUN:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandRunAcceleration;
                _currentMoveSpeed = _groundStandRunSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_RUN_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandRunAcceleration;
                _currentMoveSpeed = _groundStandRunSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandSprintAcceleration;
                _currentMoveSpeed = _groundStandSprintSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_STAND_SPRINT_JUMP:
                _currentStepDownDepth = _groundStandStepDownDepth;
                _currentStepUpHeight = _groundStandStepUpHeight;
                _currentSlopeUpAngle = _groundStandSlopeUpAngle;
                _currentSlopeDownAngle = _groundStandSlopeDownAngle;
                _currentMoveAccel = _groundStandSprintAcceleration;
                _currentMoveSpeed = _groundStandSprintSpeed;
                _currentJumpPower = _groundStandJumpForce;
                break;

            case CharacterMovementState.GROUND_CROUCH_IDLE:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchIdleAcceleration;
                _currentMoveSpeed = _groundCrouchIdleSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_WALK:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchWalkAcceleration;
                _currentMoveSpeed = _groundCrouchWalkSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_CROUCH_RUN:
                _currentStepDownDepth = _groundCrouchStepDownDepth;
                _currentStepUpHeight = _groundCrouchStepUpHeight;
                _currentSlopeUpAngle = _groundCrouchSlopeUpAngle;
                _currentSlopeDownAngle = _groundCrouchSlopeDownAngle;
                _currentMoveAccel = _groundCrouchRunAcceleration;
                _currentMoveSpeed = _groundCrouchRunSpeed;
                _currentJumpPower = 0f;
                break;

            case CharacterMovementState.GROUND_SLIDE:
            case CharacterMovementState.GROUND_ROLL:
            default:
                _currentStepDownDepth = 0f;
                _currentStepUpHeight = 0f;
                _currentSlopeUpAngle = 0f;
                _currentSlopeDownAngle = 0f;
                _currentMoveAccel = 0f;
                _currentMoveSpeed = 0f;
                _currentJumpPower = 0f;
                break;
        }
    }

    protected virtual void GroundMove(Vector3 originalMove)
    {
        Vector3 lastPosition = _capsule.position;

        GroundResizeCapsule();

        Vector3 moveH = Vector3.ProjectOnPlane(originalMove, _charUp);
        Vector3 moveV = originalMove - moveH;
        float moveVMag = moveV.magnitude;

        Vector3 remainingMove = moveH;

        // perform the vertical move (usually jump)
        if (moveVMag > 0f)
        {
            CapsuleMove(moveV);
            _canGround = false;
        }

        if (remainingMove.magnitude > _currentMinMoveDist)
        {
            var stepUpHeight = 0f;
            var canStepUp = moveVMag == 0f;
            var didStepUp = false;
            var didStepUpRecover = false;
            var positionBeforeStepUp = Vector3.zero;
            var moveBeforeStepUp = Vector3.zero;

            CapsuleResolvePenetration();

            for (uint it = 0; it < GROUND_MAX_MOVE_ITERATIONS; it++)
            {
                remainingMove -= CapsuleMove(remainingMove, out RaycastHit moveHit, out Vector3 moveHitNormal);

                // perform step up recover
                if (didStepUp && !didStepUpRecover)
                {
                    didStepUp = false;
                    didStepUpRecover = true;

                    CapsuleMove(character.down * stepUpHeight, out RaycastHit stepUpRecoverHit, out Vector3 stepUpRecoverHitNormal);

                    if (stepUpRecoverHit.collider)
                    {
                        // if we cannot step on this _ground, revert the step up
                        // and continue the loop without stepping up this time
                        if (GroundCanStandOn(stepUpRecoverHit, stepUpRecoverHitNormal, out float baseAngle) == false)
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
                if (GroundSlideOnSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
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

                    stepUpHeight = CapsuleMove(_charUp * _currentStepUpHeight).magnitude;

                    continue;
                }

                // try sliding along the obstacle
                if (GroundSlideAlongSurface(originalMove, ref remainingMove, moveHit, moveHitNormal))
                {
                    continue;
                }

                // there's nothing we can do now, so stop the move
                remainingMove = Vector3.zero;
            }
        }

        GroundStepDown(originalMove);
        CapsuleResolvePenetration();

        Vector3 moved = _capsule.position - lastPosition;
        if (moveVMag == 0f)
        {
            moved = Vector3.ProjectOnPlane(moved, _charUp);
        }

        SetVelocityByMove(moved);
    }

    protected virtual void GroundResizeCapsule()
    {
        float weight = _movementStateWeight;
        float speed = 0;
        float targetHeight = 0;
        float targetRadius = 0;
        Vector3 targetCenter = Vector3.zero;

        if (MovementState.isGroundCrouching)
        {
            targetCenter = _groundCrouchCapsuleCenter;
            targetHeight = _groundCrouchCapsuleHeight;
            targetRadius = _groundCrouchCapsuleRadius;
            speed = _groundStandToCrouchTransitionSpeed * _deltaTime;
        }
        else
        {
            targetCenter = _groundStandCapsuleCenter;
            targetHeight = _groundStandCapsuleHeight;
            targetRadius = _groundStandCapsuleRadius;
            speed = _groundCrouchToStandTransitionSpeed * _deltaTime;
        }

        // charCapsule.localPosition += charCapsule.up * Mathf.MoveTowards(charCapsule.localHeight, targetHeight, speed);
        // _capsule.center = Vector3.Lerp(_capsule.center, targetCenter, speed);
        _capsule.height = Mathf.Lerp(_capsule.height, targetHeight, speed);
        _capsule.radius = Mathf.Lerp(_capsule.radius, targetRadius, speed);

        weight = Mathf.Lerp(weight, 1f, speed);
        _movementStateWeight = weight;
    }

    protected virtual bool GroundSlideOnSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (remainingMove == Vector3.zero)
            return false;

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle))
        {
            if (slopeAngle == 0f)
            {
                return false;
            }

            Plane plane = new Plane(hitNormal, hit.point);
            Ray ray = new Ray(hit.point + remainingMove, _charUp);
            plane.Raycast(ray, out float enter);

            Vector3 slopeMove = remainingMove + (_charUp * enter);

            if (_currentMaintainVelocityOnSurface == false)
            {
                slopeMove = slopeMove.normalized * remainingMove.magnitude;
            }

            remainingMove = slopeMove;
            return true;
        }

        return false;
    }

    protected virtual bool GroundSlideAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        float remainingMoveSize = remainingMove.magnitude;

        if (hit.collider is null || remainingMoveSize == 0f)
            return false;

        RecalculateNormalIfZero(hit, ref hitNormal);

        Vector3 hitProject = Vector3.ProjectOnPlane(hitNormal, _charUp);
        Vector3 slideMove = Vector3.ProjectOnPlane(originalMove.normalized * remainingMoveSize, hitProject);
        if (_currentMaintainVelocityAlongSurface)
        {
            // to avoid sliding along perpendicular surface for very small values,
            // may be a result of small miscalculations
            if (slideMove.magnitude > MIN_MOVE_ALONG_SURFACE_FOR_MAINTAIN_VELOCITY)
            {
                slideMove = slideMove.normalized * remainingMoveSize;
            }
        }

        remainingMove = slideMove;
        return true;
    }

    protected virtual bool GroundStepDown(Vector3 originalMove)
    {
        var verticalMove = Vector3.Project(originalMove, _charUp).magnitude;
        if (verticalMove != 0f || _currentStepDownDepth <= 0)
            return false;

        CapsuleResolvePenetration();

        var moved = CapsuleMove(character.down * _currentStepDownDepth, out RaycastHit hit, out Vector3 hitNormal);

        if (GroundCanStandOn(hit, hitNormal) == false)
        {
            _capsule.position -= moved;
            return false;
        }

        return true;
    }

    protected virtual bool GroundCheckIsGround(Collider collider)
    {
        if (collider is null)
        {
            return false;
        }

        return _groundLayer.Contains(collider.gameObject.layer);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit)
    {
        return GroundCanStandOn(hit, Vector3.zero);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit, Vector3 slopeNormal)
    {
        return GroundCanStandOn(hit, slopeNormal, out float slopeAngle);
    }

    protected virtual bool GroundCanStandOn(RaycastHit hit, Vector3 slopeNormal, out float slopeAngle)
    {
        slopeAngle = 0f;

        if (hit.collider)
        {
            if (GroundCheckIsGround(hit.collider) == false)
            {
                return false;
            }

            RecalculateNormalIfZero(hit, ref slopeNormal);

            float maxSlopeAngle = Math.Clamp(_currentSlopeUpAngle,
                MIN_GROUND_SLOPE_ANGLE, MAX_GROUND_SLOPE_ANGLE);

            slopeAngle = Vector3.Angle(_charUp, slopeNormal);

            if (FloatExtensions.IsInRange(slopeAngle, MIN_GROUND_SLOPE_ANGLE, maxSlopeAngle))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool GroundCast(float depth, out GroundResult result)
    {
        BaseSphereCast(character.down * depth, out RaycastHit hit, out Vector3 hitNormal);
        if (hitNormal == Vector3.zero)
        {
            hitNormal = hit.normal;
        }

        result = new GroundResult();

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle) == false)
        {
            if (slopeAngle < 90f)
            {
                return false;
            }
        }

        result.collider = hit.collider;
        result.direction = character.down;
        result.distance = hit.distance;

        result.angle = slopeAngle;

        result.basePosition = result.collider.transform.position;
        result.baseRotation = result.collider.transform.rotation;

        result.edgeDistance = default;

        return true;
    }

    protected virtual void GroundUpdateResult()
    {
        _groundPreviousResult = _groundResult;
        GroundCast(_groundCheckDepth, out _groundResult);
    }

    protected GroundResult _groundPreviousResult;
    protected GroundResult _groundResult;

    protected LayerMask _groundLayer;

    protected float _groundStandWalkSpeed;
    protected float _groundStandWalkAcceleration;
    protected float _groundStandRunSpeed;
    protected float _groundStandRunAcceleration;
    protected float _groundStandSprintSpeed;
    protected float _groundStandSprintAcceleration;
    protected float _groundStandSprintLeftAngleMax;
    protected float _groundStandSprintRightAngleMax;
    protected float _groundStandJumpForce;
    protected float _groundStandStepUpPercent;
    protected float _groundStandStepUpHeight => _capsule.height * _groundStandStepUpPercent / 100;
    protected float _groundStandStepDownPercent;
    protected float _groundStandStepDownDepth => _capsule.height * _groundStandStepDownPercent / 100;
    protected float _groundStandSlopeUpAngle;
    protected float _groundStandSlopeDownAngle;
    protected float _groundStandCapsuleHeight;
    protected float _groundStandCapsuleRadius;
    protected float _groundStandToCrouchTransitionSpeed;
    protected float _groundCrouchWalkSpeed;
    protected float _groundCrouchWalkAcceleration;
    protected float _groundCrouchRunSpeed;
    protected float _groundCrouchRunAcceleration;
    protected float _groundCrouchJumpForce;
    protected float _groundCrouchStepUpPercent;
    protected float _groundCrouchStepUpHeight => _capsule.height * _groundCrouchStepUpPercent / 100;
    protected float _groundCrouchStepDownPercent;
    protected float _groundCrouchStepDownDepth => _capsule.height * _groundCrouchStepUpPercent / 100;
    protected float _groundCrouchSlopeUpAngle;
    protected float _groundCrouchSlopeDownAngle;
    protected float _groundCrouchCapsuleHeight;
    protected float _groundCrouchCapsuleRadius;
    protected float _groundCrouchToStandTransitionSpeed;
    protected float _groundMinMoveDistance;
    protected float _groundCheckDepth;
    protected float _groundStandIdleAcceleration;
    protected float _groundStandIdleSpeed;
    protected float _groundCrouchIdleSpeed;
    protected float _groundCrouchIdleAcceleration;

    protected bool _groundStandMaintainVelocityOnSurface;
    protected bool _groundStandMaintainVelocityAlongSurface;
    protected bool _groundCrouchAutoRiseToStandSprint;
    protected bool _groundCrouchMaintainVelocityOnSurface;
    protected bool _groundCrouchMaintainVelocityAlongSurface;

    protected Vector3 _groundStandCapsuleCenter;
    protected Vector3 _groundCrouchCapsuleCenter;
    protected bool _canGround = true;
}