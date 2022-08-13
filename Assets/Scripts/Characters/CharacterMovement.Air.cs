using System;
using UnityEngine;

public partial class CharacterMovement : CharacterBehaviour
{
    protected const uint AIR_MAX_MOVE_ITERATIONS = 10;
    protected const float GRAVITY_MULTIPLIER = .05f;

    protected virtual void PhysAir()
    {
        AirCalculateValues();

        Vector3 charUp = _charUp;
        Vector3 charForward = character.forward;
        Vector3 charRight = character.right;
        float mass = character.mass;
        float gravitySpeed = _airGravityAcceleration * mass * _deltaTime * _deltaTime * GRAVITY_MULTIPLIER;

        Vector3 vel = _velocity * _deltaTime;
        Vector3 velH = Vector3.ProjectOnPlane(vel, charUp);
        Vector3 velV = vel - velH;

        Vector3 moveV = velV + (charUp * gravitySpeed);
        Vector3 moveH = velH;

        // Vector3 moveHX = Vector3.ProjectOnPlane(moveH, charForward);
        // Vector3 moveHZ = moveH - moveHX;
        // // processed move input
        // Vector3 moveInputRaw = _move;
        // Vector3 moveInput = new Vector3(moveInputRaw.x, 0f, moveInputRaw.y);
        // moveInput = Quaternion.Euler(0f, _charView.turnAngle, 0f) * moveInput;
        // moveInput = character.rotation * moveInput;

        // // helping movement in _air
        // Vector3 move_help_h = _currentMoveSpeed * moveInput * _deltaTime;
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
        if (_jump && _currentJumpCount < _airMaxJumpCount)
        {
            _currentJumpCount++;

            if (_currentMaintainVelocityOnJump == false)
            {
                moveV = Vector3.zero;
            }

            moveV = charUp * _currentJumpPower;
        }

        Vector3 move = moveH + moveV;

        AirMove(move);
    }

    protected virtual void AirCalculateValues()
    {
        _currentMoveAccel = _airMoveAcceleration;
        _currentMoveSpeed = _airMoveSpeed;
        _currentJumpPower = _airJumpPower;
        _currentMaxJumpCount = _airMaxJumpCount;
        _currentMinMoveDist = _airMinMoveDistance;

        // TODO: add this field in data asset
        _currentMaintainVelocityOnJump = false;
    }

    protected virtual void AirMove(Vector3 move)
    {
        Vector3 lastPosition = _capsule.position;
        Vector3 remainingMove = move;

        for (int i = 0; i < AIR_MAX_MOVE_ITERATIONS; i++)
        {
            remainingMove -= CapsuleMove(remainingMove, out RaycastHit hit, out Vector3 hitNormal);

            if (hit.collider == null)
            {
                // no collision, so end the move
                remainingMove = Vector3.zero;
                break;
            }

            AirMoveAlongSurface(move, ref remainingMove, hit, hitNormal);
        }

        Vector3 moved = _capsule.position - lastPosition;
        SetVelocityByMove(moved);
    }

    protected virtual void AirMoveAlongSurface(Vector3 originalMove, ref Vector3 remainingMove, RaycastHit hit, Vector3 hitNormal)
    {
        if (hit.collider == null || remainingMove == global::UnityEngine.Vector3.zero)
            return;

        RecalculateNormalIfZero(hit, ref hitNormal);

        if (GroundCanStandOn(hit, hitNormal, out float slopeAngle))
        {
            remainingMove = Vector3.zero;
            _canGround = true;
            return;
        }

        // hit.normal gives normal respective to capsule's body,
        // useful for sliding off on corners
        Vector3 slideMove = Vector3.ProjectOnPlane(remainingMove, hit.normal);
        if (_currentMaintainVelocityAlongSurface)
        {
            slideMove = slideMove.normalized * remainingMove.magnitude;
        }

        remainingMove = slideMove;
    }

    protected float _airGravityAcceleration;
    protected float _airGravityMaxSpeed;
    protected float _airMinMoveDistance;
    protected float _airMoveSpeed;
    protected float _airMoveAcceleration;
    protected float _airJumpPower;

    protected uint _airMaxJumpCount;
}